namespace EBookBuilder.Api.Models.Processing;

public class ParsedDocument
{
    public string Title { get; set; } = string.Empty;

    public List<ParsedParagraph> Paragraphs { get; set; } = new();
}

public class ParsedParagraph
{
    public string Text { get; set; } = string.Empty;

    public string? StyleId { get; set; }

    public bool IsBold { get; set; }

    public ParagraphAlignment Alignment { get; set; }
}

public enum ParagraphAlignment
{
    Left,
    Center,
    Right,
    Justify
}