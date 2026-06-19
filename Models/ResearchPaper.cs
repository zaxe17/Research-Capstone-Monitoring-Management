using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace monitoring_management.Models;

public enum PaperStatus
{
    Pending,
    Approved,
    Rejected
}

public class ResearchPaper
{
    [Key]
    [Column("paper_id")]
    [MaxLength(11)]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public string PaperId { get; set; }

    [Column("title")]
    [Required]
    [MaxLength(255)]
    public string Title { get; set; }

    [Column("description")]
    [Required]
    public string Description { get; set; }

    [Column("keywords")]
    public string? Keywords { get; set; }

    [Column("category_id")]
    public int? CategoryId { get; set; }

    [Column("year")]
    public int Year { get; set; }

    [Column("file_path")]
    [MaxLength(255)]
    public string? FilePath { get; set; }

    [Column("uploaded_by")]
    [Required]
    [MaxLength(11)]
    public string UploadedBy { get; set; }

    [Column("status")]
    public PaperStatus Status { get; set; } = PaperStatus.Pending;

    [Column("date_uploaded")]
    public DateTime DateUploaded { get; set; } = DateTime.Now;

    public Student? Student { get; set; }
    public Category? Category { get; set; }
    public ICollection<ResearchMember> ResearchMembers { get; set; } = new List<ResearchMember>();
}