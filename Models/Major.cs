using System.ComponentModel.DataAnnotations;

namespace BTL_QuanLySinhVien.Models;

public class Major
{
    [Key]
    [StringLength(20)]
    [Display(Name = "Mã chuyên ngành")]
    public string MajorId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập tên chuyên ngành.")]
    [StringLength(100, ErrorMessage = "Tên chuyên ngành không được vượt quá 100 ký tự.")]
    [Display(Name = "Tên chuyên ngành")]
    public string Name { get; set; } = string.Empty;

    public ICollection<Student> Students { get; set; } = [];
}
