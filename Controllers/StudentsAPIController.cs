using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BTL_QuanLySinhVien.Models;
using BTL_QuanLySinhVien.Data;

[Route("api/[controller]")]
[ApiController]
public class StudentsAPIController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    public StudentsAPIController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/Student
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Student>>> GetStudent()
    {
        return await _context.Students.ToListAsync();
    }

    // GET: api/Student/5
    [HttpGet("{studentid}")]
    public async Task<ActionResult<Student>> GetStudent(string studentid)
    {
        var student = await _context.Students.FindAsync(studentid);

        if (student == null)
        {
            return NotFound();
        }

        return student;
    }

    // PUT: api/Student/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{studentid}")]
    public async Task<IActionResult> PutStudent(string? studentid, Student student)
    {
        if (studentid != student.StudentId)
        {
            return BadRequest();
        }

        _context.Entry(student).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!StudentExists(studentid))
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

    // POST: api/Student
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<Student>> PostStudent(Student student)
    {
        _context.Students.Add(student);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetStudent", new { studentid = student.StudentId }, student);
    }

    // DELETE: api/Student/5
    [HttpDelete("{studentid}")]
    public async Task<IActionResult> DeleteStudent(string? studentid)
    {
        var student = await _context.Students.FindAsync(studentid);
        if (student == null)
        {
            return NotFound();
        }

        _context.Students.Remove(student);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool StudentExists(string? studentid)
    {
        return _context.Students.Any(e => e.StudentId == studentid);
    }
}
