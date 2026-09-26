using EBookBuilder.Api.Models.Processing;

namespace EBookBuilder.Api.Services.Cleaning;

public interface IContactCleanerService
{
    string CleanText(string text);

    ParsedDocument CleanDocument(
        ParsedDocument document
    );
}