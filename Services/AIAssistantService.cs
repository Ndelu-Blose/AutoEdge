using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text;

namespace AutoEdge.Services
{
    public class AIAssistantService : IAIAssistantService
    {
        private readonly HttpClient _http;
        private readonly AISettings _settings;

        public AIAssistantService(HttpClient http, IOptions<AISettings> settings)
        {
            _http = http;
            _settings = settings.Value;
            _http.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.ApiKey}");
             _http.DefaultRequestHeaders.Add("HTTP-Referer", "https://autoedge.com");
            _http.DefaultRequestHeaders.Add("X-Title", "AutoEdge Chat");
        }

        public async Task<string> GetReplyAsync(string userMessage)
        {
            try
            {
                var systemMessage = @"You are the AutoEdge AI Assistant. AutoEdge is a South African car dealership that sells new and pre-owned vehicles.
We offer:
- Car sales, financing, and trade-ins
- Vehicle servicing and maintenance
- Bookings for test drives and service appointments
- A recruitment program for mechanics, sales reps, and customer service roles
- Online assessments and interviews for applicants

When users ask about jobs, guide them to our recruitment portal.
When they ask about cars or service, assist them politely.
Provide helpful, concise responses. Do not include any tags or tokens like [OUT], [IN], etc. in your responses.";

                var request = new
                {
                    model = _settings.Model,
                    messages = new[]
                    {
                        new { role = "system", content = systemMessage },
                        new { role = "user", content = userMessage }
                    },
                    temperature = _settings.Temperature,
                    max_tokens = _settings.MaxTokens
                };

                var response = await _http.PostAsJsonAsync(_settings.BaseUrl, request);
                var responseText = await response.Content.ReadAsStringAsync();
                
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"AI API Error: {response.StatusCode} - {responseText}");
                }

                var json = JsonDocument.Parse(responseText).RootElement;

                // return json.GetProperty("choices")[0]
                //            .GetProperty("message")
                //            .GetProperty("content")
                //            .GetString() ?? "I'm sorry, I couldn't process that request.";
                var content = json.GetProperty("choices")[0]
                           .GetProperty("message")
                           .GetProperty("content")
                           .GetString() ?? "I'm sorry, I couldn't process that request.";

                // Clean up any tags or unwanted tokens
                content = content.Replace("[OUT]", "").Replace("[IN]", "").Trim();
                
                return content;
                
            }
            catch (Exception ex)
            {
                return "I apologize, but I encountered an error. Please try again or contact our support team.";
            }
        }
    }
}
