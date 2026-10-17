using System.ComponentModel.DataAnnotations;
using BTL_QuanLySinhVien.Data;
using BTL_QuanLySinhVien.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
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
                user.LockoutEnd
            })
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

        Accounts = users
            .Select(user => new AccountRow(
                user.Id,
                user.FullName,
                user.Email,
                user.UserName,
                user.EmailConfirmed,
                user.LockoutEnabled,
                user.LockoutEnd,
                rolesByUserId.GetValueOrDefault(user.Id, [])))
            .ToList();
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

    public sealed record AccountRow(
        string Id,
        string? FullName,
        string? Email,
        string? UserName,
        bool EmailConfirmed,
        bool LockoutEnabled,
        DateTimeOffset? LockoutEnd,
        IReadOnlyList<string> Roles);
}
