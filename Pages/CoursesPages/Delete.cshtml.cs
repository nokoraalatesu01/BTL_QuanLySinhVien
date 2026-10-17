using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using BTL_QuanLySinhVien.Models;
using BTL_QuanLySinhVien.Data;
using BTL_QuanLySinhVien.Services;

namespace BTL_QuanLySinhVien.Pages.CoursesPages;

public class DeleteModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly EnrollmentProtectedDeletionService _deletionService;

    public DeleteModel(
        ApplicationDbContext context,
        EnrollmentProtectedDeletionService deletionService)
    {
        _context = context;
        _deletionService = deletionService;
    }

    [BindProperty]
    public Course Course { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return NotFound();
        }

        var course = await _context.Courses
            .Include(course => course.Enrollments)
            .FirstOrDefaultAsync(course => course.CourseId == id);
        if (course is null)
        {
            return NotFound();
        }
        else
        {
            Course = course;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return NotFound();
        }

        var outcome = await _deletionService.DeleteCourseAsync(id);
        if (outcome.Status == ProtectedDeleteStatus.NotFound)
        {
            return NotFound();
        }

        if (outcome.Status == ProtectedDeleteStatus.HasEnrollments)
        {
            var course = await _context.Courses
                .Include(course => course.Enrollments)
                .FirstOrDefaultAsync(course => course.CourseId == id);
            if (course is null)
            {
                return NotFound();
            }

            Course = course;
            ModelState.AddModelError(string.Empty,
                "Không thể xóa môn học vì còn sinh viên đã đăng ký. Hãy xử lý các dữ liệu liên quan trước.");
            return Page();
        }

        TempData["StatusMessage"] = $"Đã xóa môn học {outcome.DisplayName}.";
        return RedirectToPage("./Index");
    }
}
