using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace BussinessLayer.Services
{
    public class GeminiService : IGeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public GeminiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            var key = Environment.GetEnvironmentVariable("GEMINI_API_KEY") 
                      ?? configuration["GeminiAI:ApiKey"];
            
            _apiKey = key?.Trim() ?? throw new ArgumentNullException("GEMINI_API_KEY is not configured");
        }

        public async Task<float[]> GetEmbeddingAsync(string text)
        {
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-embedding-001:embedContent?key={_apiKey}";
            
            var requestBody = new
            {
                model = "models/gemini-embedding-001",
                content = new
                {
                    parts = new[]
                    {
                        new { text = text }
                    }
                }
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync(url, jsonContent);
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(responseString);

            var values = result.GetProperty("embedding").GetProperty("values").EnumerateArray()
                .Select(x => x.GetSingle())
                .ToArray();

            return values;
        }

        public async Task<List<float[]>> GetEmbeddingsAsync(List<string> texts)
        {
            // For simplicity, we just loop. In production, we might use batchEmbedContents
            var results = new List<float[]>();
            foreach (var text in texts)
            {
                results.Add(await GetEmbeddingAsync(text));
            }
            return results;
        }

        public async Task<string> GenerateAnswerAsync(string prompt)
        {
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-flash-latest:generateContent?key={_apiKey}";

            var requestBody = new
            {
                contents = new[] { new { parts = new[] { new { text = prompt } } } }
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, jsonContent);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                return $"Lỗi khi gọi AI: {response.StatusCode} - {errorBody}";
            }

            var responseString = await response.Content.ReadAsStringAsync();
            using var result = JsonDocument.Parse(responseString);
            var candidates = result.RootElement.GetProperty("candidates");
            if (candidates.GetArrayLength() == 0)
            {
                return string.Empty;
            }

            var parts = candidates[0].GetProperty("content").GetProperty("parts");
            if (parts.GetArrayLength() == 0)
            {
                return string.Empty;
            }

            return parts[0].GetProperty("text").GetString() ?? string.Empty;
        }
    }
}
