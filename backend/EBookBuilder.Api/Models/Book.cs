namespace EBookBuilder.Api.Models;

public class Book
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public BookStatus Status { get; set; } = BookStatus.Pending;

    public string? PdfPath { get; set; }

    public ICollection<Paper> Papers { get; set; } = new List<Paper>();
}

public enum BookStatus
{
    Pending = 0,
    Processing = 1,
    Completed = 2,
    Failed = 3
}