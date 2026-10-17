using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BTL_QuanLySinhVien.Models;
using BTL_QuanLySinhVien.Data;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class TeachersAPIController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    public TeachersAPIController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/Teacher
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Teacher>>> GetTeacher()
    {
        return await _context.Teachers.ToListAsync();
    }

    // GET: api/Teacher/5
    [HttpGet("{teacherid}")]
    public async Task<ActionResult<Teacher>> GetTeacher(string teacherid)
    {
        var teacher = await _context.Teachers.FindAsync(teacherid);

        if (teacher == null)
        {
            return NotFound();
        }

        return teacher;
    }

    // PUT: api/Teacher/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{teacherid}")]
    public async Task<IActionResult> PutTeacher(string? teacherid, Teacher teacher)
    {
        if (teacherid != teacher.TeacherId)
        {
            return BadRequest();
        }

        _context.Entry(teacher).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!TeacherExists(teacherid))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // POST: api/Teacher
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<Teacher>> PostTeacher(Teacher teacher)
    {
        _context.Teachers.Add(teacher);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetTeacher", new { teacherid = teacher.TeacherId }, teacher);
    }

    // DELETE: api/Teacher/5
    [HttpDelete("{teacherid}")]
    public async Task<IActionResult> DeleteTeacher(string? teacherid)
    {
        var teacher = await _context.Teachers.FindAsync(teacherid);
        if (teacher == null)
        {
            return NotFound();
        }

        _context.Teachers.Remove(teacher);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool TeacherExists(string? teacherid)
    {
        return _context.Teachers.Any(e => e.TeacherId == teacherid);
    }
}
