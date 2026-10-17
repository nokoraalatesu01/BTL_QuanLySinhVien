using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using BTL_QuanLySinhVien.Models;
using BTL_QuanLySinhVien.Data;

namespace BTL_QuanLySinhVien.Pages.TeacherPages;

public class DeleteModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public DeleteModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Teacher Teacher { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(string? teacherid)
    {
        if (teacherid is null)
        {
            return NotFound();
        }

        var teacher = await _context.Teachers.FirstOrDefaultAsync(m => m.TeacherId == teacherid);
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

    public async Task<IActionResult> OnPostAsync(string? teacherid)
    {
        if (teacherid is null)
        {
            return NotFound();
        }

        var teacher = await _context.Teachers.FindAsync(teacherid);
        if (teacher != null)
        {
            Teacher = teacher;
            _context.Teachers.Remove(Teacher);
            await _context.SaveChangesAsync();
        }

        return RedirectToPage("./Index");
    }
}
