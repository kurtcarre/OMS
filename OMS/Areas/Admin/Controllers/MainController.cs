using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OMS.Auth;

namespace OMS.Admin.Controllers
{
    [Area("Admin")]
    public class MainController : Controller
    {
        public ActionResult Index()
        {
            ViewData["Active"] = "dashboard";
            return View();
        }
    }
}