using Hirfolyam.Application.Features;
using Hirfolyam.Application.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Hirfolyam.Presentation.Controllers;

[ApiController]
[Route("api/articles")]
public sealed class ArticlesController(ISender sender) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<PagedResult<ArticleListItemResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status500InternalServerError)]
    public async Task<PagedResult<ArticleListItemResponse>> GetAsync([FromQuery] GetArticlesQuery query, CancellationToken cancellationToken)
    {
        return await sender.Send(query, cancellationToken);
    }
}
