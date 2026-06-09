using System.Collections.Generic;
using System.Threading.Tasks;

namespace BussinessLayer.Services
{
    public interface IGeminiService
    {
        Task<float[]> GetEmbeddingAsync(string text);
        Task<List<float[]>> GetEmbeddingsAsync(List<string> texts);
        Task<string> GenerateAnswerAsync(string prompt);
    }
}
