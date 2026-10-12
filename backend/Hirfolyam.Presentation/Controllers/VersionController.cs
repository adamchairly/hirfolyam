using Hirfolyam.Application.Features;
using Hirfolyam.Application.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Hirfolyam.Presentation.Controllers;

[ApiController]
[Route("api/version")]
public sealed class VersionController(ISender sender) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<GetVersionResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status500InternalServerError)]
    public async Task<GetVersionResponse> GetAsync(CancellationToken cancellationToken)
    {
        return await sender.Send(new GetVersionQuery(), cancellationToken);
    }
}
