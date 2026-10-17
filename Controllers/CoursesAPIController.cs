using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BTL_QuanLySinhVien.Models;
using BTL_QuanLySinhVien.Data;

[Route("api/[controller]")]
[ApiController]
public class CoursesAPIController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    public CoursesAPIController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/Course
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Course>>> GetCourse()
    {
        return await _context.Courses.ToListAsync();
    }

    // GET: api/Course/5
    [HttpGet("{courseid}")]
    public async Task<ActionResult<Course>> GetCourse(string courseid)
    {
        var course = await _context.Courses.FindAsync(courseid);

        if (course == null)
        {
            return NotFound();
        }

        return course;
    }

    // PUT: api/Course/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{courseid}")]
    public async Task<IActionResult> PutCourse(string? courseid, Course course)
    {
        if (courseid != course.CourseId)
        {
            return BadRequest();
        }

        _context.Entry(course).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!CourseExists(courseid))
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

    // POST: api/Course
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<Course>> PostCourse(Course course)
    {
        _context.Courses.Add(course);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetCourse", new { courseid = course.CourseId }, course);
    }

    // DELETE: api/Course/5
    [HttpDelete("{courseid}")]
    public async Task<IActionResult> DeleteCourse(string? courseid)
    {
        var course = await _context.Courses.FindAsync(courseid);
        if (course == null)
        {
            return NotFound();
        }

        _context.Courses.Remove(course);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool CourseExists(string? courseid)
    {
        return _context.Courses.Any(e => e.CourseId == courseid);
    }
}
