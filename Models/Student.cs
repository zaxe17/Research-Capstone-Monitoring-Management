using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace monitoring_management.Models;

public class Student
{
    [Key]
    [Column("student_id")]
    [MaxLength(11)]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public string StudentId { get; set; }

    [Column("student_no")]
    [Required]
    [MaxLength(20)]
    public string StudentNo { get; set; }

    [Column("program")]
    [Required]
    [MaxLength(20)]
    public string Program { get; set; }

    [Column("full_name")]
    [Required]
    [MaxLength(100)]
    public string FullName { get; set; }

    [Column("email")]
    [Required]
    [MaxLength(100)]
    public string Email { get; set; }

    [Column("password")]
    [Required]
    [MaxLength(255)]
    public string Password { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public ICollection<ResearchPaper> ResearchPapers { get; set; } = new List<ResearchPaper>();
    public ICollection<ResearchMember> ResearchMembers { get; set; } = new List<ResearchMember>();
}