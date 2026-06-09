using System.Collections.Generic;
using System.Threading.Tasks;
using BussinessLayer.DTOs;
using BussinessLayer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PresentationLayer.Pages.Lecturer
{
    [Authorize(Roles = "Lecturer")]
    public class AllSubjectsModel : PageModel
    {
        private readonly ISubjectService _subjectService;

        public AllSubjectsModel(ISubjectService subjectService)
        {
            _subjectService = subjectService;
        }

        public IEnumerable<SubjectDto> Subjects { get; set; } = new List<SubjectDto>();

        public async Task OnGetAsync()
        {
            // GetAllSubjectsAsync(false) để không hiển thị môn đã xóa
            Subjects = await _subjectService.GetAllSubjectsAsync(false);
        }
    }
}
