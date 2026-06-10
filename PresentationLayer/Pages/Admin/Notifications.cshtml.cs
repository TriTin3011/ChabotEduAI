using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BussinessLayer.Services;
using System.ComponentModel.DataAnnotations;

namespace PresentationLayer.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class NotificationsModel : PageModel
    {
        private readonly IEmailService _emailService;
        private readonly IUserService _userService;

        public NotificationsModel(IEmailService emailService, IUserService userService)
        {
            _emailService = emailService;
            _userService = userService;
        }

        [BindProperty]
        public string TargetAudience { get; set; } = "All";

        [BindProperty]
        [Required(ErrorMessage = "Vui lòng nhập tiêu đề email")]
        public string Subject { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Vui lòng nhập nội dung email")]
        public string EmailContent { get; set; } = string.Empty;

        public void OnGet()
        {
            ViewData["ActiveMenu"] = "AdminDashboard";
        }

        public async Task<IActionResult> OnPostSendAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var allUsers = await _userService.GetAllUsersAsync();
            var targetUsers = TargetAudience switch
            {
                "Students" => allUsers.Where(u => u.Role == "Student"),
                "Lecturers" => allUsers.Where(u => u.Role == "Lecturer"),
                _ => allUsers
            };

            var validEmails = targetUsers
                .Where(u => !string.IsNullOrWhiteSpace(u.Email))
                .Select(u => u.Email!)
                .Distinct()
                .ToList();

            if (!validEmails.Any())
            {
                TempData["ErrorMessage"] = "Không tìm thấy địa chỉ email hợp lệ nào cho nhóm đối tượng này.";
                return RedirectToPage();
            }

            await _emailService.SendBroadcastEmailAsync(validEmails, Subject, EmailContent);

            TempData["SuccessMessage"] = $"Đã gửi thông báo thành công đến {validEmails.Count} email.";
            return RedirectToPage();
        }
    }
}
