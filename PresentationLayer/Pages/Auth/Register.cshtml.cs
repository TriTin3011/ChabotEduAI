using System.Threading.Tasks;
using BussinessLayer.DTOs;
using BussinessLayer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PresentationLayer.Pages.Auth
{
    public class RegisterModel : PageModel
    {
        private readonly IAuthService _authService;

        public RegisterModel(IAuthService authService)
        {
            _authService = authService;
        }

        [BindProperty]
        public RegisterDto Input { get; set; } = new RegisterDto();

        public void OnGet()
        {
            // Nếu đã đăng nhập thì chuyển về trang chủ
            if (User.Identity?.IsAuthenticated == true)
            {
                Response.Redirect("/");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var (success, error) = await _authService.RegisterAsync(Input);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, error);
                return Page();
            }

            TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng đăng nhập.";
            return RedirectToPage("/Auth/Login");
        }
    }
}
