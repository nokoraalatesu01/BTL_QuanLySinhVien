using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BTL_QuanLySinhVien.Models;
using BTL_QuanLySinhVien.Data;
using BTL_QuanLySinhVien.Services;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class CoursesAPIController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly EnrollmentProtectedDeletionService _deletionService;

    public CoursesAPIController(
        ApplicationDbContext context,
        EnrollmentProtectedDeletionService deletionService)
    {
        _context = context;
        _deletionService = deletionService;
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
        if (string.IsNullOrWhiteSpace(courseid))
        {
            return NotFound();
        }

        var outcome = await _deletionService.DeleteCourseAsync(courseid);
        return outcome.Status switch
        {
            ProtectedDeleteStatus.Deleted => NoContent(),
            ProtectedDeleteStatus.NotFound => NotFound(),
            ProtectedDeleteStatus.HasEnrollments => Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Không thể xóa môn học",
                Detail = "Môn học còn sinh viên đã đăng ký. Hãy xử lý các dữ liệu liên quan trước."
            }),
            _ => throw new InvalidOperationException("Unexpected course deletion result.")
        };
    }

    private bool CourseExists(string? courseid)
    {
        return _context.Courses.Any(e => e.CourseId == courseid);
    }
}
