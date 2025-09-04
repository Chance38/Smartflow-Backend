using Microsoft.AspNetCore.Mvc;
using SmartFlowBackend.Domain.Interfaces;
using Middleware;
using SmartFlowBackend.Domain.Contracts;
using SmartFlowBackend.Domain;

namespace SmartFlowBackend.Application.Controller
{
    [Route("smartflow/v1")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly ILogger<CategoryController> _logger;

        public CategoryController(ICategoryService categoryService, ILogger<CategoryController> logger)
        {
            _categoryService = categoryService;
            _logger = logger;
        }

        [HttpPost("category")]
        [ProducesResponseType(typeof(OkSituation), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ClientErrorSituation), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ServerErrorSituation), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddCategory([FromBody] AddCategoryRequest req)
        {
            var requestId = ServiceMiddleware.GetRequestId(HttpContext);

            var userId = TestUser.Id;
            _logger.LogInformation("Received request to add category for user: {UserId}", userId);

            try
            {
                await _categoryService.AddCategoryAsync(req, userId);
                _logger.LogInformation("Create category Successfully");
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

        [HttpGet("categories")]
        [ProducesResponseType(typeof(List<Category>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ClientErrorSituation), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ServerErrorSituation), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllCategories()
        {
            var requestId = ServiceMiddleware.GetRequestId(HttpContext);

            var userId = TestUser.Id;
            _logger.LogInformation("Received request to get all categories for user: {UserId}", userId);

            try
            {
                var categories = await _categoryService.GetAllCategoriesByUserIdAsync(userId);

                return Ok(new
                {
                    requestId,
                    categories
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

        // [HttpDelete]
        // public async Task<IActionResult> DeleteCategory([FromBody] DeleteCategoryRequest req)
        // {
        //     var requestId = ServiceMiddleware.GetRequestId(HttpContext);


        // }
    }
}