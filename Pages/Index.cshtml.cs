using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BTL_QuanLySinhVien.Pages
{
    [AllowAnonymous]
    public class IndexModel : PageModel
    {
        public bool HasUnsupportedRole { get; private set; }

        public IActionResult OnGet()
        {
            if (User.IsInRole("Admin"))
            {
                return RedirectToPage("/Dashboard/Admin/Index");
            }

            if (User.IsInRole("Teacher"))
            {
                return RedirectToPage("/Dashboard/Teacher/Index");
            }

            if (User.IsInRole("Student"))
            {
                return RedirectToPage("/Dashboard/Student/Index");
            }

            HasUnsupportedRole = User.Identity?.IsAuthenticated == true;
            return Page();
        }
    }
}
