using EBookBuilder.Api.Models.Processing;

namespace EBookBuilder.Api.Services.Pdf;

public interface IPdfDocumentService
{
    IReadOnlyDictionary<int, int> GenerateBook(
        string bookName,
        IReadOnlyList<PdfPaperContent> papers,
        string outputPath
    );
}