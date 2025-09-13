using Microsoft.AspNetCore.Mvc;
using Middleware;
using SmartFlowBackend.Application.SwaggerSetting;
using SmartFlowBackend.Domain.Contracts;
using SmartFlowBackend.Domain.Interfaces;

namespace SmartFlowBackend.Application.Controller
{
    [ApiController]
    [Route("smartflow/v1")]
    public class RecordTemplateController : ControllerBase
    {
        private readonly IRecordTemplateService _recordTemplateService;
        private readonly ILogger<RecordTemplateController> _logger;

        public RecordTemplateController(
            IRecordTemplateService recordTemplateService,
            ILogger<RecordTemplateController> logger)
        {
            _recordTemplateService = recordTemplateService;
            _logger = logger;
        }

        [HttpPost("record-template")]
        [ProducesResponseType(typeof(OkSituation), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ClientErrorSituation), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ServerErrorSituation), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddRecordTemplate([FromBody] AddRecordTemplateRequest req)
        {
            var requestId = ServiceMiddleware.GetRequestId(HttpContext);

            var userId = TestUser.Id;
            _logger.LogInformation("Received request to add record for user {UserId}", userId);

            try
            {
                await _recordTemplateService.AddRecordTemplateAsync(req, userId);
                _logger.LogInformation("Create record template Successfully");
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

        [HttpGet("record-templates")]
        [ProducesResponseType(typeof(GetAllRecordTemplatesResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ClientErrorSituation), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ServerErrorSituation), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllRecordTemplates()
        {
            var requestId = ServiceMiddleware.GetRequestId(HttpContext);

            var userId = TestUser.Id;
            _logger.LogInformation("Received request to get all record templates for user {UserId}", userId);

            try
            {
                var recordTemplates = await _recordTemplateService.GetAllRecordTemplatesAsync(userId);
                return Ok(new GetAllRecordTemplatesResponse
                {
                    RequestId = requestId,
                    RecordTemplates = recordTemplates
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ClientErrorSituation
                {
                    RequestId = requestId,
                    ErrorMessage = ex.Message
                });
            }
        }

        [HttpDelete("record-template")]
        [ProducesResponseType(typeof(OkSituation), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ServerErrorSituation), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteRecordTemplate([FromBody] DeleteRecordTemplateRequest req)
        {
            var requestId = ServiceMiddleware.GetRequestId(HttpContext);

            var userId = TestUser.Id;
            _logger.LogInformation("Received request to delete record template for user {UserId}", userId);


            await _recordTemplateService.DeleteRecordTemplateAsync(req, userId);
            _logger.LogInformation("Delete record template '{RecordTemplateName}' Successfully", req.RecordTemplateName);

            return Ok(new OkSituation
            {
                RequestId = requestId
            });
        }
    }
}
