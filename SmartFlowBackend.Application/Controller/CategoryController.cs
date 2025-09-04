using Microsoft.AspNetCore.Mvc;
using SmartFlowBackend.Domain.Interfaces;
using Middleware;
using SmartFlowBackend.Domain.Contracts;
using SmartFlowBackend.Domain;

namespace SmartFlowBackend.Application.Controller
{
    [Route("api/v1/categories")]
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

        [HttpPost]
        public async Task<IActionResult> AddCategory([FromBody] AddCategoryRequest req)
        {
            var requestId = ServiceMiddleware.GetRequestId(HttpContext);
            _logger.LogInformation("Received request to add category");

            try
            {
                await _categoryService.AddCategoryAsync(req);

                _logger.LogInformation("Create category Successfully");
                return Ok(new
                {
                    requestId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category");
                return StatusCode(500, new
                {
                    requestId,
                    errorMessage = "An unexpected error occurred"
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCategoriesByUserId()
        {
            var requestId = ServiceMiddleware.GetRequestId(HttpContext);
            _logger.LogInformation("Received request to get all categories for user: {UserId}", TestUser.Id);

            try
            {
                var categories = await _categoryService.GetAllCategoriesByUserIdAsync();

                return Ok(new
                {
                    requestId,
                    categories
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting categories");
                return StatusCode(500, new { requestId, message = "An error occurred" });
            }
        }

        // [HttpDelete]
        // public async Task<IActionResult> DeleteCategory([FromBody] DeleteCategoryRequest req)
        // {
        //     var requestId = ServiceMiddleware.GetRequestId(HttpContext);


        // }
    }
}