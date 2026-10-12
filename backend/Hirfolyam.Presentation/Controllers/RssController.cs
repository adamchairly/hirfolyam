using Hirfolyam.Application.Features;
using Hirfolyam.Application.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Hirfolyam.Presentation.Controllers;

[ApiController]
[Route("api/rss")]
public sealed class RssController(ISender sender) : ControllerBase
{
    [HttpGet]
    [ResponseCache(Duration = 300)]
    [ProducesResponseType<string>(StatusCodes.Status200OK, "application/xml")]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status500InternalServerError)]
    public async Task<ContentResult> GetAsync(CancellationToken cancellationToken)
    {
        var feed = await sender.Send(new GetRssFeedQuery { SiteUrl = $"{Request.Scheme}://{Request.Host}/" }, cancellationToken);

        // Browsers show application/rss+xml as plain text, but render application/xml as a document tree.
        // Feed readers recognize the feed by its content either way.
        return Content(feed, "application/xml; charset=utf-8");
    }
}
