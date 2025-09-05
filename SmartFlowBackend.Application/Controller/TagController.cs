using Microsoft.AspNetCore.Mvc;
using SmartFlowBackend.Domain.Interfaces;
using Middleware;
using SmartFlowBackend.Domain.Contracts;
using SmartFlowBackend.Domain;

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
                return NotFound(new ClientErrorSituation
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
        [ProducesResponseType(typeof(List<Tag>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ClientErrorSituation), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ServerErrorSituation), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllTags()
        {
            var requestId = ServiceMiddleware.GetRequestId(HttpContext);

            var userId = TestUser.Id;
            _logger.LogInformation("Received request to get all tags for user: {UserId}", userId);

            try
            {
                var tags = await _tagService.GetAllTagsByUserIdAsync(userId);

                return Ok(new
                {
                    requestId,
                    tags
                });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new ClientErrorSituation
                {
                    RequestId = requestId,
                    ErrorMessage = ex.Message
                });
            }
        }

        [HttpDelete("tag/{tagName}")]
        [ProducesResponseType(typeof(OkSituation), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ClientErrorSituation), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ServerErrorSituation), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteTag([FromBody] DeleteTagRequest req)
        {
            var requestId = ServiceMiddleware.GetRequestId(HttpContext);

            var userId = TestUser.Id;
            _logger.LogInformation("Received request to delete tag '{TagName}' for user: {UserId}", req.Name, userId);

            try
            {
                await _tagService.DeleteTagAsync(userId, req.Name);
                _logger.LogInformation("Delete tag '{TagName}' successfully", req.Name);

                return Ok(new OkSituation
                {
                    RequestId = requestId
                });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new ClientErrorSituation
                {
                    RequestId = requestId,
                    ErrorMessage = ex.Message
                });
            }
        }

        [HttpPut("tag")]
        [ProducesResponseType(typeof(OkSituation), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ClientErrorSituation), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ServerErrorSituation), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateTag([FromBody] UpdateTagRequest req)
        {
            var requestId = ServiceMiddleware.GetRequestId(HttpContext);

            var userId = TestUser.Id;
            _logger.LogInformation("Received request to update tag for user: {UserId}", userId);

            try
            {
                await _tagService.UpdateTagAsync(req, userId);
                _logger.LogInformation("Update tag Successfully");
            }
            catch (ArgumentException ex)
            {
                return NotFound(new ClientErrorSituation
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
