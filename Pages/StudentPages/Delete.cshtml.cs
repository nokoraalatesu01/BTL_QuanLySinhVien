using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using BTL_QuanLySinhVien.Models;
using BTL_QuanLySinhVien.Data;

namespace BTL_QuanLySinhVien.Pages.StudentPages;

public class DeleteModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public DeleteModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
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

    public async Task<IActionResult> OnPostAsync(string? studentid)
    {
        if (studentid is null)
        {
            return NotFound();
        }

        var student = await _context.Students.FindAsync(studentid);
        if (student != null)
        {
            Student = student;
            _context.Students.Remove(Student);
            await _context.SaveChangesAsync();
        }

        return RedirectToPage("./Index");
    }
}
