using DShop2024.Services.ChatboxAI;
using Microsoft.AspNetCore.Mvc;
using OpenAI.Chat;

namespace DShop2024.Controllers
{
    //[Route("api/chatbox")]
    public class ChatboxController : Controller
    {
        private readonly ChatBotService _chatService;

        public ChatboxController(ChatBotService chatService)
        {
            _chatService = chatService;
        }

        [HttpPost]
        public async Task<IActionResult> Ask([FromBody] ChatMessage msg)
        {
            var reply = await _chatService.AskAsync(msg.Message);
            return Ok(new { reply });
        }

    }

}
public class ChatMessage
{
    public string Message { get; set; }
}