using Microsoft.AspNetCore.Mvc;
using SmartFlowBackend.Application.Contracts;
using SmartFlowBackend.Domain.Entities;
using SmartFlowBackend.Domain.Interfaces;
using Middleware;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace SmartFlowBackend.Controller
{
    [ApiController]
    [Route("smartflow/v1/tag")]
    public class TagController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<TagController> _logger;

        public TagController(IUnitOfWork unitOfWork, ILogger<TagController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> AddTag([FromBody] AddTagRequest req)
        {
            var requestId = ServiceMiddleware.GetRequestId(HttpContext);
            _logger.LogInformation("Received request to add tag");
            try
            {
                var tag = new Tag { Id = Guid.NewGuid(), Name = req.Name };
                await _unitOfWork.Tags.AddAsync(tag);
                await _unitOfWork.SaveAsync();
                _logger.LogInformation("Create tag Successfully");
                return Ok(new { requestId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while creating a tag.");
                return StatusCode(500, new { requestId, errorMessage = "An unexpected error occurred." });
            }
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetAllTagsByUserId([FromRoute(Name = "userId")] Guid userId)
        {
            var requestId = ServiceMiddleware.GetRequestId(HttpContext);
            try
            {
                var tags = await _unitOfWork.Tags.GetAllTagsByUserIdAsync(userId);
                var tagDtos = tags.Select(t => new TagDto { Id = t.Id, Name = t.Name }).ToList();
                return Ok(new { requestId, tags = tagDtos });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while retrieving tags.");
                return StatusCode(500, new { requestId, errorMessage = "An unexpected error occurred." });
            }
        }
    }
}
