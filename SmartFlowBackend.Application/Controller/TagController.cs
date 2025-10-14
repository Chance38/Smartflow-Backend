using Microsoft.AspNetCore.Mvc;
using Middleware;
using Domain.Contract;
using Domain.Interface;

namespace Application.Controller
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
            var userId = ServiceMiddleware.GetUserId(HttpContext);
            _logger.LogInformation("Received request to add tag for user: {UserId}", userId);

            try
            {
                await _tagService.AddTagAsync(userId, req.Tag);
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
            var userId = ServiceMiddleware.GetUserId(HttpContext);
            _logger.LogInformation("Received request to get all tags for user: {UserId}", userId);

            var tags = await _tagService.GetAllTagsAsync(userId);

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
            var userId = ServiceMiddleware.GetUserId(HttpContext);
            _logger.LogInformation("Received request to delete tag '{TagName}' for user: {UserId}", req.Tag.Name, userId);

            try
            {
                await _tagService.DeleteTagAsync(userId, req.Tag);
                _logger.LogInformation("Deleted tag '{TagName}' successfully", req.Tag.Name);
            }
            catch (ArgumentException)
            {
                return Ok(new OkSituation
                {
                    RequestId = requestId
                });
            }

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
            var userId = ServiceMiddleware.GetUserId(HttpContext);
            _logger.LogInformation("Received request to update tag for user: {UserId}", userId);

            try
            {
                await _tagService.UpdateTagAsync(userId, req.OldTag, req.NewTag);
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
