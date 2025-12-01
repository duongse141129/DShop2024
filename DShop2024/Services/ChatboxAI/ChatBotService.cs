using Microsoft.EntityFrameworkCore;
using OpenAI;
using OpenAI.Chat;

namespace DShop2024.Services.ChatboxAI
{
    public class ChatBotService
    {
        //private readonly OpenAIClient _client;
        private readonly ChatClient _chatClient;
        private readonly DShopContext _context;

        public ChatBotService(DShopContext context)
        {
            //_client = new OpenAIClient(Environment.GetEnvironmentVariable("OPENAI_API_KEY"));
            _context = context;
            _chatClient = new(model: "gpt-4o", apiKey: "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");
        }

        public async Task<string> AskAsync(string userMessage)
        {
            var products = await _context.Products.Where(p => p.Status > 0 && p.Stock > 0).Include(p => p.Ratings).ToListAsync();
            var categories = await _context.Categories.Where(p => p.Status > 0).ToListAsync();
            var brands = await _context.Brands.Where(p => p.Status > 0).ToListAsync();

            var systemPrompt = @$"
                Bạn là chatbot tư vấn bán balo.

                Dữ liệu sản phẩm:
                {System.Text.Json.JsonSerializer.Serialize(products)}

                Dữ liệu danh mục:
                {System.Text.Json.JsonSerializer.Serialize(categories)}

                Dữ liệu thương hiệu:
                {System.Text.Json.JsonSerializer.Serialize(brands)}

                Nhiệm vụ:
                - Nếu user đưa tên sản phẩm → tìm trong Products → trả về thông tin.
                - Nếu user đưa nhu cầu (đi học, du lịch, leo núi, phượt, laptop, …) → gợi ý danh mục + sản phẩm phù hợp.
                - Nếu không tìm thấy sản phẩm → xin lỗi lịch sự.
                ";
            try
            {
                ChatCompletion completion = await _chatClient.CompleteChatAsync(userMessage);
                return completion.Content[0].Text;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }


            //var response = await _client.ChatCompletions.CreateAsync(
            //    model: "gpt-4.1-mini",
            //    messages: [
            //        new(role: "system", content: systemPrompt),
            //    new(role: "user", content: userMessage)
            //    ]
            //);

            //return response.Content[0].Text;
        }
    }
}
