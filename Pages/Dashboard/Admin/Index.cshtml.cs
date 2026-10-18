using BTL_QuanLySinhVien.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BTL_QuanLySinhVien.Pages.Dashboard.Admin;

[Authorize(Roles = "Admin")]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public int StudentCount { get; private set; }
    public int TeacherCount { get; private set; }
    public int CourseCount { get; private set; }
    public int AccountCount { get; private set; }
    public IReadOnlyList<RoleCount> RoleCounts { get; private set; } = [];

    public async Task OnGetAsync()
    {
        StudentCount = await _context.Students.CountAsync();
        TeacherCount = await _context.Teachers.CountAsync();
        CourseCount = await _context.Courses.CountAsync();
        AccountCount = await _context.Users.CountAsync();

        RoleCounts = await (
            from userRole in _context.UserRoles.AsNoTracking()
            join role in _context.Roles.AsNoTracking() on userRole.RoleId equals role.Id
            group userRole by role.Name into roleGroup
            select new RoleCount(roleGroup.Key ?? "Không xác định", roleGroup.Count()))
            .OrderBy(roleCount => roleCount.Role)
            .ToListAsync();
    }

    public sealed record RoleCount(string Role, int Count);
}
