namespace EBookBuilder.Api.Models.Processing;

public class PdfPaperContent
{
    public string Title { get; set; } = string.Empty;

    public int OrderIndex { get; set; }

    public List<ParsedParagraph> Paragraphs { get; set; } = new();
}