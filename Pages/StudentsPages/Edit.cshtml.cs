using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection;
using System.Data;
using System.Security.Cryptography;
using System.Text.Json;
using BTL_QuanLySinhVien.Models;
using BTL_QuanLySinhVien.Data;

namespace BTL_QuanLySinhVien.Pages.StudentsPages;

public class EditModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<EditModel> _logger;
    private readonly IDataProtector _versionProtector;

    public EditModel(
        ApplicationDbContext context,
        ILogger<EditModel> logger,
        IDataProtectionProvider dataProtectionProvider)
    {
        _context = context;
        _logger = logger;
        _versionProtector = dataProtectionProvider.CreateProtector("StudentEditVersion");
    }

    [BindProperty]
    public Student Student { get; set; } = default!;

    [BindProperty]
    public string? OriginalValuesToken { get; set; }

    public IList<SelectListItem> Majors { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(string? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var student = await _context.Students.FirstOrDefaultAsync(m => m.StudentId == id);
        if (student is null)
        {
            return NotFound();
        }
        Student = student;
        OriginalValuesToken = ProtectVersion(student);
        await LoadMajorsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return NotFound();
        }

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

        if (string.IsNullOrWhiteSpace(OriginalValuesToken))
        {
            ModelState.AddModelError(string.Empty, "Phiên sửa không hợp lệ. Vui lòng tải lại biểu mẫu.");
            await LoadMajorsAsync();
            return Page();
        }

        StudentVersion originalVersion;
        try
        {
            originalVersion = JsonSerializer.Deserialize<StudentVersion>(
                _versionProtector.Unprotect(OriginalValuesToken))
                ?? throw new JsonException("Student edit version is empty.");
        }
        catch (CryptographicException ex)
        {
            _logger.LogWarning(ex, "Token phiên sửa sinh viên không hợp lệ cho {StudentId}.", id);
            ModelState.AddModelError(string.Empty, "Phiên sửa không hợp lệ. Vui lòng tải lại biểu mẫu.");
            await LoadMajorsAsync();
            return Page();
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Không thể đọc phiên sửa sinh viên {StudentId}.", id);
            ModelState.AddModelError(string.Empty, "Phiên sửa không hợp lệ. Vui lòng tải lại biểu mẫu.");
            await LoadMajorsAsync();
            return Page();
        }

        if (!string.Equals(originalVersion.StudentId, id, StringComparison.Ordinal))
        {
            ModelState.AddModelError(string.Empty, "Mã sinh viên không khớp với phiên sửa. Vui lòng mở lại hồ sơ.");
            Student.StudentId = id;
            await LoadMajorsAsync();
            return Page();
        }

        await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var existingStudent = await _context.Students
            .FirstOrDefaultAsync(student => student.StudentId == id);
        if (existingStudent is null)
        {
            return NotFound();
        }

        if (originalVersion != StudentVersion.From(existingStudent))
        {
            await transaction.RollbackAsync();
            Student = existingStudent;
            OriginalValuesToken = ProtectVersion(existingStudent);
            ModelState.Clear();
            ModelState.AddModelError(string.Empty,
                "Hồ sơ đã được thay đổi sau khi bạn mở biểu mẫu. Thông tin mới nhất đang được hiển thị; hãy kiểm tra rồi lưu lại.");
            await LoadMajorsAsync();
            return Page();
        }

        existingStudent.FullName = Student.FullName;
        existingStudent.DateOfBirth = Student.DateOfBirth;
        existingStudent.MajorId = Student.MajorId;

        try
        {
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            TempData["StatusMessage"] = $"Đã cập nhật sinh viên {existingStudent.FullName}.";
            return RedirectToPage("./Index");
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await transaction.RollbackAsync();
            _logger.LogWarning(ex, "Xung đột khi cập nhật hồ sơ sinh viên {StudentId}.", id);
            ModelState.AddModelError(string.Empty,
                "Hồ sơ đã bị xóa hoặc thay đổi bởi người khác. Vui lòng tải lại và kiểm tra trước khi lưu.");
            Student.StudentId = id;
            await LoadMajorsAsync();
            return Page();
        }
        catch (DbUpdateException ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Không thể cập nhật hồ sơ sinh viên {StudentId}.", id);
            ModelState.AddModelError(string.Empty, "Không thể lưu thay đổi. Vui lòng thử lại.");
            Student.StudentId = existingStudent.StudentId;
            await LoadMajorsAsync();
            return Page();
        }
    }

    private string ProtectVersion(Student student)
    {
        return _versionProtector.Protect(JsonSerializer.Serialize(StudentVersion.From(student)));
    }

    private async Task LoadMajorsAsync()
    {
        Majors = await _context.Majors
            .AsNoTracking()
            .OrderBy(major => major.Name)
            .Select(major => new SelectListItem(major.Name, major.MajorId))
            .ToListAsync();
    }

    private sealed record StudentVersion(
        string StudentId,
        string FullName,
        DateTime DateOfBirth,
        string MajorId)
    {
        public static StudentVersion From(Student student) =>
            new(student.StudentId, student.FullName, student.DateOfBirth, student.MajorId);
    }
}
