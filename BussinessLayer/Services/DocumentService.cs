using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BussinessLayer.DTOs;
using DataAccessLayer.Repositories;

namespace BussinessLayer.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly IGeminiService _geminiService;

        public DocumentService(IDocumentRepository documentRepository, IGeminiService geminiService)
        {
            _documentRepository = documentRepository;
            _geminiService = geminiService;
        }

        public async Task<IEnumerable<DocumentDto>> GetAllDocumentsAsync()
        {
            var documents = await _documentRepository.GetAllDocumentsAsync();
            return documents.Select(d => new DocumentDto
            {
                Id = d.Id,
                Title = d.Title,
                FileType = d.FileType,
                Status = Enum.Parse<DocumentStatus>(d.Status),
                UploadedAt = d.UploadedAt,
                FileUrl = d.FileUrl,
                SubjectId = d.SubjectId,
                ChapterId = d.ChapterId,
                SubjectName = d.Subject?.Name,
                ChapterTitle = d.Chapter?.Title,
                UploaderId = d.UploaderId,
                UploaderName = d.Uploader?.Username
            }).ToList();
        }

        public async Task<IEnumerable<DocumentDto>> GetDocumentsBySubjectAsync(int subjectId)
        {
            var documents = await _documentRepository.GetAllDocumentsAsync(); // Should add a specialized repo method
            return documents.Where(d => d.SubjectId == subjectId).Select(d => new DocumentDto
            {
                Id = d.Id,
                Title = d.Title,
                FileType = d.FileType,
                Status = Enum.Parse<DocumentStatus>(d.Status),
                UploadedAt = d.UploadedAt,
                FileUrl = d.FileUrl,
                SubjectId = d.SubjectId,
                ChapterId = d.ChapterId,
                UploaderId = d.UploaderId,
                UploaderName = d.Uploader?.Username
            }).ToList();
        }

        public async Task<IEnumerable<DocumentDto>> GetDocumentsByChapterAsync(int chapterId)
        {
            var documents = await _documentRepository.GetAllDocumentsAsync(); 
            return documents.Where(d => d.ChapterId == chapterId).Select(d => new DocumentDto
            {
                Id = d.Id,
                Title = d.Title,
                FileType = d.FileType,
                Status = Enum.Parse<DocumentStatus>(d.Status),
                UploadedAt = d.UploadedAt,
                FileUrl = d.FileUrl,
                SubjectId = d.SubjectId,
                ChapterId = d.ChapterId,
                UploaderId = d.UploaderId,
                UploaderName = d.Uploader?.Username
            }).ToList();
        }

        public async Task<IEnumerable<DocumentDto>> GetDocumentsByUploaderAsync(int uploaderId)
        {
            var documents = await _documentRepository.GetAllDocumentsAsync(); // In real app, filter in DB
            return documents.Where(d => d.UploaderId == uploaderId).Select(d => new DocumentDto
            {
                Id = d.Id,
                Title = d.Title,
                FileType = d.FileType,
                Status = Enum.Parse<DocumentStatus>(d.Status),
                UploadedAt = d.UploadedAt,
                FileUrl = d.FileUrl,
                SubjectId = d.SubjectId,
                ChapterId = d.ChapterId,
                SubjectName = d.Subject?.Name,
                ChapterTitle = d.Chapter?.Title,
                UploaderId = d.UploaderId,
                UploaderName = d.Uploader?.Username
            }).ToList();
        }

        public async Task<DocumentDto?> GetDocumentByIdAsync(int id)
        {
            // Use the version that includes Uploader navigation
            var doc = await _documentRepository.GetDocumentByIdWithUploaderAsync(id);
            if (doc == null) return null;

            return new DocumentDto
            {
                Id = doc.Id,
                Title = doc.Title,
                FileType = doc.FileType,
                FileUrl = doc.FileUrl,
                Content = doc.Content,
                Status = Enum.Parse<DocumentStatus>(doc.Status),
                UploadedAt = doc.UploadedAt,
                SubjectId = doc.SubjectId,
                ChapterId = doc.ChapterId,
                UploaderId = doc.UploaderId,
                UploaderName = doc.Uploader?.Username,
                SubjectName = doc.Subject?.Name,
                ChapterTitle = doc.Chapter?.Title
            };
        }

        public async Task<string> GetDocumentTextAsync(int id)
        {
            var doc = await _documentRepository.GetDocumentByIdAsync(id);
            return doc?.Content ?? string.Empty;
        }

        public async Task<int> AddDocumentAsync(string title, string fileType, string fileUrl, int? subjectId, int? chapterId, int? uploaderId)
        {
            return await AddDocumentAsync(title, fileType, fileUrl, subjectId, chapterId, uploaderId, null);
        }

        public async Task<int> AddDocumentAsync(string title, string fileType, string fileUrl, int? subjectId, int? chapterId, int? uploaderId, string? extractedContent)
        {
            var document = new DataAccessLayer.Entities.Document
            {
                Title = title,
                FileType = fileType,
                FileUrl = fileUrl,
                SubjectId = subjectId,
                ChapterId = chapterId,
                UploaderId = uploaderId,
                Status = "Indexed",
                UploadedAt = System.DateTime.UtcNow,
                Content = extractedContent ?? string.Empty
            };
            await _documentRepository.AddDocumentAsync(document);
            return document.Id;
        }

        public async Task<bool> ProcessDocumentAsync(int documentId, string extractedContent)
        {
            var doc = await _documentRepository.GetDocumentByIdAsync(documentId);
            if (doc == null) return false;
            doc.Content = extractedContent;
            doc.Status = "Indexed";
            // await _documentRepository.UpdateDocumentAsync(doc); // Needs Update method
            return true;
        }

        public async Task<bool> ProcessDocumentEmbeddingAsync(int documentId)
        {
            var doc = await _documentRepository.GetDocumentByIdAsync(documentId);
            if (doc == null || string.IsNullOrWhiteSpace(doc.Content)) return false;

            // Simple chunking strategy: split by 200 words approx
            var words = doc.Content.Split(new[] { ' ', '\r', '\n', '\t' }, System.StringSplitOptions.RemoveEmptyEntries);
            int chunkSize = 200;
            var chunks = new List<DataAccessLayer.Entities.DocumentChunk>();
            int orderIndex = 1;

            for (int i = 0; i < words.Length; i += chunkSize)
            {
                int length = System.Math.Min(chunkSize, words.Length - i);
                var chunkWords = new string[length];
                System.Array.Copy(words, i, chunkWords, 0, length);
                var chunkText = string.Join(" ", chunkWords);

                // Call Gemini for embedding
                var vector = await _geminiService.GetEmbeddingAsync(chunkText);

                chunks.Add(new DataAccessLayer.Entities.DocumentChunk
                {
                    DocumentId = documentId,
                    Content = chunkText,
                    Embedding = new Pgvector.Vector(vector),
                    OrderIndex = orderIndex++
                });
            }

            if (chunks.Any())
            {
                await _documentRepository.AddDocumentChunksAsync(chunks);
            }
            return true;
        }

        public async Task<bool> DeleteDocumentAsync(int id)
        {
            return await _documentRepository.DeleteDocumentAsync(id);
        }
    }
}
