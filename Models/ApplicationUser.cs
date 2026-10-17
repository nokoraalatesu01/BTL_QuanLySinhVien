using Microsoft.AspNetCore.Identity;

namespace BTL_QuanLySinhVien.Models
{
    public class ApplicationUser : IdentityUser
    {
        [ProtectedPersonalData]
        public string? FullName { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
