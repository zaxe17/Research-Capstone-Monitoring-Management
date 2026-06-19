using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace monitoring_management.Models;

public class Admin
{
    [Key]
    [Column("admin_id")]
    public int AdminId { get; set; }

    [Column("username")]
    [Required]
    [MaxLength(50)]
    public string Username { get; set; }

    [Column("password")]
    [Required]
    [MaxLength(255)]
    public string Password { get; set; }
}