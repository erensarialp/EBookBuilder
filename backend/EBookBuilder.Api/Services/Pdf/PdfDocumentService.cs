using System.Collections.Concurrent;
using EBookBuilder.Api.Models.Processing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace EBookBuilder.Api.Services.Pdf;

public class PdfDocumentService : IPdfDocumentService
{
    public IReadOnlyDictionary<int, int> GenerateBook(
        string bookName,
        IReadOnlyList<PdfPaperContent> papers,
        string outputPath)
    {
        var outputDirectory =
            Path.GetDirectoryName(
                outputPath
            );

        if (
            !string.IsNullOrWhiteSpace(
                outputDirectory
            )
        )
        {
            Directory.CreateDirectory(
                outputDirectory
            );
        }

        var orderedPapers =
            papers
                .OrderBy(
                    x => x.OrderIndex
                )
                .ToList();

        var startPages =
            new ConcurrentDictionary<int, int>();

        Document
            .Create(document =>
            {
                document.Page(page =>
                {
                    page.Size(
                        PageSizes.A4
                    );

                    page.Margin(
                        2,
                        Unit.Centimetre
                    );

                    page.DefaultTextStyle(
                        style =>
                            style
                                .FontSize(10)
                                .FontColor(
                                    Colors
                                        .Grey
                                        .Darken4
                                )
                    );

                    page.Content()
                        .Column(column =>
                        {
                            ComposeCover(
                                column.Item(),
                                bookName
                            );

                            column.Item()
                                .PageBreak();

                            ComposeTableOfContents(
                                column.Item(),
                                orderedPapers
                            );

                            if (
                                orderedPapers.Count >
                                0
                            )
                            {
                                column.Item()
                                    .PageBreak();
                            }

                            for (
                                var index = 0;
                                index <
                                orderedPapers.Count;
                                index++
                            )
                            {
                                var paper =
                                    orderedPapers[
                                        index
                                    ];

                                ComposePaper(
                                    column.Item(),
                                    paper,
                                    startPages
                                );

                                if (
                                    index <
                                    orderedPapers.Count -
                                    1
                                )
                                {
                                    column.Item()
                                        .PageBreak();
                                }
                            }
                        });

                    page.Footer()
                        .PaddingTop(10)
                        .BorderTop(1)
                        .BorderColor(
                            Colors
                                .Grey
                                .Lighten2
                        )
                        .AlignCenter()
                        .Text(text =>
                        {
                            text
                                .CurrentPageNumber();

                            text.Span(
                                " / "
                            );

                            text
                                .TotalPages();
                        });
                });
            })
            .GeneratePdf(
                outputPath
            );

        return startPages
            .OrderBy(
                x => x.Key
            )
            .ToDictionary(
                x => x.Key,
                x => x.Value
            );
    }

    private static void ComposeCover(
        IContainer container,
        string bookName)
    {
        container
            .PaddingTop(180)
            .Column(column =>
            {
                column.Item()
                    .AlignCenter()
                    .Text(bookName)
                    .FontSize(28)
                    .Bold()
                    .FontColor(
                        Colors
                            .Blue
                            .Darken2
                    );

                column.Item()
                    .PaddingTop(16)
                    .AlignCenter()
                    .Text(
                        "Bildiriler E-Kitabı"
                    )
                    .FontSize(16)
                    .FontColor(
                        Colors
                            .Grey
                            .Darken1
                    );

                column.Item()
                    .PaddingTop(40)
                    .AlignCenter()
                    .Width(80)
                    .Height(3)
                    .Background(
                        Colors
                            .Blue
                            .Medium
                    );
            });
    }

    private static void ComposeTableOfContents(
        IContainer container,
        IReadOnlyList<PdfPaperContent> papers)
    {
        container.Column(column =>
        {
            column.Item()
                .PaddingBottom(24)
                .Text(
                    "İÇİNDEKİLER"
                )
                .FontSize(22)
                .Bold()
                .FontColor(
                    Colors
                        .Blue
                        .Darken2
                );

            foreach (
                var paper
                in papers
            )
            {
                var sectionName =
                    GetSectionName(
                        paper.OrderIndex
                    );

                column.Item()
                    .PaddingVertical(7)
                    .SectionLink(
                        sectionName
                    )
                    .Row(row =>
                    {
                        row.RelativeItem()
                            .Text(
                                $"{paper.OrderIndex}. {paper.Title}"
                            )
                            .FontSize(10);

                        row.ConstantItem(45)
                            .AlignRight()
                            .Text(text =>
                            {
                                text
                                    .BeginPageNumberOfSection(
                                        sectionName
                                    );
                            });
                    });
            }
        });
    }

    private static void ComposePaper(
        IContainer container,
        PdfPaperContent paper,
        ConcurrentDictionary<int, int> startPages)
    {
        var sectionName =
            GetSectionName(
                paper.OrderIndex
            );

        container
            .Section(
                sectionName
            )
            .Column(column =>
            {
                column.Item()
                    .Dynamic(
                        new StartPageCaptureComponent(
                            paper.OrderIndex,
                            startPages
                        )
                    );

                column.Item()
                    .PaddingBottom(18)
                    .Text(
                        $"{paper.OrderIndex}. {paper.Title}"
                    )
                    .FontSize(16)
                    .Bold()
                    .FontColor(
                        Colors
                            .Blue
                            .Darken2
                    );

                var paragraphs =
                    paper
                        .Paragraphs
                        .Where(
                            paragraph =>
                                !string
                                    .IsNullOrWhiteSpace(
                                        paragraph.Text
                                    )
                        )
                        .ToList();

                if (
                    paragraphs.Count > 0 &&
                    string.Equals(
                        paragraphs[0].Text,
                        paper.Title,
                        StringComparison
                            .OrdinalIgnoreCase
                    )
                )
                {
                    paragraphs.RemoveAt(
                        0
                    );
                }

                foreach (
                    var paragraph
                    in paragraphs
                )
                {
                    column.Item()
                        .PaddingBottom(8)
                        .Element(
                            element =>
                                ComposeParagraph(
                                    element,
                                    paragraph
                                )
                        );
                }
            });
    }

    private static void ComposeParagraph(
        IContainer container,
        ParsedParagraph paragraph)
    {
        var normalizedText =
            paragraph.Text.Trim();

        var isSectionTitle =
            normalizedText.Equals(
                "ÖZET",
                StringComparison
                    .OrdinalIgnoreCase
            )
            ||
            normalizedText.Equals(
                "ABSTRACT",
                StringComparison
                    .OrdinalIgnoreCase
            );

        if (
            isSectionTitle
        )
        {
            container
                .PaddingTop(8)
                .Text(
                    normalizedText
                )
                .FontSize(12)
                .Bold();

            return;
        }

        var text =
            container
                .Text(
                    normalizedText
                )
                .FontSize(10);

        if (
            paragraph.IsBold
        )
        {
            text.Bold();
        }

        switch (
            paragraph.Alignment
        )
        {
            case ParagraphAlignment.Center:

                text.AlignCenter();

                break;

            case ParagraphAlignment.Right:

                text.AlignRight();

                break;

            case ParagraphAlignment.Justify:

                text.Justify();

                break;

            default:

                text.AlignLeft();

                break;
        }
    }

    private static string GetSectionName(
        int orderIndex)
    {
        return
            $"paper-{orderIndex}";
    }
}