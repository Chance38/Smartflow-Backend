using Microsoft.AspNetCore.Mvc;
using Middleware;
using SmartFlowBackend.Application.SwaggerSetting;
using SmartFlowBackend.Domain.Contracts;
using SmartFlowBackend.Domain.Interfaces;

namespace SmartFlowBackend.Application.Controller
{
    [ApiController]
    [Route("smartflow/v1")]
    public class TagController : ControllerBase
    {
        private readonly ITagService _tagService;
        private readonly ILogger<TagController> _logger;

        public TagController(ITagService tagService, ILogger<TagController> logger)
        {
            _tagService = tagService;
            _logger = logger;
        }

        [HttpPost("tag")]
        [ProducesResponseType(typeof(OkSituation), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ClientErrorSituation), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ServerErrorSituation), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddTag([FromBody] AddTagRequest req)
        {
            var requestId = ServiceMiddleware.GetRequestId(HttpContext);

            var userId = TestUser.Id;
            _logger.LogInformation("Received request to add tag for user: {UserId}", userId);

            try
            {
                await _tagService.AddTagAsync(req, userId);
                _logger.LogInformation("Create tag Successfully");

            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ClientErrorSituation
                {
                    RequestId = requestId,
                    ErrorMessage = ex.Message
                });
            }

            return Ok(new OkSituation
            {
                RequestId = requestId
            });
        }

        [HttpGet("tags")]
        [ProducesResponseType(typeof(GetAllTagsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ClientErrorSituation), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ServerErrorSituation), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllTags()
        {
            var requestId = ServiceMiddleware.GetRequestId(HttpContext);

            var userId = TestUser.Id;
            _logger.LogInformation("Received request to get all tags for user: {UserId}", userId);

            var tags = new List<Tag>();
            try
            {
                tags = await _tagService.GetAllTagsByUserIdAsync(userId);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ClientErrorSituation
                {
                    RequestId = requestId,
                    ErrorMessage = ex.Message
                });
            }

            return Ok(new GetAllTagsResponse
            {
                RequestId = requestId,
                Tags = tags
            });
        }

        [HttpDelete("tag")]
        [ProducesResponseType(typeof(OkSituation), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ServerErrorSituation), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteTag([FromBody] DeleteTagRequest req)
        {
            var requestId = ServiceMiddleware.GetRequestId(HttpContext);

            var userId = TestUser.Id;
            _logger.LogInformation("Received request to delete tag '{TagName}' for user: {UserId}", req.TagName, userId);

            await _tagService.DeleteTagAsync(userId, req.TagName);

            return Ok(new OkSituation
            {
                RequestId = requestId
            });

        }

        [HttpPut("tag")]
        [ProducesResponseType(typeof(OkSituation), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ClientErrorSituation), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ServerErrorSituation), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateTag([FromBody] UpdateTagRequest req)
        {
            var requestId = ServiceMiddleware.GetRequestId(HttpContext);

            var userId = TestUser.Id;
            _logger.LogInformation("Received request to update tag for user: {UserId}", userId);

            try
            {
                await _tagService.UpdateTagAsync(req, userId);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ClientErrorSituation
                {
                    RequestId = requestId,
                    ErrorMessage = ex.Message
                });
            }

            return Ok(new OkSituation
            {
                RequestId = requestId
            });
        }
    }
}
