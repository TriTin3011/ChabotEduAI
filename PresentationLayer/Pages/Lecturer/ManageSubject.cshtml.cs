using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;
using BussinessLayer.DTOs;
using BussinessLayer.Services;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using PresentationLayer.Hubs;

namespace PresentationLayer.Pages.Lecturer
{
    public class ManageSubjectModel : PageModel
    {
        private readonly ISubjectService _subjectService;
        private readonly IDocumentService _documentService;
        private readonly IFileTextExtractorService _textExtractor;
        private readonly IHubContext<CourseHub> _hubContext;

        public ManageSubjectModel(ISubjectService subjectService, IDocumentService documentService, IFileTextExtractorService textExtractor, IHubContext<CourseHub> hubContext)
        {
            _subjectService = subjectService;
            _documentService = documentService;
            _textExtractor = textExtractor;
            _hubContext = hubContext;
        }

        public SubjectDto Subject { get; set; } = default!;

        [BindProperty]
        public string NewChapterTitle { get; set; } = string.Empty;

        [BindProperty]
        public int UpdateChapterId { get; set; }

        [BindProperty]
        public string UpdateChapterTitle { get; set; } = string.Empty;

        [BindProperty]
        public int? UploadChapterId { get; set; }
        
        [BindProperty]
        public string UploadTitle { get; set; } = string.Empty;
        
        [BindProperty]
        public IFormFile? UploadFile { get; set; }

        public bool IsOwner { get; set; } = false;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Subject = await _subjectService.GetSubjectByIdAsync(id);
            if (Subject == null) return NotFound();

            // Lấy LecturerId từ claims
            var userIdStr = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value ?? User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdStr, out int userId))
            {
                if (Subject.LecturerId == userId || User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value == "Admin")
                {
                    IsOwner = true;
                }
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAddChapterAsync(int id)
        {
            if (!string.IsNullOrWhiteSpace(NewChapterTitle))
            {
                var subject = await _subjectService.GetSubjectByIdAsync(id);
                int order = (subject?.Chapters?.Count ?? 0) + 1;
                await _subjectService.AddChapterAsync(id, NewChapterTitle, order);
                await _hubContext.Clients.All.SendAsync("CourseChanged");
            }
            return RedirectToPage(new { id = id });
        }

        public async Task<IActionResult> OnPostUpdateChapterAsync(int id)
        {
            if (UpdateChapterId > 0 && !string.IsNullOrWhiteSpace(UpdateChapterTitle))
            {
                await _subjectService.UpdateChapterAsync(UpdateChapterId, UpdateChapterTitle);
                await _hubContext.Clients.All.SendAsync("CourseChanged");
            }
            return RedirectToPage(new { id = id });
        }

        [BindProperty]
        public string DeleteChapterOption { get; set; } = "delete";

        public async Task<IActionResult> OnPostDeleteChapterAsync(int id, int chapterId)
        {
            bool keepDocuments = DeleteChapterOption == "keep";
            await _subjectService.DeleteChapterWithOptionsAsync(chapterId, keepDocuments);
            await _hubContext.Clients.All.SendAsync("CourseChanged");
            return RedirectToPage(new { id = id });
        }

        public async Task<IActionResult> OnPostUploadFileAsync(int id)
        {
            if (UploadFile != null && UploadFile.Length > 0 && !string.IsNullOrWhiteSpace(UploadTitle))
            {
                var filesDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "files");
                Directory.CreateDirectory(filesDir);
                var filePath = Path.Combine(filesDir, UploadFile.FileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await UploadFile.CopyToAsync(stream);
                }

                var fileUrl = $"/files/{UploadFile.FileName}";
                var fileType = Path.GetExtension(UploadFile.FileName).TrimStart('.').ToLower();

                // Use FileTextExtractorService (supports txt, md, csv, pdf via PdfPig)
                var extractedContent = _textExtractor.ExtractText(filePath);

                int? uploaderId = null;
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value ?? User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim != null && int.TryParse(userIdClaim, out var uId))
                {
                    uploaderId = uId;
                }

                var documentId = await _documentService.AddDocumentAsync(UploadTitle, fileType, fileUrl, id, UploadChapterId, uploaderId, extractedContent);
                if (documentId > 0)
                {
                    // Tự động băm và nhúng ngay lập tức
                    await _documentService.ProcessDocumentEmbeddingAsync(documentId);
                }
                await _hubContext.Clients.All.SendAsync("CourseChanged");
            }

            return RedirectToPage(new { id = id });
        }

        [BindProperty]
        public int MoveDocumentId { get; set; }

        [BindProperty]
        public int? MoveToChapterId { get; set; }

        public async Task<IActionResult> OnPostMoveDocumentAsync(int id)
        {
            if (MoveDocumentId > 0)
            {
                await _documentService.UpdateDocumentChapterAsync(MoveDocumentId, MoveToChapterId);
                await _hubContext.Clients.All.SendAsync("CourseChanged");
            }
            return RedirectToPage(new { id = id });
        }

        public async Task<IActionResult> OnPostDeleteDocumentAsync(int id, int docId)
        {
            await _documentService.DeleteDocumentAsync(docId);
            await _hubContext.Clients.All.SendAsync("CourseChanged");
            return RedirectToPage(new { id = id });
        }
    }
}
