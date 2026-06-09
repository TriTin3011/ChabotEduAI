using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Threading.Tasks;
using BussinessLayer.DTOs;
using BussinessLayer.Services;

namespace PresentationLayer.Pages.Admin
{
    public class UsersModel : PageModel
    {
        private readonly IUserService _userService;
        private readonly IEmailService _emailService;

        public UsersModel(IUserService userService, IEmailService emailService)
        {
            _userService = userService;
            _emailService = emailService;
        }

        public IEnumerable<UserDto> Users { get; set; } = new List<UserDto>();

        [BindProperty] public string NewUsername { get; set; } = string.Empty;
        [BindProperty] public string NewPassword { get; set; } = string.Empty;
        [BindProperty] public string NewRole { get; set; } = "Student";
        [BindProperty] public string? NewEmail { get; set; }

        [BindProperty] public int UpdateId { get; set; }
        [BindProperty] public string UpdateUsername { get; set; } = string.Empty;
        [BindProperty] public string UpdateRole { get; set; } = string.Empty;
        [BindProperty] public string? UpdateEmail { get; set; }
        [BindProperty] public string? UpdatePassword { get; set; }

        public async Task OnGetAsync()
        {
            Users = await _userService.GetAllUsersAsync(false);
        }

        public async Task<IActionResult> OnPostAddUserAsync()
        {
            if (!string.IsNullOrWhiteSpace(NewUsername) && !string.IsNullOrWhiteSpace(NewPassword))
            {
                var created = await _userService.CreateUserAsync(NewUsername, NewPassword, NewRole, NewEmail);

                // Gửi email thông tin tài khoản nếu có email
                if (created && !string.IsNullOrWhiteSpace(NewEmail))
                {
                    try
                    {
                        await _emailService.SendAccountCreatedEmailAsync(NewEmail, NewUsername, NewPassword, NewRole);
                        TempData["SuccessMessage"] = $"Tạo tài khoản thành công! Đã gửi thông tin đến {NewEmail}.";
                    }
                    catch
                    {
                        TempData["WarnMessage"] = "Tạo tài khoản thành công nhưng không gửi được email. Kiểm tra lại cấu hình SMTP.";
                    }
                }
                else if (created)
                {
                    TempData["SuccessMessage"] = "Tạo tài khoản thành công!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Tên đăng nhập đã tồn tại.";
                }
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateUserAsync()
        {
            if (UpdateId > 0 && !string.IsNullOrWhiteSpace(UpdateUsername))
            {
                await _userService.UpdateUserAsync(UpdateId, UpdateUsername, UpdateRole, UpdateEmail, UpdatePassword);
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteUserAsync(int id)
        {
            await _userService.SoftDeleteUserAsync(id);
            return RedirectToPage();
        }
    }
}
