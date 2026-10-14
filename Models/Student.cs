using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;


namespace BTL_QuanLySinhVien.Models
{
    public class Student
    {
        [Key]
        public string StudentId { get; set; } = string.Empty; 

        [Required, MaxLength(100)]
        public string FullName { get; set; } = string.Empty; 

        public DateTime DateOfBirth { get; set; } 

        [Required, MaxLength(50)]
        public string Major { get; set; } = string.Empty; 

        public ICollection<Enrollment>? Enrollments { get; set; }
    }
}
