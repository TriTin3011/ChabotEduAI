using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;
using BussinessLayer.Services;
using System.Linq;

namespace PresentationLayer.Pages.Admin
{
    public class DashboardModel : PageModel
    {
        private readonly IUserService _userService;
        private readonly ISubjectService _subjectService;
        private readonly IDocumentService _documentService;

        public int TotalUsers { get; set; }
        public int TotalSubjects { get; set; }
        public int TotalDocuments { get; set; }

        public DashboardModel(IUserService userService, ISubjectService subjectService, IDocumentService documentService)
        {
            _userService = userService;
            _subjectService = subjectService;
            _documentService = documentService;
        }

        public async Task OnGetAsync()
        {
            var users = await _userService.GetAllUsersAsync(false);
            TotalUsers = users.Count();

            var subjects = await _subjectService.GetAllSubjectsAsync(false);
            TotalSubjects = subjects.Count();

            var documents = await _documentService.GetAllDocumentsAsync();
            TotalDocuments = documents.Count();
        }
    }
}
