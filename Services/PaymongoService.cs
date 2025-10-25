using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

public class PayMongoService
{
    private readonly HttpClient _client;
    private readonly string _secretKey;

    public PayMongoService(IConfiguration configuration)
    {
        _secretKey = configuration["PayMongo:SecretKey"];
        _client = new HttpClient();
        var base64Key = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_secretKey}:"));
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64Key);
    }

    public async Task<string?> CreateCheckout(decimal amount, string referenceNumber)
    {
        var payload = new
        {
            data = new
            {
                attributes = new
                {
                    amount = (int)(amount * 100),
                    currency = "PHP",
                    description = $"Payment for Billing {referenceNumber}",
                    redirect = new
                    {
                        success = "https://localhost:5206/Billing/PaymentSuccess",
                        failed = "https://localhost:5206/Billing/PaymentFailed"
                    }
                }
            }
        };

        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("https://api.paymongo.com/v1/links", content);

        if (!response.IsSuccessStatusCode)
            return null;

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("data").GetProperty("attributes").GetProperty("checkout_url").GetString();
    }
}
