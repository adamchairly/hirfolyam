using Hirfolyam.Application.Features;
using Hirfolyam.Application.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Hirfolyam.Presentation.Controllers;

[ApiController]
[Route("api/articles")]
public sealed class ArticlesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<PagedResult<ArticleListItemResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<BaseResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<BaseResponse>(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType<BaseResponse>(StatusCodes.Status500InternalServerError)]
    public async Task<PagedResult<ArticleListItemResponse>> GetAsync([FromQuery] GetArticlesQuery query, CancellationToken cancellationToken)
    {
        return await mediator.Send(query, cancellationToken);
    }
}
