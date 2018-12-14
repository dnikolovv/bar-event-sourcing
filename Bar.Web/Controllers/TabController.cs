using Bar.CQRS.Commands;
using Bar.CQRS.Commands.Tab;
using Bar.CQRS.Queries.Tab;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bar.Web.Controllers
{
    [Route("api/[controller]")]
    public class TabController : BaseController
    {
        private readonly IMediator _mediator;

        public TabController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("close/{id}")]
        public async Task<IActionResult> CloseTab(Guid id, [FromBody]decimal amountPaid) =>
            (await _mediator.Send(new CloseTab { TabId = id, AmountPaid = amountPaid }))
                .Match(Ok, Error);

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id) =>
            (await _mediator.Send(new GetTabView { Id = id }))
                .Match(Ok, Error);

        [HttpPost("open/{id}")]
        public async Task<IActionResult> OpenTab(Guid id, [FromBody] string clientName) =>
            (await _mediator.Send(new OpenTab { TabId = id, ClientName = clientName }))
                .Match(Ok, Error);

        [HttpPost("order/{id}")]
        public async Task<IActionResult> OrderBeverages(Guid id, [FromBody]List<int> menuNumbers) =>
            (await _mediator.Send(new OrderBeverages { TabId = id, MenuNumbers = menuNumbers }))
                .Match(Ok, Error);

        [HttpPost("serve/{id}")]
        public async Task<IActionResult> ServeBeverages(Guid id, [FromBody]List<int> menuNumbers) =>
            (await _mediator.Send(new ServeBeverages { TabId = id, MenuNumbers = menuNumbers }))
                .Match(Ok, Error);
    }
}