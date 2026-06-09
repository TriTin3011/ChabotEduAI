using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BussinessLayer.DTOs;
using BussinessLayer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PresentationLayer.Pages.Lecturer
{
    [Authorize(Roles = "Lecturer")]
    public class MyDocumentsModel : PageModel
    {
        private readonly IDocumentService _documentService;

        public MyDocumentsModel(IDocumentService documentService)
        {
            _documentService = documentService;
        }

        public IEnumerable<DocumentDto> Documents { get; set; } = new List<DocumentDto>();

        public async Task OnGetAsync()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
            {
                Documents = await _documentService.GetDocumentsByUploaderAsync(userId);
            }
        }
    }
}
