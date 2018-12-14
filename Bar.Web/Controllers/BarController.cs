using Bar.CQRS.Commands.Bar;
using Bar.CQRS.Queries.Bar;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Bar.Web.Controllers
{
    [Route("api/[controller]")]
    public class BarController : BaseController
    {
        private readonly IMediator _mediator;

        public BarController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("beverages")]
        public async Task<IActionResult> GetInStockBeverages() =>
            (await _mediator.Send(new GetInStockBeverages()))
                .Match(Ok, Error);

        [HttpPost("beverages")]
        public async Task<IActionResult> AddBeverages([FromBody] AddBeveragesToMenu request) =>
            (await _mediator.Send(request))
                .Match(Ok, Error);
    }
}
