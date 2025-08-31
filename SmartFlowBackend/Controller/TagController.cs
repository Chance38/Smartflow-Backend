using Contracts.Tag;
using Microsoft.AspNetCore.Mvc;
using Middleware;

namespace Controller.Tag;

[ApiController]
[Route("smartflow/v1/tag")]
public class TagController : ControllerBase
{
    public TagController()
    {
    }

    [HttpPost("{userId}")]
    public async Task<IActionResult> AddTag([FromRoute(Name = "userId")] string userId, [FromBody] AddTagRequest req)
    {
        var requestId = ServiceMiddleware.GetRequestId(HttpContext);

        return Ok(new
        {
            RequestId = requestId
        });
    }
}