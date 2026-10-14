using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace BTL_QuanLySinhVien.Models
{
    public class Course 
    {
        [Key]
        public string CourseId { get; set; } = string.Empty; 

        [Required, MaxLength(150)]
        public string CourseName { get; set; } = string.Empty;

        public int Credits { get; set; } 

        public ICollection<Enrollment>? Enrollments { get; set; }
    }
}
