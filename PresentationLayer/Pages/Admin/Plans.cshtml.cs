using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using BussinessLayer.DTOs;
using BussinessLayer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PresentationLayer.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class PlansModel : PageModel
    {
        private readonly ISubscriptionPlanService _planService;

        public PlansModel(ISubscriptionPlanService planService)
        {
            _planService = planService;
        }

        public IEnumerable<SubscriptionPlanDto> Plans { get; set; } = new List<SubscriptionPlanDto>();

        // ── Create ──
        [BindProperty, Required] public string NewName        { get; set; } = string.Empty;
        [BindProperty]           public string NewDescription { get; set; } = string.Empty;
        [BindProperty]           public decimal NewPrice      { get; set; }
        [BindProperty]           public int NewLimit          { get; set; } = 5;
        [BindProperty]           public int NewSortOrder      { get; set; } = 0;
        [BindProperty]           public bool NewIsActive      { get; set; } = true;

        // ── Update ──
        [BindProperty] public int    EditId          { get; set; }
        [BindProperty] public string EditName        { get; set; } = string.Empty;
        [BindProperty] public string EditDescription { get; set; } = string.Empty;
        [BindProperty] public decimal EditPrice      { get; set; }
        [BindProperty] public int    EditLimit       { get; set; }
        [BindProperty] public int    EditSortOrder   { get; set; }
        [BindProperty] public bool   EditIsActive    { get; set; }

        public async Task OnGetAsync()
        {
            Plans = await _planService.GetAllAsync();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            var (ok, err) = await _planService.CreateAsync(new SubscriptionPlanDto
            {
                Name                 = NewName,
                Description          = NewDescription,
                Price                = NewPrice,
                MonthlyQuestionLimit = NewLimit,
                SortOrder            = NewSortOrder,
                IsActive             = NewIsActive
            });

            TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok ? "Thêm gói thành công!" : err;
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateAsync()
        {
            var (ok, err) = await _planService.UpdateAsync(new SubscriptionPlanDto
            {
                Id                   = EditId,
                Name                 = EditName,
                Description          = EditDescription,
                Price                = EditPrice,
                MonthlyQuestionLimit = EditLimit,
                SortOrder            = EditSortOrder,
                IsActive             = EditIsActive
            });

            TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok ? "Cập nhật gói thành công!" : err;
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var (ok, err) = await _planService.DeleteAsync(id);
            TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok ? "Đã xóa gói!" : err;
            return RedirectToPage();
        }
    }
}
