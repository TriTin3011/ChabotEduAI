using System.Collections.Generic;
using System.Threading.Tasks;
using BussinessLayer.DTOs;
using BussinessLayer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using PresentationLayer.Hubs;

namespace PresentationLayer.Pages.Lecturer
{
    [Authorize(Roles = "Lecturer")]
    public class AllSubjectsModel : PageModel
    {
        private readonly ISubjectService _subjectService;
        private readonly IHubContext<CourseHub> _hubContext;

        public AllSubjectsModel(ISubjectService subjectService, IHubContext<CourseHub> hubContext)
        {
            _subjectService = subjectService;
            _hubContext = hubContext;
        }

        public IEnumerable<SubjectDto> Subjects { get; set; } = new List<SubjectDto>();

        [BindProperty]
        public string NewCode { get; set; } = "";
        [BindProperty]
        public string NewName { get; set; } = "";

        public async Task OnGetAsync()
        {
            // GetAllSubjectsAsync(false) để không hiển thị môn đã xóa
            Subjects = await _subjectService.GetAllSubjectsAsync(false);
        }

        public async Task<IActionResult> OnPostAddSubjectAsync()
        {
            if (!string.IsNullOrWhiteSpace(NewCode) && !string.IsNullOrWhiteSpace(NewName))
            {
                // Assign to current lecturer implicitly? Or leave unassigned. Let's say leave unassigned or just null since they create it for the system.
                await _subjectService.AddSubjectAsync(NewCode, NewName, null);
                
                // Notify clients
                await _hubContext.Clients.All.SendAsync("CourseChanged");
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteSubjectAsync(int id)
        {
            await _subjectService.SoftDeleteSubjectAsync(id);
            
            // Notify clients
            await _hubContext.Clients.All.SendAsync("CourseChanged");
            
            return RedirectToPage();
        }
    }
}
