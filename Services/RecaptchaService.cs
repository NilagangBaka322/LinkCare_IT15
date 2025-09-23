using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LinkCare_IT15.Services  // Matches the using statement
{
    public class RecaptchaResponse
    {
        public bool Success { get; set; }
        public double? Score { get; set; }
        public string[] ErrorCodes { get; set; } = Array.Empty<string>();
    }

    public class RecaptchaService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<RecaptchaService> _logger;

        public RecaptchaService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<RecaptchaService> logger)
        {
            _httpClient = httpClientFactory.CreateClient();
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<RecaptchaResponse> VerifyTokenAsync(string token)
        {
            _logger.LogInformation("Verifying reCAPTCHA token at {Time}", DateTime.UtcNow);
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("No token provided for reCAPTCHA verification");
                return new RecaptchaResponse { Success = false, ErrorCodes = new[] { "missing-input-response" } };
            }

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("secret", _configuration["ReCaptchaSettings:SecretKey"]),
                new KeyValuePair<string, string>("response", token)
            });

            try
            {
                var response = await _httpClient.PostAsync("https://www.google.com/recaptcha/api/siteverify", content);
                var json = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("reCAPTCHA response received at {Time}, Status: {Status}, Response: {Json}", DateTime.UtcNow, response.StatusCode, json);

                var result = JsonSerializer.Deserialize<RecaptchaResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (result == null)
                {
                    result = new RecaptchaResponse { Success = false, ErrorCodes = new[] { "parse-error" } };
                }

                result.Success = result.Success && result.Score >= 0.5; // Minimum score threshold
                _logger.LogInformation("reCAPTCHA verification completed: Success={Success}, Score={Score}, ErrorCodes={ErrorCodes}", result.Success, result.Score, string.Join(", ", result.ErrorCodes));

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during reCAPTCHA verification");
                return new RecaptchaResponse { Success = false, ErrorCodes = new[] { "exception", ex.Message } };
            }
        }
    }
}