using System.Text;
using System.Text.Json;

namespace EmployeesMVC_Core_8.Services.WhatsApp
{
    public class WhatsAppService : IWhatsAppService
    {
        private readonly HttpClient _httpClient;

        public WhatsAppService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        
        public async Task SendMessage(string phone, string messageText)
        {
            var url = "https://graph.facebook.com/v22.0/306078622595857/messages";

            var payload = new
            {
                messaging_product = "whatsapp",
                to = phone,
                type = "template",
                template = new
                {
                    name = "hello_world",
                    language = new
                    {
                        code = "en_US"
                    }
                }
            };

            var json = JsonSerializer.Serialize(payload);

            var request = new HttpRequestMessage(HttpMethod.Post, url);

            request.Headers.Add("Authorization", "Bearer EAAYongQ3mTQBQ3ZCqG5QNjvzlrzupTQRnMUvQYrZC2ioQoQdYGJyRzY8ztyGzAVuyZCGr5OBpSZBHZCC08xjoStCSSZCAQnGCv16OR0pYCCkmWso9uon2QsvIMgKMQfZCnEm2OLdsBTmijktWXQIDvXzaFNjTgyYPlBkQWwV60lzFzUsVIMlmvhDjW4ZCP74y8adgrdwNFkUcIW1MFjGrwn2ooR0k3vjaDXPbf7iGiqUJqEa6kS6hZAyoXGeqH3AvHRCQ62iUbAwDRibDV3uRjc0WTNG0");

            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);

            var result = await response.Content.ReadAsStringAsync();

            Console.WriteLine(result);
        }
    }
}
