using Hirfolyam.Application.Features;
using Hirfolyam.Application.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Hirfolyam.Presentation.Controllers;

[ApiController]
[Route("api/sources")]
public sealed class SourcesController(ISender sender) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<GetSourcesResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status500InternalServerError)]
    public async Task<GetSourcesResponse> GetAsync(CancellationToken cancellationToken)
    {
        return await sender.Send(new GetSourcesQuery(), cancellationToken);
    }
}
