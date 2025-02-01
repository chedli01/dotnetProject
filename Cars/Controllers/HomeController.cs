using Cars.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Cars.Services;


namespace Cars.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

          [HttpPost]
        public IActionResult SendMessage(ContactFormModel model)
        {
            if (ModelState.IsValid)
            {
                // Create an instance of EmailService without DI
                var emailService = new EmailService();
                emailService.SendEmail(model.Name, model.Email, model.Subject, model.Message);
                return RedirectToAction("Success");
            }

            return View("Index");
        }

        public IActionResult Success()
        {
            return View();
        }
    }
}