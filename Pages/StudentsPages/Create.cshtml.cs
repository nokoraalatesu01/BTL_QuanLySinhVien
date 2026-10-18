using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Mvc.Rendering;
using BTL_QuanLySinhVien.Models;
using BTL_QuanLySinhVien.Data;

namespace BTL_QuanLySinhVien.Pages.StudentsPages;

public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CreateModel> _logger;

    public CreateModel(ApplicationDbContext context, ILogger<CreateModel> logger)
    {
        _context = context;
        _logger = logger;
    }

    public IList<SelectListItem> Majors { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync()
    {
        await LoadMajorsAsync();
        return Page();
    }

    [BindProperty]
    public Student Student { get; set; } = default!;

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadMajorsAsync();
            return Page();
        }

        if (!await _context.Majors.AnyAsync(major => major.MajorId == Student.MajorId))
        {
            ModelState.AddModelError("Student.MajorId", "Vui lòng chọn chuyên ngành hợp lệ.");
            await LoadMajorsAsync();
            return Page();
        }

        if (await _context.Students.AnyAsync(student => student.StudentId == Student.StudentId))
        {
            ModelState.AddModelError("Student.StudentId", "Mã sinh viên này đã tồn tại.");
            await LoadMajorsAsync();
            return Page();
        }

        var student = new Student
        {
            StudentId = Student.StudentId,
            FullName = Student.FullName,
            DateOfBirth = Student.DateOfBirth,
            MajorId = Student.MajorId
        };

        _context.Students.Add(student);
        try
        {
            await _context.SaveChangesAsync();
            TempData["StatusMessage"] = $"Đã thêm sinh viên {student.FullName}.";
            return RedirectToPage("./Index");
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
        {
            _logger.LogWarning(ex, "Trùng mã sinh viên khi thêm hồ sơ {StudentId}.", Student.StudentId);
            ModelState.AddModelError("Student.StudentId", "Mã sinh viên này đã tồn tại.");
            await LoadMajorsAsync();
            return Page();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Không thể lưu hồ sơ sinh viên {StudentId}.", Student.StudentId);
            ModelState.AddModelError(string.Empty, "Không thể lưu sinh viên. Vui lòng kiểm tra dữ liệu và thử lại.");
            await LoadMajorsAsync();
            return Page();
        }
    }

    private async Task LoadMajorsAsync()
    {
        Majors = await _context.Majors
            .AsNoTracking()
            .OrderBy(major => major.Name)
            .Select(major => new SelectListItem(major.Name, major.MajorId))
            .ToListAsync();
    }
}
