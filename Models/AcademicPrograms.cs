using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace monitoring_management.Models;

public class AcademicProgram
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("code")]
    [Required]
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    [Column("college_name")]
    [Required]
    [MaxLength(200)]
    public string CollegeName { get; set; } = string.Empty;

    [Column("sort_order")]
    public int SortOrder { get; set; }
}