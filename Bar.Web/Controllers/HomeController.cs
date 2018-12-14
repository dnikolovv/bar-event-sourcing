using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Bar.Web.Controllers
{
    public class HomeController : Controller
    {
        public async Task<IActionResult> Index() =>
            View();
    }
}
