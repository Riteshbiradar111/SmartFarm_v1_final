using Microsoft.AspNetCore.Mvc;
using Smart_Farm_and_Crop_Yeild_Management_System.Models;
//my first change
namespace Smart_Farm_and_Crop_Yeild_Management_System.Controllers
{
    public class HomeController : Controller
    {
        // GET: /Home/Index
        public IActionResult Index()
        {
            return View();
        }

        // GET: /Home/Error
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}
