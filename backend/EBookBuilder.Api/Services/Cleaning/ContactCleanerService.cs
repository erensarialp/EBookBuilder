using System.Text;
using System.Text.RegularExpressions;
using EBookBuilder.Api.Models.Processing;

namespace EBookBuilder.Api.Services.Cleaning;

public class ContactCleanerService : IContactCleanerService
{
    private const string ContactLabelPattern =
        @"(?:tel\s*\.?\s*no|telefon|tel|gsm|cep(?:\s*telefonu)?|mobile|" +
        @"e[\s-]?posta|e-?mail|email|mail|irtibat|iletişim)";

    private static readonly Regex LabeledEmailRegex =
        new(
            @"\b(?:e[\s-]?posta|e-?mail|email|mail)\s*:?\s*" +
            @"[A-Za-z0-9._%+\-]+@[A-Za-z0-9.\-]+\.[A-Za-z]{2,}",
            RegexOptions.IgnoreCase |
            RegexOptions.Compiled
        );

    private static readonly Regex EmailRegex =
        new(
            @"[A-Za-z0-9._%+\-]+@[A-Za-z0-9.\-]+\.[A-Za-z]{2,}",
            RegexOptions.IgnoreCase |
            RegexOptions.Compiled
        );

    private static readonly Regex LabeledPhoneRegex =
        new(
            @"\b(?:tel(?:efon)?|gsm|cep(?:\s*telefonu)?|mobile)" +
            @"\s*:?\s*" +
            @"(?:\+90|0)?[\s().\-]*" +
            @"\(?\d{3}\)?[\s.\-]*" +
            @"\d{3}[\s.\-]*\d{2}[\s.\-]*\d{2}",
            RegexOptions.IgnoreCase |
            RegexOptions.Compiled
        );

    private static readonly Regex TurkishPhoneRegex =
        new(
            @"(?<!\d)(?:\+90|0)[\s().\-]*" +
            @"\(?\d{3}\)?[\s.\-]*" +
            @"\d{3}[\s.\-]*\d{2}[\s.\-]*\d{2}(?!\d)",
            RegexOptions.Compiled
        );

    private static readonly Regex StandaloneContactLineRegex =
        new(
            $@"^(?:\s*{ContactLabelPattern}" +
            @"\s*(?:[:.\-()|/]+\s*)*)+$",
            RegexOptions.IgnoreCase |
            RegexOptions.Compiled
        );

    private static readonly Regex LeadingContactLabelRegex =
        new(
            $@"^\s*{ContactLabelPattern}" +
            @"\s*(?:[:.\-()|/]+\s*)+",
            RegexOptions.IgnoreCase |
            RegexOptions.Compiled
        );

    private static readonly Regex MultipleWhitespaceRegex =
        new(
            @"\s+",
            RegexOptions.Compiled
        );

    private static readonly Regex LeadingSeparatorRegex =
        new(
            @"^(?:\s*[|/,;:\-()]+\s*)+",
            RegexOptions.Compiled
        );

    private static readonly Regex TrailingSeparatorRegex =
        new(
            @"(?:\s*[|/,;:\-()]+\s*)+$",
            RegexOptions.Compiled
        );

    public string CleanText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var cleanedText =
            NormalizeInput(text);

        cleanedText =
            LabeledEmailRegex.Replace(
                cleanedText,
                string.Empty
            );

        cleanedText =
            EmailRegex.Replace(
                cleanedText,
                string.Empty
            );

        cleanedText =
            LabeledPhoneRegex.Replace(
                cleanedText,
                string.Empty
            );

        cleanedText =
            TurkishPhoneRegex.Replace(
                cleanedText,
                string.Empty
            );

        cleanedText =
            NormalizeText(cleanedText);

        if (
            string.IsNullOrWhiteSpace(cleanedText) ||
            StandaloneContactLineRegex.IsMatch(cleanedText)
        )
        {
            return string.Empty;
        }

        cleanedText =
            RemoveLeadingContactLabels(
                cleanedText
            );

        cleanedText =
            NormalizeText(cleanedText);

        if (
            string.IsNullOrWhiteSpace(cleanedText) ||
            StandaloneContactLineRegex.IsMatch(cleanedText)
        )
        {
            return string.Empty;
        }

        return cleanedText;
    }

    public ParsedDocument CleanDocument(
        ParsedDocument document)
    {
        var cleanedParagraphs =
            document.Paragraphs
                .Select(paragraph =>
                    new ParsedParagraph
                    {
                        Text =
                            CleanText(
                                paragraph.Text
                            ),

                        StyleId =
                            paragraph.StyleId,

                        IsBold =
                            paragraph.IsBold,

                        Alignment =
                            paragraph.Alignment
                    }
                )
                .Where(paragraph =>
                    !string.IsNullOrWhiteSpace(
                        paragraph.Text
                    )
                )
                .ToList();

        return new ParsedDocument
        {
            Title =
                CleanText(
                    document.Title
                ),

            Paragraphs =
                cleanedParagraphs
        };
    }

    private static string NormalizeInput(
        string text)
    {
        var normalized =
            text.Normalize(
                NormalizationForm.FormKC
            );

        normalized =
            normalized
                .Replace('\u00A0', ' ')
                .Replace('\u2007', ' ')
                .Replace('\u202F', ' ')
                .Replace("\u200B", string.Empty)
                .Replace("\u200C", string.Empty)
                .Replace("\u200D", string.Empty)
                .Replace("\uFEFF", string.Empty);

        normalized =
            MultipleWhitespaceRegex.Replace(
                normalized,
                " "
            );

        return normalized.Trim();
    }

    private static string RemoveLeadingContactLabels(
        string text)
    {
        var result = text;

        while (true)
        {
            var match =
                LeadingContactLabelRegex.Match(
                    result
                );

            if (!match.Success)
            {
                break;
            }

            result =
                result[match.Length..];

            result =
                LeadingSeparatorRegex.Replace(
                    result,
                    string.Empty
                );

            result =
                result.Trim();
        }

        return result;
    }

    private static string NormalizeText(
        string text)
    {
        var result =
            MultipleWhitespaceRegex.Replace(
                text,
                " "
            );

        result =
            Regex.Replace(
                result,
                @"\s*\|\s*",
                " | "
            );

        result =
            Regex.Replace(
                result,
                @"\s*/\s*",
                " / "
            );

        result =
            LeadingSeparatorRegex.Replace(
                result,
                string.Empty
            );

        result =
            TrailingSeparatorRegex.Replace(
                result,
                string.Empty
            );

        return result.Trim();
    }
}