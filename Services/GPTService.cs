using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ChatGPTApp.Services
{
    public class GPTService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public GPTService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _apiKey = "YOUR-API-KEY-HERE";
        }

        public async Task<string> GetGPTResponse(List<(string Role, string Content)> conversationHistory)
        {
            try
            {
                foreach (var msg in conversationHistory)
                {
                    Console.WriteLine($"Role: {msg.Role}, Content: {msg.Content}");
                    if (msg.Role == "bot")
                    {
                        throw new Exception("Found 'bot' role, which is not valid.");
                    }
                }

                
                var requestBody = new
                {
                    model = "gpt-4",
                    messages = conversationHistory.Select(msg => new { role = msg.Role, content = msg.Content })
                };

                var jsonRequestBody = JsonConvert.SerializeObject(requestBody);

                Console.WriteLine("Request Body: " + jsonRequestBody);

                var content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);

                var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);

                Console.WriteLine("Response Status Code: " + (int)response.StatusCode);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Error Response: " + errorContent);
                    return $"Error: {response.StatusCode}. Details: {errorContent}";
                }

                var responseString = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Response String: " + responseString);

                dynamic jsonResponse = JsonConvert.DeserializeObject(responseString);
                return jsonResponse.choices[0].message.content.ToString();
            }
            catch (HttpRequestException ex)
            {

                Console.WriteLine($"Request error: {ex.Message}");
                return "An error occurred while processing your request. Please try again.";
            }
            catch (Exception ex)
            {

                Console.WriteLine($"General error in GetGPTResponse: {ex.Message}");
                return "An unexpected error occurred. Please try again.";
            }
        }
    }
}


