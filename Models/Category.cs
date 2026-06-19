using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace monitoring_management.Models;

public class Category
{
    [Key]
    [Column("category_id")]
    public int CategoryId { get; set; }

    [Column("category_name")]
    [Required]
    [MaxLength(100)]
    public string CategoryName { get; set; }

    public ICollection<ResearchPaper> ResearchPapers { get; set; } = new List<ResearchPaper>();
}