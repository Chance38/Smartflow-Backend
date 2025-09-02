using SmartFlowBackend.Application.Contracts;
using SmartFlowBackend.Domain.Entities;
using SmartFlowBackend.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Middleware;
using Microsoft.EntityFrameworkCore;

namespace SmartFlowBackend.Api.Controller;

[ApiController]
[Route("smartflow/v1/category")]
public class CategoryController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CategoryController> _logger;

    public CategoryController(IUnitOfWork unitOfWork, ILogger<CategoryController> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    [HttpPost("{userId}")]
    public async Task<IActionResult> AddCategory([FromRoute(Name = "userId")] Guid userId, [FromBody] AddCategoryRequest req)
    {
        _logger.LogInformation("Received request to add category for user {UserId}", userId);
        var requestId = ServiceMiddleware.GetRequestId(HttpContext);
        try
        {
            var category = new Category
            {
                Id = Guid.NewGuid(),
                Name = req.Name,
                Type = req.Type,
                UserId = userId
            };
            await _unitOfWork.Categories.AddAsync(category);
            await _unitOfWork.SaveAsync();
            _logger.LogInformation("Create category Successfully");
            return Ok(new { requestId });
        }
        catch (ArgumentOutOfRangeException)
        {
            _logger.LogWarning("Invalid category type");
            return BadRequest(new { requestId, errorMessage = "Invalid category type" });
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error occurred while creating a category for user {UserId}", userId);
            return StatusCode(500, new { requestId, errorMessage = $"Database error occurred while creating category, {ex.Message}" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while creating a category for user {UserId}", userId);
            return StatusCode(500, new { requestId, errorMessage = "An unexpected error occurred." });
        }
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetAllCategoriesByUserId([FromRoute(Name = "userId")] Guid userId)
    {
        var requestId = ServiceMiddleware.GetRequestId(HttpContext);
        var categories = await _unitOfWork.Categories.GetAllCategoriesByUserIdAsync(userId);
        return Ok(new { requestId, categories });
    }
}