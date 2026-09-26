namespace EBookBuilder.Api.Models;

public class Paper
{
    public int Id { get; set; }

    public int BookId { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string OriginalFilePath { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public int OrderIndex { get; set; }

    public int? StartPage { get; set; }

    public Book Book { get; set; } = null!;
}