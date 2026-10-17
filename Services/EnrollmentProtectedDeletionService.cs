using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using BTL_QuanLySinhVien.Data;
using BTL_QuanLySinhVien.Models;

namespace BTL_QuanLySinhVien.Services;

public enum ProtectedDeleteStatus
{
    Deleted,
    NotFound,
    HasEnrollments
}

public sealed record ProtectedDeleteOutcome(ProtectedDeleteStatus Status, string? DisplayName = null);

public sealed class EnrollmentProtectedDeletionService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<EnrollmentProtectedDeletionService> _logger;

    public EnrollmentProtectedDeletionService(
        ApplicationDbContext context,
        ILogger<EnrollmentProtectedDeletionService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public Task<ProtectedDeleteOutcome> DeleteStudentAsync(string studentId) =>
        DeleteAsync(
            () => _context.Students
                .FromSqlInterpolated(
                    $"SELECT * FROM [dbo].[Students] WITH (UPDLOCK, HOLDLOCK) WHERE [StudentId] = {studentId}")
                .FirstOrDefaultAsync(),
            () => _context.Enrollments.AnyAsync(enrollment => enrollment.StudentId == studentId),
            _context.Students,
            student => student.FullName,
            "sinh viên",
            studentId);

    public Task<ProtectedDeleteOutcome> DeleteCourseAsync(string courseId) =>
        DeleteAsync(
            () => _context.Courses
                .FromSqlInterpolated(
                    $"SELECT * FROM [dbo].[Courses] WITH (UPDLOCK, HOLDLOCK) WHERE [CourseId] = {courseId}")
                .FirstOrDefaultAsync(),
            () => _context.Enrollments.AnyAsync(enrollment => enrollment.CourseId == courseId),
            _context.Courses,
            course => course.CourseName,
            "môn học",
            courseId);

    private async Task<ProtectedDeleteOutcome> DeleteAsync<TEntity>(
        Func<Task<TEntity?>> loadForUpdate,
        Func<Task<bool>> hasEnrollments,
        DbSet<TEntity> entities,
        Func<TEntity, string> displayName,
        string entityLabel,
        string entityId)
        where TEntity : class
    {
        await using var transaction =
            await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

        var entity = await loadForUpdate();
        if (entity is null)
        {
            await transaction.RollbackAsync();
            return new ProtectedDeleteOutcome(ProtectedDeleteStatus.NotFound);
        }

        if (await hasEnrollments())
        {
            await transaction.RollbackAsync();
            return new ProtectedDeleteOutcome(
                ProtectedDeleteStatus.HasEnrollments,
                displayName(entity));
        }

        entities.Remove(entity);
        try
        {
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return new ProtectedDeleteOutcome(ProtectedDeleteStatus.Deleted, displayName(entity));
        }
        catch (DbUpdateException ex) when (ex.GetBaseException() is SqlException { Number: 547 })
        {
            await transaction.RollbackAsync();
            _logger.LogWarning(ex,
                "Deletion of {EntityType} {EntityId} was blocked by a related record.",
                entityLabel,
                entityId);
            return new ProtectedDeleteOutcome(ProtectedDeleteStatus.HasEnrollments);
        }
        catch (DbUpdateException ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Could not delete {EntityType} {EntityId}.", entityLabel, entityId);
            throw;
        }
    }
}
