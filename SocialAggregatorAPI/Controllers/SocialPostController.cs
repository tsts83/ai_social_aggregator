namespace Namespace.SocialAggregatorAPI.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "ApiKeyOrJwt")]

public class SocialPosterController : ControllerBase
{
    private readonly IMiniSocialPosterService _posterService;

    public SocialPosterController(IMiniSocialPosterService posterService)
    {
        _posterService = posterService;
    }

    [HttpPost("trigger")]
    public async Task<IActionResult> TriggerPostAsync()
    {
        var result = await _posterService.PostNextUnpostedArticleAsync();
        if (result.Contains("Article posted"))
        {
            return Ok(result); // Article posted successfully
        }
        else
        {
            return StatusCode(400, result); // Something went wrong (e.g., posting failed)
        }
    }
}
