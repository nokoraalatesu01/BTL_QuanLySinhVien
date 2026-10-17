using System.ComponentModel.DataAnnotations;


namespace BTL_QuanLySinhVien.Models
{
    public class Teacher
    {
        [Key]
        [StringLength(450)]
        [Display(Name = "Mã giảng viên")]
        public string TeacherId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập họ và tên.")]
        [StringLength(100, ErrorMessage = "Họ và tên không được vượt quá 100 ký tự.")]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập học hàm/học vị.")]
        [StringLength(50, ErrorMessage = "Học hàm/học vị không được vượt quá 50 ký tự.")]
        [Display(Name = "Học hàm/học vị")]
        public string AcademicTitle { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập khoa hoặc bộ môn.")]
        [StringLength(50, ErrorMessage = "Khoa/bộ môn không được vượt quá 50 ký tự.")]
        [Display(Name = "Khoa/bộ môn")]
        public string Department { get; set; } = string.Empty;
    }
}
