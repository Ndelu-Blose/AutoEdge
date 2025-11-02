using Microsoft.AspNetCore.Mvc;
using AutoEdge.Services;

namespace AutoEdge.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IAIAssistantService _ai;

        public ChatController(IAIAssistantService ai)
        {
            _ai = ai;
        }

        [HttpPost("ask")]
        public async Task<IActionResult> Ask([FromBody] ChatRequest req)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(req.Message))
                    return BadRequest(new { reply = "Please enter your question." });

                var reply = await _ai.GetReplyAsync(req.Message);
                return Ok(new { reply });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { reply = $"Error processing request: {ex.Message}" });
            }
        }

        public class ChatRequest
        {
            public string Message { get; set; } = string.Empty;
        }
    }
}
