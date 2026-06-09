using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PresentationLayer.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public IActionResult OnGet()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var role = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value;
                if (role == "Admin") return RedirectToPage("/Admin/Dashboard");
                if (role == "Lecturer") return RedirectToPage("/Lecturer/MySubjects");
                return RedirectToPage("/Chat/Index");
            }
            return RedirectToPage("/Auth/Login");
        }
    }
}
