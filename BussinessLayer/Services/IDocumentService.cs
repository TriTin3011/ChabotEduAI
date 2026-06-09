using System.Collections.Generic;
using System.Threading.Tasks;
using BussinessLayer.DTOs;

namespace BussinessLayer.Services
{
    public interface IDocumentService
    {
        Task<IEnumerable<DocumentDto>> GetAllDocumentsAsync();
        Task<IEnumerable<DocumentDto>> GetDocumentsBySubjectAsync(int subjectId);
        Task<IEnumerable<DocumentDto>> GetDocumentsByChapterAsync(int chapterId);
        Task<IEnumerable<DocumentDto>> GetDocumentsByUploaderAsync(int uploaderId);
        Task<DocumentDto?> GetDocumentByIdAsync(int id);
        Task<string> GetDocumentTextAsync(int id);
        Task<bool> AddDocumentAsync(string title, string fileType, string fileUrl, int? subjectId, int? chapterId, int? uploaderId);
        Task<bool> AddDocumentAsync(string title, string fileType, string fileUrl, int? subjectId, int? chapterId, int? uploaderId, string? extractedContent);
        Task<bool> ProcessDocumentAsync(int documentId, string extractedContent);
        Task<bool> ProcessDocumentEmbeddingAsync(int documentId);
        Task<bool> DeleteDocumentAsync(int id);
    }
}
