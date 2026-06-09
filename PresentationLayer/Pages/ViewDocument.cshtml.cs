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

        public ViewDocumentModel(IDocumentService documentService, DataAccessLayer.Repositories.IDocumentRepository documentRepository)
        {
            _documentService = documentService;
            _documentRepository = documentRepository;
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

            // Chỉ load chunks cho Admin và Lecturer
            var role = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value;
            if (role != "Student")
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
            // Chỉ Admin và Lecturer mới được thực hiện embedding
            var role = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value;
            if (role == "Student")
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
