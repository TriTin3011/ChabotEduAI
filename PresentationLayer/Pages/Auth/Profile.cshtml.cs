using System.Threading.Tasks;
using DataAccessLayer.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PresentationLayer.ViewModels.Auth;

namespace PresentationLayer.Pages.Auth
{
    [Authorize]
    public class ProfileModel : PageModel
    {
        private readonly IUserRepository _userRepository;

        public ProfileModel(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;

        [BindProperty]
        public ProfilePasswordChangeViewModel PasswordChangeModel { get; set; } = new ProfilePasswordChangeViewModel();

        public string Message { get; set; } = string.Empty;
        public bool IsSuccess { get; set; } = false;

        public async Task<IActionResult> OnGetAsync()
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username)) return RedirectToPage("/Auth/Login");

            var user = await _userRepository.GetUserByUsernameAsync(username);
            if (user == null) return RedirectToPage("/Auth/Login");

            Username = user.Username;
            Email = user.Email ?? "Chưa có email";
            Role = user.Role;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username)) return RedirectToPage("/Auth/Login");

            var user = await _userRepository.GetUserByUsernameAsync(username);
            if (user == null) return RedirectToPage("/Auth/Login");

            // Populate view data again
            Username = user.Username;
            Email = user.Email ?? "Chưa có email";
            Role = user.Role;

            if (string.IsNullOrEmpty(PasswordChangeModel.CurrentPassword) || string.IsNullOrEmpty(PasswordChangeModel.NewPassword) || string.IsNullOrEmpty(PasswordChangeModel.ConfirmPassword))
            {
                Message = "Vui lòng nhập đầy đủ thông tin.";
                IsSuccess = false;
                return Page();
            }

            if (PasswordChangeModel.NewPassword != PasswordChangeModel.ConfirmPassword)
            {
                Message = "Mật khẩu mới và xác nhận mật khẩu không khớp.";
                IsSuccess = false;
                return Page();
            }

            // The system uses plain text passwords for now
            if (user.PasswordHash != PasswordChangeModel.CurrentPassword)
            {
                Message = "Mật khẩu hiện tại không đúng.";
                IsSuccess = false;
                return Page();
            }

            user.PasswordHash = PasswordChangeModel.NewPassword;
            await _userRepository.UpdateUserAsync(user);

            Message = "Đổi mật khẩu thành công!";
            IsSuccess = true;

            // Clear inputs on success
            PasswordChangeModel = new ProfilePasswordChangeViewModel();

            return Page();
        }
    }
}
