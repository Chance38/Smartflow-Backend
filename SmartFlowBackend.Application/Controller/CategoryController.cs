using Microsoft.AspNetCore.Mvc;
using SmartFlowBackend.Application.Contracts;
using SmartFlowBackend.Domain.Entities;
using SmartFlowBackend.Domain.Interfaces;
using Middleware;

namespace SmartFlowBackend.Application.Controller
{
    [Route("api/v1/categories")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CategoryController> _logger;

        public CategoryController(IUnitOfWork unitOfWork, ILogger<CategoryController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> AddCategory([FromBody] AddCategoryRequest req)
        {
            var requestId = ServiceMiddleware.GetRequestId(HttpContext);
            _logger.LogInformation("Received request to add category");

            try
            {
                var userId = TestUser.Id;

                var category = new Domain.Entities.Category
                {
                    Id = Guid.NewGuid(),
                    Name = req.Name,
                    Type = req.Type,
                    UserId = userId
                };

                await _unitOfWork.Categories.AddAsync(category);
                await _unitOfWork.SaveAsync();

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
                var userId = TestUser.Id;
                var categories = await _unitOfWork.Categories.GetAllCategoriesByUserIdAsync(userId);

                var Categories = categories
                    .Select(c => new Contracts.Category
                    {
                        Name = c.Name,
                        Type = c.Type
                    }).ToList();

                return Ok(new
                {
                    requestId,
                    categories = Categories
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