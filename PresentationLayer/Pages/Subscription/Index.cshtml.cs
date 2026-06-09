using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BussinessLayer.DTOs;
using BussinessLayer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PresentationLayer.Pages.Subscription
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ISubscriptionService _subscriptionService;

        public IndexModel(ISubscriptionService subscriptionService)
        {
            _subscriptionService = subscriptionService;
        }

        public SubscriptionInfoDto Info { get; set; } = new();

        public async Task OnGetAsync()
        {
            var userId = GetUserId();
            if (userId > 0)
                Info = await _subscriptionService.GetSubscriptionInfoAsync(userId);
        }

        public async Task<IActionResult> OnPostUpgradeAsync(string plan)
        {
            var userId = GetUserId();
            if (userId > 0)
                await _subscriptionService.UpgradePlanAsync(userId, plan);

            TempData["SuccessMessage"] = $"Nâng cấp gói {plan} thành công! (Demo — chưa tích hợp thanh toán)";
            return RedirectToPage();
        }

        private int GetUserId()
        {
            var val = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value
                   ?? User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(val, out var id) ? id : 0;
        }
    }
}
