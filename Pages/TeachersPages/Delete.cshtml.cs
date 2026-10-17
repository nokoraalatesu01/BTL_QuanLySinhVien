using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using BTL_QuanLySinhVien.Models;
using BTL_QuanLySinhVien.Data;

namespace BTL_QuanLySinhVien.Pages.TeachersPages;

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
    public Teacher Teacher { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return NotFound();
        }

        var teacher = await _context.Teachers.FirstOrDefaultAsync(m => m.TeacherId == id);
        if (teacher is null)
        {
            return NotFound();
        }
        else
        {
            Teacher = teacher;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return NotFound();
        }

        var teacher = await _context.Teachers.FirstOrDefaultAsync(item => item.TeacherId == id);
        if (teacher is null)
        {
            return NotFound();
        }

        Teacher = teacher;
        _context.Teachers.Remove(teacher);
        try
        {
            await _context.SaveChangesAsync();
            TempData["StatusMessage"] = $"Đã xóa giảng viên {teacher.FullName}.";
            return RedirectToPage("./Index");
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 547 })
        {
            _logger.LogWarning(ex, "Không thể xóa giảng viên {TeacherId} do dữ liệu liên quan.", id);
            ModelState.AddModelError(string.Empty,
                "Không thể xóa giảng viên vì hồ sơ đang được dữ liệu khác tham chiếu. Không có dữ liệu liên quan nào bị xóa.");
            return Page();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Không thể xóa hồ sơ giảng viên {TeacherId}.", id);
            ModelState.AddModelError(string.Empty,
                "Không thể xóa giảng viên do lỗi cơ sở dữ liệu. Không có dữ liệu liên quan nào bị xóa.");
            return Page();
        }
    }
}
