namespace EBookBuilder.Api.DTOs;

public class BookResponse
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string? PdfPath { get; set; }

    public List<PaperResponse> Papers { get; set; } = [];
}

public class PaperResponse
{
    public int Id { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public int OrderIndex { get; set; }

    public int? StartPage { get; set; }
}