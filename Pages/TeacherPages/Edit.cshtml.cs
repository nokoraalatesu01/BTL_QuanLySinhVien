using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using BTL_QuanLySinhVien.Models;
using BTL_QuanLySinhVien.Data;

namespace BTL_QuanLySinhVien.Pages.TeacherPages;

public class EditModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public EditModel(ApplicationDbContext context)
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
        Teacher = teacher;
        return Page();
    }

    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see https://aka.ms/RazorPagesCRUD.
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        _context.Attach(Teacher).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!TeacherExists(Teacher.TeacherId))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return RedirectToPage("./Index");
    }

    private bool TeacherExists(string teacherid)
    {
        return _context.Teachers.Any(e => e.TeacherId == teacherid);
    }
}
