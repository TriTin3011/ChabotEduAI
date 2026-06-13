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

        public async Task OnGetAsync()
        {
            // GetAllSubjectsAsync(false) để không hiển thị môn đã xóa
            Subjects = await _subjectService.GetAllSubjectsAsync(false);
        }
    }
}
