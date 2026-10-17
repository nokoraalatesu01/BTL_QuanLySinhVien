using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BTL_QuanLySinhVien.Models;
using BTL_QuanLySinhVien.Data;
using BTL_QuanLySinhVien.Services;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class StudentsAPIController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly EnrollmentProtectedDeletionService _deletionService;

    public StudentsAPIController(
        ApplicationDbContext context,
        EnrollmentProtectedDeletionService deletionService)
    {
        _context = context;
        _deletionService = deletionService;
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
        if (string.IsNullOrWhiteSpace(studentid))
        {
            return NotFound();
        }

        var outcome = await _deletionService.DeleteStudentAsync(studentid);
        return outcome.Status switch
        {
            ProtectedDeleteStatus.Deleted => NoContent(),
            ProtectedDeleteStatus.NotFound => NotFound(),
            ProtectedDeleteStatus.HasEnrollments => Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Không thể xóa sinh viên",
                Detail = "Sinh viên còn học phần đã đăng ký. Hãy xử lý các dữ liệu liên quan trước."
            }),
            _ => throw new InvalidOperationException("Unexpected student deletion result.")
        };
    }

    private bool StudentExists(string? studentid)
    {
        return _context.Students.Any(e => e.StudentId == studentid);
    }
}
