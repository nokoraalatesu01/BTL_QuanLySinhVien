using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using BTL_QuanLySinhVien.Models;
using BTL_QuanLySinhVien.Data;

namespace BTL_QuanLySinhVien.Pages.TeachersPages;

public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CreateModel> _logger;

    public CreateModel(ApplicationDbContext context, ILogger<CreateModel> logger)
    {
        _context = context;
        _logger = logger;
    }

    public IActionResult OnGet()
    {
        return Page();
    }

    [BindProperty]
    public Teacher Teacher { get; set; } = new();

    public async Task<IActionResult> OnPostAsync()
    {
        Teacher.TeacherId = Teacher.TeacherId?.Trim() ?? string.Empty;
        Teacher.FullName = Teacher.FullName?.Trim() ?? string.Empty;
        Teacher.AcademicTitle = Teacher.AcademicTitle?.Trim() ?? string.Empty;
        Teacher.Department = Teacher.Department?.Trim() ?? string.Empty;

        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (await _context.Teachers.AnyAsync(teacher => teacher.TeacherId == Teacher.TeacherId))
        {
            ModelState.AddModelError("Teacher.TeacherId", "Mã giảng viên này đã tồn tại.");
            return Page();
        }

        var teacher = new Teacher
        {
            TeacherId = Teacher.TeacherId,
            FullName = Teacher.FullName,
            AcademicTitle = Teacher.AcademicTitle,
            Department = Teacher.Department
        };
        _context.Teachers.Add(teacher);

        try
        {
            await _context.SaveChangesAsync();
            TempData["StatusMessage"] = $"Đã thêm giảng viên {teacher.FullName}.";
            return RedirectToPage("./Index");
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
        {
            _logger.LogWarning(ex, "Trùng mã giảng viên khi thêm hồ sơ {TeacherId}.", Teacher.TeacherId);
            ModelState.AddModelError("Teacher.TeacherId", "Mã giảng viên này đã tồn tại.");
            return Page();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Không thể lưu hồ sơ giảng viên {TeacherId}.", Teacher.TeacherId);
            ModelState.AddModelError(string.Empty, "Không thể lưu giảng viên. Vui lòng kiểm tra dữ liệu và thử lại.");
            return Page();
        }
    }
}
