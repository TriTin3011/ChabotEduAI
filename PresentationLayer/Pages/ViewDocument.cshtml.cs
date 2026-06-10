using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;
using System.Linq;
using BussinessLayer.DTOs;
using BussinessLayer.Services;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System;

namespace PresentationLayer.Pages
{
    [Authorize]
    public class ViewDocumentModel : PageModel
    {
        private readonly IDocumentService _documentService;
        private readonly DataAccessLayer.Repositories.IDocumentRepository _documentRepository;
        private readonly ISubjectService _subjectService;

        public ViewDocumentModel(IDocumentService documentService, DataAccessLayer.Repositories.IDocumentRepository documentRepository, ISubjectService subjectService)
        {
            _documentService = documentService;
            _documentRepository = documentRepository;
            _subjectService = subjectService;
        }

        public DocumentDto Document { get; set; } = new DocumentDto();
        public string TextContent { get; set; } = string.Empty;
        public List<string> SimulatedChunks { get; set; } = new List<string>();

        [TempData]
        public string? SuccessMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var doc = await _documentService.GetDocumentByIdAsync(id);
            if (doc == null)
            {
                return NotFound();
            }

            Document = doc;
            TextContent = doc.Content ?? await _documentService.GetDocumentTextAsync(id);
            if (string.IsNullOrWhiteSpace(TextContent))
                TextContent = "Không có nội dung dạng text cho file này.";

            var role = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value;
            
            bool canViewChunks = false;
            if (role == "Admin")
            {
                canViewChunks = true;
            }
            else if (role == "Lecturer" && doc.SubjectId.HasValue)
            {
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
                if (int.TryParse(userIdClaim, out int userId))
                {
                    var subject = await _subjectService.GetSubjectByIdAsync(doc.SubjectId.Value);
                    if (subject != null && subject.LecturerId == userId)
                    {
                        canViewChunks = true;
                    }
                }
            }

            if (canViewChunks)
            {
                var dbChunks = await _documentRepository.GetDocumentChunksAsync(id);
                if (dbChunks != null && dbChunks.Any())
                {
                    SimulatedChunks = dbChunks.OrderBy(c => c.OrderIndex).Select(c => c.Content).ToList();
                }
            }

            return Page();
        }

        public async Task<IActionResult> OnPostProcessEmbeddingAsync(int id)
        {
            var doc = await _documentService.GetDocumentByIdAsync(id);
            if (doc == null) return NotFound();

            var role = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value;
            
            bool canProcess = false;
            if (role == "Admin")
            {
                canProcess = true;
            }
            else if (role == "Lecturer" && doc.SubjectId.HasValue)
            {
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
                if (int.TryParse(userIdClaim, out int userId))
                {
                    var subject = await _subjectService.GetSubjectByIdAsync(doc.SubjectId.Value);
                    if (subject != null && subject.LecturerId == userId)
                    {
                        canProcess = true;
                    }
                }
            }

            if (!canProcess)
            {
                return Forbid();
            }

            var result = await _documentService.ProcessDocumentEmbeddingAsync(id);
            if (result)
            {
                SuccessMessage = "Băm và Nhúng Vector thành công!";
            }
            return RedirectToPage(new { id = id });
        }
    }
}
