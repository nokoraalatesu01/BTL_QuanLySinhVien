using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using BTL_QuanLySinhVien.Models;
using BTL_QuanLySinhVien.Data;
using BTL_QuanLySinhVien.Services;

namespace BTL_QuanLySinhVien.Pages.StudentsPages;

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
    public Student Student { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(string? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var student = await _context.Students
            .Include(student => student.Enrollments)
            .Include(student => student.Major)
            .FirstOrDefaultAsync(student => student.StudentId == id);
        if (student is null)
        {
            return NotFound();
        }
        else
        {
            Student = student;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return NotFound();
        }

        var outcome = await _deletionService.DeleteStudentAsync(id);
        if (outcome.Status == ProtectedDeleteStatus.NotFound)
        {
            return NotFound();
        }

        if (outcome.Status == ProtectedDeleteStatus.HasEnrollments)
        {
            var student = await _context.Students
                .Include(student => student.Enrollments)
                .Include(student => student.Major)
                .FirstOrDefaultAsync(student => student.StudentId == id);
            if (student is null)
            {
                return NotFound();
            }

            Student = student;
            ModelState.AddModelError(string.Empty,
                "Không thể xóa sinh viên vì còn học phần đã đăng ký. Hãy xử lý các dữ liệu liên quan trước.");
            return Page();
        }

        TempData["StatusMessage"] = $"Đã xóa sinh viên {outcome.DisplayName}.";
        return RedirectToPage("./Index");
    }
}
