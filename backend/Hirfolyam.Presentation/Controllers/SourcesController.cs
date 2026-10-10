using Hirfolyam.Application.Features;
using Hirfolyam.Application.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Hirfolyam.Presentation.Controllers;

[ApiController]
[Route("api/sources")]
public sealed class SourcesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<GetSourcesResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<BaseResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<BaseResponse>(StatusCodes.Status500InternalServerError)]
    public async Task<GetSourcesResponse> GetAsync(CancellationToken cancellationToken)
    {
        return await mediator.Send(new GetSourcesQuery(), cancellationToken);
    }
}
