using Microsoft.AspNetCore.Mvc;

public class TestController : Controller
{
    public IActionResult DoctorLayoutPreview()
    {
        return View();
    }
}
