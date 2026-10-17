using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;


namespace BTL_QuanLySinhVien.Models
{
    public class Student
    {
        [Key]
        [Required(ErrorMessage = "Vui lòng nhập mã sinh viên.")]
        [Display(Name = "Mã sinh viên")]
        public string StudentId { get; set; } = string.Empty; 

        [Required(ErrorMessage = "Vui lòng nhập họ và tên.")]
        [MaxLength(100, ErrorMessage = "Họ và tên không được vượt quá 100 ký tự.")]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; } = string.Empty; 

        [Required(ErrorMessage = "Vui lòng nhập ngày sinh.")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày sinh")]
        public DateTime DateOfBirth { get; set; } 

        [Required(ErrorMessage = "Vui lòng nhập ngành.")]
        [MaxLength(50, ErrorMessage = "Ngành không được vượt quá 50 ký tự.")]
        [Display(Name = "Ngành")]
        public string Major { get; set; } = string.Empty; 

        [Display(Name = "Số học phần đã đăng ký")]
        public ICollection<Enrollment>? Enrollments { get; set; }
    }
}
