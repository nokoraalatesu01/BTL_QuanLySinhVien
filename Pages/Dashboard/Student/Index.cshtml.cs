using BTL_QuanLySinhVien.Data;
using BTL_QuanLySinhVien.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BTL_QuanLySinhVien.Pages.Dashboard.Student;

[Authorize(Roles = "Student")]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public IndexModel(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public BTL_QuanLySinhVien.Models.Student? Profile { get; private set; }
    public bool HasLinkedProfile { get; private set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return Forbid();
        }

        var roles = await _userManager.GetRolesAsync(user);
        if (roles.Count != 1 || roles[0] != "Student")
        {
            return Forbid();
        }

        if (user.StudentId is null)
        {
            return Page();
        }

        Profile = await _context.Students
            .AsNoTracking()
            .SingleOrDefaultAsync(student => student.StudentId == user.StudentId);
        HasLinkedProfile = Profile is not null;
        return Page();
    }
}
