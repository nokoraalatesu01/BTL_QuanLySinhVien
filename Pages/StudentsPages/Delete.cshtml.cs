using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Data;
using BTL_QuanLySinhVien.Models;
using BTL_QuanLySinhVien.Data;

namespace BTL_QuanLySinhVien.Pages.StudentsPages;

public class DeleteModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DeleteModel> _logger;

    public DeleteModel(ApplicationDbContext context, ILogger<DeleteModel> logger)
    {
        _context = context;
        _logger = logger;
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

        await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var student = await _context.Students
            .Include(student => student.Enrollments)
            .FirstOrDefaultAsync(student => student.StudentId == id);
        if (student is null)
        {
            return NotFound();
        }

        if (await _context.Enrollments.AnyAsync(enrollment => enrollment.StudentId == id))
        {
            await transaction.RollbackAsync();
            Student = student;
            ModelState.AddModelError(string.Empty,
                "Không thể xóa sinh viên vì còn học phần đã đăng ký. Hãy xử lý các dữ liệu liên quan trước.");
            return Page();
        }

        _context.Students.Remove(student);
        try
        {
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            TempData["StatusMessage"] = $"Đã xóa sinh viên {student.FullName}.";
            return RedirectToPage("./Index");
        }
        catch (DbUpdateException ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Không thể xóa hồ sơ sinh viên {StudentId}.", id);
            Student = student;
            ModelState.AddModelError(string.Empty,
                "Không thể xóa sinh viên do dữ liệu liên quan hoặc lỗi cơ sở dữ liệu. Không xóa dữ liệu liên quan.");
            return Page();
        }

    }
}
