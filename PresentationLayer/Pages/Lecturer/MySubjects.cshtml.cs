using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Threading.Tasks;
using BussinessLayer.DTOs;
using BussinessLayer.Services;
using System.Security.Claims;
using System.Linq;

namespace PresentationLayer.Pages.Lecturer
{
    public class MySubjectsModel : PageModel
    {
        private readonly ISubjectService _subjectService;

        public MySubjectsModel(ISubjectService subjectService)
        {
            _subjectService = subjectService;
        }

        public IEnumerable<SubjectDto> Subjects { get; set; } = new List<SubjectDto>();

        public async Task OnGetAsync()
        {
            var userIdStr = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            if (int.TryParse(userIdStr, out int userId))
            {
                Subjects = await _subjectService.GetSubjectsByLecturerIdAsync(userId);
            }
        }
    }
}
