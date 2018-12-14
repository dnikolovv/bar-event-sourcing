using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Bar.Web.Controllers
{
    public class BaseController : Controller
    {
        protected IActionResult Ok(Unit unit) =>
            Ok();

        protected IActionResult Error<TError>(TError error) =>
            new BadRequestObjectResult(error);
    }
}
