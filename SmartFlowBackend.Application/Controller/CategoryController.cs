using Microsoft.AspNetCore.Mvc;
using Middleware;
using Domain.Contract;
using Domain.Interface;

namespace Application.Controller
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
        [ProducesResponseType(typeof(ClientErrorSituation), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ServerErrorSituation), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddCategory([FromBody] AddCategoryRequest req)
        {
            var requestId = ServiceMiddleware.GetRequestId(HttpContext);
            var userId = ServiceMiddleware.GetUserId(HttpContext);
            _logger.LogInformation("Received request to add category for user: {UserId}", userId);

            try
            {
                await _categoryService.AddCategoryAsync(userId, req.Category);
                _logger.LogInformation("Create category Successfully");
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

        [HttpGet("categories")]
        [ProducesResponseType(typeof(GetAllCategoriesResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ClientErrorSituation), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ServerErrorSituation), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllCategories()
        {
            var requestId = ServiceMiddleware.GetRequestId(HttpContext);
            var userId = ServiceMiddleware.GetUserId(HttpContext);
            _logger.LogInformation("Received request to get all categories for user: {UserId}", userId);

            var categories = new List<Category>();
            try
            {
                categories = await _categoryService.GetAllCategoriesAsync(userId);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "There is something wrong, this path shouldn't occur.");
                return BadRequest(new ClientErrorSituation
                {
                    RequestId = requestId,
                    ErrorMessage = ex.Message
                });
            }

            return Ok(new GetAllCategoriesResponse
            {
                RequestId = requestId,
                Categories = categories
            });
        }

        [HttpDelete("category")]
        [ProducesResponseType(typeof(OkSituation), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ServerErrorSituation), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteCategory([FromBody] DeleteCategoryRequest req)
        {
            var requestId = ServiceMiddleware.GetRequestId(HttpContext);
            var userId = ServiceMiddleware.GetUserId(HttpContext);
            _logger.LogInformation("Received request to delete category for user: {UserId}", userId);

            try
            {
                _logger.LogInformation("Deleted category name: '{CategoryName}', type: '{CategoryType}' successfully", req.Category.Name, req.Category.Type);
                await _categoryService.DeleteCategoryAsync(userId, req.Category);
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

        [HttpPut("category")]
        [ProducesResponseType(typeof(OkSituation), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ClientErrorSituation), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ServerErrorSituation), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateCategory([FromBody] UpdateCategoryRequest req)
        {
            var requestId = ServiceMiddleware.GetRequestId(HttpContext);
            var userId = ServiceMiddleware.GetUserId(HttpContext);
            _logger.LogInformation("Received request to update category for user: {UserId}", userId);

            try
            {
                await _categoryService.UpdateCategoryAsync(userId, req.OldCategory, req.NewCategory);
                _logger.LogInformation("Updated category successfully");
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