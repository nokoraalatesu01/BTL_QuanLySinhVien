using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using BTL_QuanLySinhVien.Models;
using BTL_QuanLySinhVien.Data;

namespace BTL_QuanLySinhVien.Pages.TeachersPages;

public class EditModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<EditModel> _logger;

    public EditModel(ApplicationDbContext context, ILogger<EditModel> logger)
    {
        _context = context;
        _logger = logger;
    }

    [BindProperty]
    public Teacher Teacher { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(string? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var teacher = await _context.Teachers.FirstOrDefaultAsync(m => m.TeacherId == id);
        if (teacher is null)
        {
            return NotFound();
        }
        Teacher = teacher;
        return Page();
    }

    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see https://aka.ms/RazorPagesCRUD.
    public async Task<IActionResult> OnPostAsync(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return NotFound();
        }

        if (!string.Equals(Teacher.TeacherId, id, StringComparison.Ordinal))
        {
            ModelState.AddModelError(string.Empty, "Không được thay đổi mã giảng viên.");
            Teacher.TeacherId = id;
            ModelState.Remove("Teacher.TeacherId");
            return Page();
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var existingTeacher = await _context.Teachers
            .FirstOrDefaultAsync(teacher => teacher.TeacherId == id);
        if (existingTeacher is null)
        {
            return NotFound();
        }

        existingTeacher.FullName = Teacher.FullName.Trim();
        existingTeacher.AcademicTitle = Teacher.AcademicTitle.Trim();
        existingTeacher.Department = Teacher.Department.Trim();

        try
        {
            await _context.SaveChangesAsync();
            TempData["StatusMessage"] = $"Đã cập nhật giảng viên {existingTeacher.FullName}.";
            return RedirectToPage("./Index");
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Xung đột khi cập nhật hồ sơ giảng viên {TeacherId}.", id);
            if (!await TeacherExistsAsync(id))
            {
                return NotFound();
            }

            ModelState.AddModelError(string.Empty,
                "Hồ sơ đã thay đổi trong lúc cập nhật. Vui lòng tải lại và kiểm tra thông tin.");
            return Page();
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 547 })
        {
            _logger.LogError(ex, "Không thể cập nhật giảng viên {TeacherId} do dữ liệu liên quan.", id);
            ModelState.AddModelError(string.Empty, "Không thể cập nhật do dữ liệu giảng viên đang được tham chiếu.");
            return Page();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Không thể cập nhật hồ sơ giảng viên {TeacherId}.", id);
            ModelState.AddModelError(string.Empty, "Không thể lưu thay đổi. Vui lòng thử lại.");
            return Page();
        }
    }

    private Task<bool> TeacherExistsAsync(string id)
    {
        return _context.Teachers.AnyAsync(teacher => teacher.TeacherId == id);
    }
}
