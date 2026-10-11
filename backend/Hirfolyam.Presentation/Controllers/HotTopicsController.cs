using Hirfolyam.Application.Features;
using Hirfolyam.Application.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Hirfolyam.Presentation.Controllers;

[ApiController]
[Route("api/hot-topics")]
public sealed class HotTopicsController(ISender sender) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<GetHotTopicsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status500InternalServerError)]
    public async Task<GetHotTopicsResponse> GetAsync(CancellationToken cancellationToken)
    {
        return await sender.Send(new GetHotTopicsQuery(), cancellationToken);
    }
}
