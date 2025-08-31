using Contracts.Category;
using Microsoft.AspNetCore.Mvc;
using Middleware;

namespace Controller.Category;

[ApiController]
[Route("smartflow/v1/category")]
public class CategoryController : ControllerBase
{
    public CategoryController()
    {
    }

    [HttpPost("{userId}")]
    public async Task<IActionResult> AddCategory([FromRoute(Name = "userId")] string userId, [FromBody] AddCategoryRequest req)
    {
        var requestId = ServiceMiddleware.GetRequestId(HttpContext);

        return Ok(new
        {
            RequestId = requestId
        });
    }
}