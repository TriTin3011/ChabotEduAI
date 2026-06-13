using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Threading.Tasks;
using BussinessLayer.DTOs;
using BussinessLayer.Services;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;
using Microsoft.AspNetCore.SignalR;
using PresentationLayer.Hubs;

namespace PresentationLayer.Pages.Admin
{
    public class SubjectsModel : PageModel
    {
        private readonly ISubjectService _subjectService;
        private readonly IUserService _userService;
        private readonly IHubContext<CourseHub> _hubContext;

        public SubjectsModel(ISubjectService subjectService, IUserService userService, IHubContext<CourseHub> hubContext)
        {
            _subjectService = subjectService;
            _userService = userService;
            _hubContext = hubContext;
        }

        public IEnumerable<SubjectDto> Subjects { get; set; } = new List<SubjectDto>();
        public SelectList Lecturers { get; set; }

        [BindProperty]
        public string NewCode { get; set; } = string.Empty;
        [BindProperty]
        public string NewName { get; set; } = string.Empty;
        [BindProperty]
        public int? NewLecturerId { get; set; }

        [BindProperty]
        public int UpdateId { get; set; }
        [BindProperty]
        public string UpdateCode { get; set; } = string.Empty;
        [BindProperty]
        public string UpdateName { get; set; } = string.Empty;
        [BindProperty]
        public int? UpdateLecturerId { get; set; }

        public async Task OnGetAsync()
        {
            Subjects = await _subjectService.GetAllSubjectsAsync(true); // Include deleted
            var lecturers = await _userService.GetLecturersAsync();
            Lecturers = new SelectList(lecturers, "Id", "Username");
        }

        public async Task<IActionResult> OnPostAddSubjectAsync()
        {
            if (!string.IsNullOrWhiteSpace(NewCode) && !string.IsNullOrWhiteSpace(NewName))
            {
                await _subjectService.AddSubjectAsync(NewCode, NewName, NewLecturerId);
                await _hubContext.Clients.All.SendAsync("CourseChanged");
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateSubjectAsync()
        {
            if (UpdateId > 0 && !string.IsNullOrWhiteSpace(UpdateCode) && !string.IsNullOrWhiteSpace(UpdateName))
            {
                await _subjectService.UpdateSubjectAsync(UpdateId, UpdateCode, UpdateName, UpdateLecturerId);
                await _hubContext.Clients.All.SendAsync("CourseChanged");
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteSubjectAsync(int id)
        {
            await _subjectService.SoftDeleteSubjectAsync(id);
            await _hubContext.Clients.All.SendAsync("CourseChanged");
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRestoreSubjectAsync(int id)
        {
            await _subjectService.RestoreSubjectAsync(id);
            await _hubContext.Clients.All.SendAsync("CourseChanged");
            return RedirectToPage();
        }
    }
}
