using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using EBookBuilder.Api.Models.Processing;

namespace EBookBuilder.Api.Services.Word;

public class WordDocumentService : IWordDocumentService
{
    public ParsedDocument ReadDocument(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException(
                "Word dosyası bulunamadı.",
                filePath
            );
        }

        using var document = WordprocessingDocument.Open(
            filePath,
            false
        );

        var mainDocumentPart = document.MainDocumentPart;

        if (mainDocumentPart is null)
        {
            throw new InvalidDataException(
                "Word belgesinin ana bölümü bulunamadı."
            );
        }

        var body = mainDocumentPart.Document?.Body;

        if (body is null)
        {
            throw new InvalidDataException(
                "Word belgesinin içeriği okunamadı."
            );
        }

        var parsedParagraphs =
            new List<ParsedParagraph>();

        foreach (
            var paragraph
            in body.Elements<Paragraph>()
        )
        {
            var text =
                GetParagraphText(paragraph);

            if (string.IsNullOrWhiteSpace(text))
            {
                continue;
            }

            var parsedParagraph =
                new ParsedParagraph
                {
                    Text = text.Trim(),

                    StyleId =
                        GetStyleId(paragraph),

                    IsBold =
                        IsParagraphBold(paragraph),

                    Alignment =
                        GetAlignment(paragraph)
                };

            parsedParagraphs.Add(
                parsedParagraph
            );
        }

        var title =
            ExtractTitle(parsedParagraphs);

        return new ParsedDocument
        {
            Title = title,
            Paragraphs = parsedParagraphs
        };
    }

    private static string GetParagraphText(
        Paragraph paragraph)
    {
        var textParts =
            paragraph
                .Descendants<Text>()
                .Select(text => text.Text);

        return string.Concat(textParts);
    }

    private static string? GetStyleId(
        Paragraph paragraph)
    {
        return paragraph
            .ParagraphProperties?
            .ParagraphStyleId?
            .Val?
            .Value;
    }

    private static bool IsParagraphBold(
        Paragraph paragraph)
    {
        var runs =
            paragraph
                .Elements<Run>()
                .Where(
                    run =>
                        !string.IsNullOrWhiteSpace(
                            run.InnerText
                        )
                )
                .ToList();

        if (runs.Count == 0)
        {
            return false;
        }

        return runs.All(
            run =>
                run.RunProperties?
                    .Bold is not null
        );
    }

    private static ParagraphAlignment GetAlignment(
        Paragraph paragraph)
    {
        var justification =
            paragraph
                .ParagraphProperties?
                .Justification?
                .Val?
                .Value;

        if (
            justification ==
            JustificationValues.Center
        )
        {
            return ParagraphAlignment.Center;
        }

        if (
            justification ==
            JustificationValues.Right
        )
        {
            return ParagraphAlignment.Right;
        }

        if (
            justification ==
            JustificationValues.Both
        )
        {
            return ParagraphAlignment.Justify;
        }

        return ParagraphAlignment.Left;
    }

    private static string ExtractTitle(
        List<ParsedParagraph> paragraphs)
    {
        if (paragraphs.Count == 0)
        {
            return "Başlıksız Dosya";
        }

        return paragraphs[0].Text;
    }
}