using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Contract_Monthly_Claim_System.Models
{
    public class Claim
    {
        public int Id { get; set; }
        
        [Required]
        public int UserId { get; set; }
        
        [ForeignKey("UserId")]
        public User User { get; set; } = null!;
        
        [Required]
        [StringLength(100)]
        public string LecturerName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(200)]
        public string CourseName { get; set; } = string.Empty;
        
        [Required]
        public DateTime LectureDate { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal HoursWorked { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal HourlyRate { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; } // Calculated: HoursWorked * HourlyRate
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        [StringLength(500)]
        public string? DocumentPath { get; set; } // Path to uploaded document
        
        public DateTime SubmittedDate { get; set; } = DateTime.Now;
        
        [Required]
        [StringLength(50)]
        public string CoordinatorStatus { get; set; } = "Pending"; // Pending, Approved, Rejected
        
        [StringLength(50)]
        public string? ManagerStatus { get; set; } = "Pending"; // Pending, Approved, Rejected
        
        [StringLength(500)]
        public string? CoordinatorNotes { get; set; }
        
        [StringLength(500)]
        public string? ManagerNotes { get; set; }
    }
}