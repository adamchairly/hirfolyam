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
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status500InternalServerError)]
    public async Task<GetHotTopicsResponse> GetAsync([FromQuery] GetHotTopicsQuery query, CancellationToken cancellationToken)
    {
        return await sender.Send(query, cancellationToken);
    }

    [HttpGet("{id:long}")]
    [ProducesResponseType<GetHotTopicResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status500InternalServerError)]
    public async Task<GetHotTopicResponse> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        return await sender.Send(new GetHotTopicQuery { Id = id }, cancellationToken);
    }
}
