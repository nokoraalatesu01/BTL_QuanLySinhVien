using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace BTL_QuanLySinhVien.Models
{
    public class Enrollment
    {
        [Key]
        public int EnrollmentId { get; set; }

        public string StudentId { get; set; } = string.Empty;
        public Student? Student { get; set; }

        public string CourseId { get; set; } = string.Empty;
        public Course? Course { get; set; }

        public double AttendanceScore { get; set; } 
        public double MidtermScore { get; set; }    
        public double FinalScore { get; set; }
    }
}
