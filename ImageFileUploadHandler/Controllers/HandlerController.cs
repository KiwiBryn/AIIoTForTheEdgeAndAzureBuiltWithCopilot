using Microsoft.AspNetCore.Mvc;

namespace ImageFileUploadHandler.Controllers
{
    public class HandlerController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
