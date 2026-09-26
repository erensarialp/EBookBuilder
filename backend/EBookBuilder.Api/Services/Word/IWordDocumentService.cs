using EBookBuilder.Api.Models.Processing;

namespace EBookBuilder.Api.Services.Word;

public interface IWordDocumentService
{
    ParsedDocument ReadDocument(string filePath);
}