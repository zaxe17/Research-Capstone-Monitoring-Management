// Models/AcademicProgram.cs
namespace monitoring_management.Models
{
    public class AcademicProgram
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;        // e.g. "BSIT"
        public string CollegeName { get; set; } = string.Empty; // e.g. "College of Computer and Information Sciences"
        public int SortOrder { get; set; }                      // controls display order
    }
}