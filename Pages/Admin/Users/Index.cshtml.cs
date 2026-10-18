using System.ComponentModel.DataAnnotations;
using System.Data;
using BTL_QuanLySinhVien.Data;
using BTL_QuanLySinhVien.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace BTL_QuanLySinhVien.Pages.Admin.Users;

[Authorize(Roles = "Admin")]
public class IndexModel : PageModel
{
    private static readonly string[] AssignableRoles = ["Student", "Teacher"];

    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ILogger<IndexModel> logger)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    public IReadOnlyList<AccountRow> Accounts { get; private set; } = [];

    [BindProperty]
    public CreateAccountInput Input { get; set; } = new();

    public async Task OnGetAsync()
    {
        await LoadAccountsAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Input.FullName = Input.FullName?.Trim() ?? string.Empty;
        Input.Email = Input.Email?.Trim() ?? string.Empty;

        if (!AssignableRoles.Contains(Input.Role, StringComparer.Ordinal))
        {
            ModelState.AddModelError("Input.Role", "Vai trò chỉ có thể là Sinh viên hoặc Giảng viên.");
        }

        if (!ModelState.IsValid)
        {
            await LoadAccountsAsync();
            return Page();
        }

        if (!await _roleManager.RoleExistsAsync(Input.Role))
        {
            ModelState.AddModelError(string.Empty, "Vai trò được chọn chưa được cấu hình trong hệ thống.");
            await LoadAccountsAsync();
            return Page();
        }

        if (await _userManager.FindByEmailAsync(Input.Email!) is not null)
        {
            ModelState.AddModelError("Input.Email", "Email này đã được sử dụng.");
            await LoadAccountsAsync();
            return Page();
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var user = new ApplicationUser
            {
                UserName = Input.Email,
                Email = Input.Email,
                FullName = Input.FullName
            };

            var createResult = await _userManager.CreateAsync(user, Input.Password);
            if (!createResult.Succeeded)
            {
                await transaction.RollbackAsync();
                AddIdentityErrors(createResult, "Không thể tạo tài khoản.");
                await LoadAccountsAsync();
                return Page();
            }

            var roleResult = await _userManager.AddToRoleAsync(user, Input.Role);
            if (!roleResult.Succeeded)
            {
                await transaction.RollbackAsync();
                _logger.LogWarning(
                    "Role assignment failed for a newly created account. Identity error codes: {ErrorCodes}",
                    GetErrorCodes(roleResult));
                ModelState.AddModelError(string.Empty, "Không thể gán vai trò; thao tác tạo tài khoản đã được hoàn tác.");
                await LoadAccountsAsync();
                return Page();
            }

            await transaction.CommitAsync();
            TempData["StatusMessage"] = "Đã tạo tài khoản.";
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while creating an account.");
            await transaction.RollbackAsync();
            ModelState.AddModelError(string.Empty, "Không thể hoàn tất thao tác tạo tài khoản. Vui lòng thử lại.");
            await LoadAccountsAsync();
            return Page();
        }
    }

    public async Task<IActionResult> OnPostUpdateLinkAsync(string? userId, string? profileId)
    {
        foreach (var key in ModelState.Keys
                     .Where(key => key.Equals("Input", StringComparison.Ordinal)
                         || key.StartsWith("Input.", StringComparison.Ordinal))
                     .ToList())
        {
            ModelState.Remove(key);
        }

        userId = userId?.Trim();
        profileId = string.IsNullOrWhiteSpace(profileId) ? null : profileId.Trim();
        if (string.IsNullOrWhiteSpace(userId))
        {
            ModelState.AddModelError(string.Empty, "Thiếu tài khoản cần cập nhật.");
            await LoadAccountsAsync();
            return Page();
        }

        if (userId.Length > 450 || profileId?.Length > 450)
        {
            ModelState.AddModelError(string.Empty, "Mã tài khoản hoặc mã hồ sơ vượt quá độ dài cho phép.");
            await LoadAccountsAsync();
            return Page();
        }

        try
        {
            await using var transaction =
                await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            var user = await _context.Users
                .SingleOrDefaultAsync(account => account.Id == userId);
            if (user is null)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError(string.Empty, "Không tìm thấy tài khoản cần cập nhật.");
                await LoadAccountsAsync();
                return Page();
            }

            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Count != 1 || !AssignableRoles.Contains(roles[0], StringComparer.Ordinal))
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError(string.Empty,
                    "Chỉ tài khoản có đúng một vai trò Student hoặc Teacher mới được liên kết hồ sơ.");
                await LoadAccountsAsync();
                return Page();
            }

            if (roles[0] == "Student")
            {
                if (!string.IsNullOrWhiteSpace(user.TeacherId))
                {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError(string.Empty, "Tài khoản đang có liên kết sai loại hồ sơ.");
                    await LoadAccountsAsync();
                    return Page();
                }

                if (profileId is null)
                {
                    user.StudentId = null;
                }
                else
                {
                    var studentExists = await _context.Students
                        .AnyAsync(student => student.StudentId == profileId);
                    var alreadyLinked = await _context.Users
                        .AnyAsync(account => account.Id != user.Id && account.StudentId == profileId);
                    if (!studentExists || alreadyLinked)
                    {
                        await transaction.RollbackAsync();
                        ModelState.AddModelError(string.Empty,
                            !studentExists
                                ? "Không tìm thấy hồ sơ sinh viên đã chọn."
                                : "Hồ sơ sinh viên này đã được liên kết với tài khoản khác.");
                        await LoadAccountsAsync();
                        return Page();
                    }

                    user.StudentId = profileId;
                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(user.StudentId))
                {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError(string.Empty, "Tài khoản đang có liên kết sai loại hồ sơ.");
                    await LoadAccountsAsync();
                    return Page();
                }

                if (profileId is null)
                {
                    user.TeacherId = null;
                }
                else
                {
                    var teacherExists = await _context.Teachers
                        .AnyAsync(teacher => teacher.TeacherId == profileId);
                    var alreadyLinked = await _context.Users
                        .AnyAsync(account => account.Id != user.Id && account.TeacherId == profileId);
                    if (!teacherExists || alreadyLinked)
                    {
                        await transaction.RollbackAsync();
                        ModelState.AddModelError(string.Empty,
                            !teacherExists
                                ? "Không tìm thấy hồ sơ giảng viên đã chọn."
                                : "Hồ sơ giảng viên này đã được liên kết với tài khoản khác.");
                        await LoadAccountsAsync();
                        return Page();
                    }

                    user.TeacherId = profileId;
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            TempData["StatusMessage"] = profileId is null
                ? "Đã gỡ liên kết hồ sơ. Tài khoản và hồ sơ vẫn được giữ nguyên."
                : "Đã cập nhật liên kết hồ sơ.";
            return RedirectToPage();
        }
        catch (DbUpdateException ex) when (ex.GetBaseException() is SqlException { Number: 2601 or 2627 })
        {
            _logger.LogWarning(ex, "A profile link could not be saved because it conflicts with an existing link.");
            ModelState.AddModelError(string.Empty, "Hồ sơ vừa được liên kết với tài khoản khác. Tải lại danh sách và thử lại.");
            await LoadAccountsAsync();
            return Page();
        }
        catch (DbUpdateException ex) when (ex.GetBaseException() is SqlException { Number: 547 })
        {
            _logger.LogWarning(ex, "A profile link could not be saved because a referenced profile no longer exists.");
            ModelState.AddModelError(string.Empty, "Hồ sơ đã chọn không còn tồn tại. Tải lại danh sách và thử lại.");
            await LoadAccountsAsync();
            return Page();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "A profile link could not be saved.");
            ModelState.AddModelError(string.Empty, "Không thể cập nhật liên kết hồ sơ do lỗi cơ sở dữ liệu.");
            await LoadAccountsAsync();
            return Page();
        }
    }

    private async Task LoadAccountsAsync()
    {
        var users = await _context.Users
            .AsNoTracking()
            .OrderBy(user => user.Email)
            .Select(user => new
            {
                user.Id,
                user.FullName,
                user.Email,
                user.UserName,
                user.EmailConfirmed,
                user.LockoutEnabled,
                user.LockoutEnd,
                user.StudentId,
                user.TeacherId
            })
            .ToListAsync();

        var students = await _context.Students
            .AsNoTracking()
            .OrderBy(student => student.StudentId)
            .Select(student => new ProfileOption(student.StudentId, student.FullName))
            .ToListAsync();

        var teachers = await _context.Teachers
            .AsNoTracking()
            .OrderBy(teacher => teacher.TeacherId)
            .Select(teacher => new ProfileOption(teacher.TeacherId, teacher.FullName))
            .ToListAsync();

        var userRoles = await (
            from userRole in _context.UserRoles.AsNoTracking()
            join role in _context.Roles.AsNoTracking() on userRole.RoleId equals role.Id
            select new { userRole.UserId, role.Name })
            .ToListAsync();

        var rolesByUserId = userRoles
            .GroupBy(userRole => userRole.UserId)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<string>)group
                    .Select(userRole => userRole.Name!)
                    .OrderBy(roleName => roleName)
                    .ToList());

        Accounts = users.Select(user =>
        {
            var roles = rolesByUserId.GetValueOrDefault(user.Id, []);
            var canManageLink = roles.Count == 1 && AssignableRoles.Contains(roles[0], StringComparer.Ordinal);
            var linkedProfileId = user.StudentId ?? user.TeacherId;
            var linkedProfileType = user.StudentId is not null
                ? "Sinh viên"
                : user.TeacherId is not null
                    ? "Giảng viên"
                    : null;
            var linkedProfileName = user.StudentId is not null
                ? students.FirstOrDefault(student => student.Id == user.StudentId)?.Name
                : user.TeacherId is not null
                    ? teachers.FirstOrDefault(teacher => teacher.Id == user.TeacherId)?.Name
                    : null;
            var availableProfiles = roles.Count == 1 && roles[0] == "Student"
                ? students.Where(student =>
                    student.Id == user.StudentId
                    || !users.Any(other => other.Id != user.Id && other.StudentId == student.Id)).ToList()
                : roles.Count == 1 && roles[0] == "Teacher"
                    ? teachers.Where(teacher =>
                        teacher.Id == user.TeacherId
                        || !users.Any(other => other.Id != user.Id && other.TeacherId == teacher.Id)).ToList()
                    : [];

            return new AccountRow(
                user.Id,
                user.FullName,
                user.Email,
                user.UserName,
                user.EmailConfirmed,
                user.LockoutEnabled,
                user.LockoutEnd,
                roles,
                linkedProfileId,
                linkedProfileType,
                linkedProfileName,
                canManageLink,
                availableProfiles);
        }).ToList();
    }

    private void AddIdentityErrors(IdentityResult result, string message)
    {
        var errorCodes = GetErrorCodes(result);
        _logger.LogWarning("Account creation failed. Identity error codes: {ErrorCodes}", errorCodes);
        ModelState.AddModelError(string.Empty, $"{message} Mã lỗi: {errorCodes}");
    }

    private static string GetErrorCodes(IdentityResult result) =>
        string.Join(", ", result.Errors.Select(error => error.Code));

    public sealed class CreateAccountInput
    {
        [Required(ErrorMessage = "Vui lòng nhập họ tên.")]
        [StringLength(100)]
        public string? FullName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        [StringLength(256)]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn vai trò.")]
        public string Role { get; set; } = string.Empty;
    }

    public sealed record ProfileOption(string Id, string Name);

    public sealed record AccountRow(
        string Id,
        string? FullName,
        string? Email,
        string? UserName,
        bool EmailConfirmed,
        bool LockoutEnabled,
        DateTimeOffset? LockoutEnd,
        IReadOnlyList<string> Roles,
        string? LinkedProfileId,
        string? LinkedProfileType,
        string? LinkedProfileName,
        bool CanManageLink,
        IReadOnlyList<ProfileOption> AvailableProfiles);
}
