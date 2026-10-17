using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;


namespace BTL_QuanLySinhVien.Models
{
    public class Teacher 
    {
        [Key]
        public string TeacherId { get; set; } = string.Empty; 

        [Required, MaxLength(100)]
        public string FullName { get; set; } = string.Empty; 

        [MaxLength(50)]
        public string AcademicTitle { get; set; } = string.Empty; 

        [Required, MaxLength(50)]
        public string Department { get; set; } = string.Empty;
    }
}
