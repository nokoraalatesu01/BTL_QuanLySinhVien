using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using BTL_QuanLySinhVien.Models;
using BTL_QuanLySinhVien.Data;

namespace BTL_QuanLySinhVien.Pages.StudentsPages;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(ApplicationDbContext context, ILogger<IndexModel> logger)
    {
        _context = context;
        _logger = logger;
    }

    public IList<Student> Student { get; private set; } = [];
    public string? LoadError { get; private set; }
    public string? SearchTerm { get; private set; }
    public int PageNumber { get; private set; }
    public int PageSize { get; private set; }
    public int TotalCount { get; private set; }
    public int TotalPages { get; private set; }
    public int StartPage => Math.Max(1, PageNumber - 2);
    public int EndPage => Math.Min(TotalPages, PageNumber + 2);

    public async Task OnGetAsync(string? searchTerm, int pageNumber = 1, int pageSize = 10)
    {
        SearchTerm = searchTerm?.Trim();
        PageSize = pageSize is 10 or 25 or 50 ? pageSize : 10;

        try
        {
            var students = _context.Students.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                students = students.Where(student =>
                    student.StudentId.Contains(SearchTerm) ||
                    student.FullName.Contains(SearchTerm));
            }

            TotalCount = await students.CountAsync();
            TotalPages = (int)Math.Ceiling(TotalCount / (double)PageSize);
            PageNumber = TotalPages == 0 ? 1 : Math.Clamp(pageNumber, 1, TotalPages);

            Student = await students
                .Include(student => student.Enrollments)
                .Include(student => student.Major)
                .OrderBy(student => student.StudentId)
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Không thể tải danh sách sinh viên từ cơ sở dữ liệu.");
            LoadError = "Không thể tải danh sách sinh viên từ cơ sở dữ liệu. Vui lòng thử lại sau.";
        }
    }
}
