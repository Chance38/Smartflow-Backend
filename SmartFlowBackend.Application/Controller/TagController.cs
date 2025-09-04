using Microsoft.AspNetCore.Mvc;
using SmartFlowBackend.Domain.Interfaces;
using Middleware;
using SmartFlowBackend.Domain.Contracts;

namespace SmartFlowBackend.Application.Controller
{
    [ApiController]
    [Route("smartflow/v1/tag")]
    public class TagController : ControllerBase
    {
        private readonly ITagService _tagService;
        private readonly ILogger<TagController> _logger;

        public TagController(ITagService tagService, ILogger<TagController> logger)
        {
            _tagService = tagService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> AddTag([FromBody] AddTagRequest req)
        {
            var requestId = ServiceMiddleware.GetRequestId(HttpContext);
            _logger.LogInformation("Received request to add tag");

            try
            {
                await _tagService.AddTagAsync(req);
                _logger.LogInformation("Create tag Successfully");
                return Ok(new
                {
                    requestId
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Error creating tag: {Message}", ex.Message);
                return BadRequest(new
                {
                    requestId,
                    errorMessage = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while creating a tag.");
                return StatusCode(500, new
                {
                    requestId,
                    errorMessage = "An unexpected error occurred."
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllTagsByUserId()
        {
            var requestId = ServiceMiddleware.GetRequestId(HttpContext);
            try
            {
                var tags = await _tagService.GetAllTagsByUserIdAsync();
                return Ok(new { requestId, tags });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while retrieving tags.");
                return StatusCode(500, new { requestId, errorMessage = "An unexpected error occurred." });
            }
        }
    }
}
