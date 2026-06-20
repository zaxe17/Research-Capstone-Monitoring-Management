namespace monitoring_management.Models;

public class StudentRowViewModel
{
    public string StudentId { get; set; } = string.Empty;
    public string StudentNo { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int LinkedPapersCount { get; set; }   // display only — uploaded + member-only papers
    public bool HasUploadedPapers { get; set; }   // real delete-block condition (FK constraint)
}