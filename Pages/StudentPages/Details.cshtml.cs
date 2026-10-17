using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using BTL_QuanLySinhVien.Models;
using BTL_QuanLySinhVien.Data;

namespace BTL_QuanLySinhVien.Pages.StudentPages;

public class DetailsModel : PageModel
{
    private readonly ApplicationDbContext _context;
    public DetailsModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public Student Student { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(string? studentid)
    {
        if (studentid is null)
        {
            return NotFound();
        }

        var student = await _context.Students.FirstOrDefaultAsync(m => m.StudentId == studentid);
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
}
