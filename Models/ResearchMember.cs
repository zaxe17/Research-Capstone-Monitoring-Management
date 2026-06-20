using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace monitoring_management.Models;

public enum MemberRole
{
    Leader,
    Member
}

public class ResearchMember
{
    [Key]
    [Column("member_id")]
    [MaxLength(13)]  // MN-00000001 = 13 chars
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public string MemberId { get; set; }

    [Column("paper_id")]
    [Required]
    [MaxLength(13)]
    public string PaperId { get; set; }

    [Column("student_id")]
    [MaxLength(13)]  // FK to students(student_id)
    public string? StudentId { get; set; }

    [Column("member_name")]
    [Required]
    [MaxLength(100)]
    public string MemberName { get; set; }

    [Column("role")]
    public MemberRole Role { get; set; } = MemberRole.Member;

    public ResearchPaper? ResearchPaper { get; set; }
    public Student? Student { get; set; }
}