using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using BTL_QuanLySinhVien.Models;
using BTL_QuanLySinhVien.Data;

namespace BTL_QuanLySinhVien.Pages.TeachersPages;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(ApplicationDbContext context, ILogger<IndexModel> logger)
    {
        _context = context;
        _logger = logger;
    }

    public IList<Teacher> Teacher { get; private set; } = [];
    public IList<string> Departments { get; private set; } = [];
    public string? SearchTerm { get; private set; }
    public string? SelectedDepartment { get; private set; }
    public string? LoadError { get; private set; }
    public int PageNumber { get; private set; }
    public int PageSize { get; private set; }
    public int TotalCount { get; private set; }
    public int TotalPages { get; private set; }
    public int StartPage => Math.Max(1, PageNumber - 2);
    public int EndPage => Math.Min(TotalPages, PageNumber + 2);

    public async Task OnGetAsync(
        string? searchTerm,
        string? department,
        int pageNumber = 1,
        int pageSize = 10)
    {
        SearchTerm = searchTerm?.Trim();
        SelectedDepartment = department?.Trim();
        PageSize = pageSize is 10 or 25 or 50 ? pageSize : 10;

        try
        {
            Departments = await _context.Teachers
                .AsNoTracking()
                .Where(teacher => teacher.Department != "")
                .Select(teacher => teacher.Department)
                .Distinct()
                .OrderBy(value => value)
                .ToListAsync();

            var teachers = _context.Teachers.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                teachers = teachers.Where(teacher =>
                    teacher.TeacherId.Contains(SearchTerm) ||
                    teacher.FullName.Contains(SearchTerm));
            }

            if (!string.IsNullOrWhiteSpace(SelectedDepartment))
            {
                teachers = teachers.Where(teacher => teacher.Department == SelectedDepartment);
            }

            TotalCount = await teachers.CountAsync();
            TotalPages = (int)Math.Ceiling(TotalCount / (double)PageSize);
            PageNumber = TotalPages == 0 ? 1 : Math.Clamp(pageNumber, 1, TotalPages);
            Teacher = await teachers
                .OrderBy(teacher => teacher.TeacherId)
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Không thể tải danh sách giảng viên từ cơ sở dữ liệu.");
            LoadError = "Không thể tải danh sách giảng viên từ cơ sở dữ liệu. Vui lòng thử lại sau.";
        }
    }
}
