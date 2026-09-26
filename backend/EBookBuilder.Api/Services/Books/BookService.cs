using EBookBuilder.Api.Data;
using EBookBuilder.Api.DTOs;
using EBookBuilder.Api.Exceptions;
using EBookBuilder.Api.Models;
using EBookBuilder.Api.Models.Processing;
using EBookBuilder.Api.Services.Cleaning;
using EBookBuilder.Api.Services.Pdf;
using EBookBuilder.Api.Services.Word;
using Microsoft.EntityFrameworkCore;

namespace EBookBuilder.Api.Services.Books;

public class BookService : IBookService
{
    private readonly AppDbContext
        _dbContext;

    private readonly IWebHostEnvironment
        _environment;

    private readonly IWordDocumentService
        _wordDocumentService;

    private readonly IContactCleanerService
        _contactCleanerService;

    private readonly IPdfDocumentService
        _pdfDocumentService;

    public BookService(
        AppDbContext dbContext,
        IWebHostEnvironment environment,
        IWordDocumentService wordDocumentService,
        IContactCleanerService contactCleanerService,
        IPdfDocumentService pdfDocumentService)
    {
        _dbContext =
            dbContext;

        _environment =
            environment;

        _wordDocumentService =
            wordDocumentService;

        _contactCleanerService =
            contactCleanerService;

        _pdfDocumentService =
            pdfDocumentService;
    }

    public async Task<BookResponse>
        CreateAsync(
            CreateBookRequest request,
            CancellationToken cancellationToken)
    {
        ValidateRequest(
            request
        );

        var book =
            new Book
            {
                Name =
                    request
                        .BookName
                        .Trim(),

                Status =
                    BookStatus.Pending,

                PdfPath =
                    null
            };

        _dbContext
            .Books
            .Add(book);

        await _dbContext
            .SaveChangesAsync(
                cancellationToken
            );

        string? bookDirectory =
            null;

        try
        {
            book.Status =
                BookStatus.Processing;

            await _dbContext
                .SaveChangesAsync(
                    cancellationToken
                );

            var webRootPath =
                _environment
                    .WebRootPath
                ?? Path.Combine(
                    _environment
                        .ContentRootPath,
                    "wwwroot"
                );

            bookDirectory =
                Path.Combine(
                    webRootPath,
                    "storage",
                    "books",
                    book.Id.ToString()
                );

            var originalsDirectory =
                Path.Combine(
                    bookDirectory,
                    "originals"
                );

            var outputDirectory =
                Path.Combine(
                    bookDirectory,
                    "output"
                );

            Directory.CreateDirectory(
                originalsDirectory
            );

            Directory.CreateDirectory(
                outputDirectory
            );

            var pdfPapers =
                new List<
                    PdfPaperContent
                >();

            for (
                var index = 0;
                index <
                    request.Files.Count;
                index++
            )
            {
                var file =
                    request.Files[
                        index
                    ];

                var originalFileName =
                    Path.GetFileName(
                        file.FileName
                    );

                var storageFileName =
                    $"{index + 1:D2}_" +
                    $"{Guid.NewGuid():N}.docx";

                var physicalPath =
                    Path.Combine(
                        originalsDirectory,
                        storageFileName
                    );

                await using (
                    var stream =
                        new FileStream(
                            physicalPath,
                            FileMode
                                .CreateNew,
                            FileAccess
                                .Write,
                            FileShare
                                .None,
                            bufferSize:
                                81920,
                            useAsync:
                                true
                        )
                )
                {
                    await file
                        .CopyToAsync(
                            stream,
                            cancellationToken
                        );
                }

                var relativePath =
                    "/storage/books/" +
                    $"{book.Id}/" +
                    "originals/" +
                    $"{storageFileName}";

                var parsedDocument =
                    _wordDocumentService
                        .ReadDocument(
                            physicalPath
                        );

                var hasContent =
                    parsedDocument
                        .Paragraphs
                        .Any(
                            paragraph =>
                                !string
                                    .IsNullOrWhiteSpace(
                                        paragraph
                                            .Text
                                    )
                        );

                if (!hasContent)
                {
                    throw new
                        BookRequestException(
                            $"\"{originalFileName}\" " +
                            $"{ErrorMessages.DocumentHasNoContent}"
                        );
                }

                var cleanedDocument =
                    _contactCleanerService
                        .CleanDocument(
                            parsedDocument
                        );

                var hasCleanContent =
                    cleanedDocument
                        .Paragraphs
                        .Any(
                            paragraph =>
                                !string
                                    .IsNullOrWhiteSpace(
                                        paragraph
                                            .Text
                                    )
                        );

                if (!hasCleanContent)
                {
                    throw new
                        BookRequestException(
                            $"\"{originalFileName}\" " +
                            $"{ErrorMessages.DocumentHasNoContent}"
                        );
                }

                var paper =
                    new Paper
                    {
                        BookId =
                            book.Id,

                        FileName =
                            originalFileName,

                        OriginalFilePath =
                            relativePath,

                        Title =
                            cleanedDocument
                                .Title,

                        OrderIndex =
                            index + 1,

                        StartPage =
                            null
                    };

                book.Papers.Add(
                    paper
                );

                pdfPapers.Add(
                    new PdfPaperContent
                    {
                        Title =
                            cleanedDocument
                                .Title,

                        OrderIndex =
                            index + 1,

                        Paragraphs =
                            cleanedDocument
                                .Paragraphs
                    }
                );
            }

            var pdfFileName =
                CreateSafePdfFileName(
                    book.Name
                );

            var physicalPdfPath =
                Path.Combine(
                    outputDirectory,
                    pdfFileName
                );

            var startPages =
                _pdfDocumentService
                    .GenerateBook(
                        book.Name,
                        pdfPapers,
                        physicalPdfPath
                    );

            foreach (
                var paper
                in book.Papers
            )
            {
                if (
                    startPages
                        .TryGetValue(
                            paper
                                .OrderIndex,
                            out var startPage
                        )
                )
                {
                    paper.StartPage =
                        startPage;
                }
            }

            var relativePdfPath =
                "/storage/books/" +
                $"{book.Id}/" +
                "output/" +
                $"{pdfFileName}";

            book.PdfPath =
                relativePdfPath;

            book.Status =
                BookStatus.Completed;

            await _dbContext
                .SaveChangesAsync(
                    cancellationToken
                );

            return MapToResponse(
                book
            );
        }
        catch
        {
            foreach (
                var entry
                in _dbContext
                    .ChangeTracker
                    .Entries<Paper>()
                    .Where(
                        entry =>
                            entry.State ==
                            EntityState
                                .Added
                    )
            )
            {
                entry.State =
                    EntityState
                        .Detached;
            }

            if (
                bookDirectory is not
                    null &&
                Directory.Exists(
                    bookDirectory
                )
            )
            {
                Directory.Delete(
                    bookDirectory,
                    recursive:
                        true
                );
            }

            book.PdfPath =
                null;

            book.Status =
                BookStatus.Failed;

            await _dbContext
                .SaveChangesAsync(
                    cancellationToken
                );

            throw;
        }
    }

    public async Task<BookResponse?>
        GetByIdAsync(
            int id,
            CancellationToken cancellationToken)
    {
        var book =
            await _dbContext
                .Books
                .AsNoTracking()
                .Include(
                    book =>
                        book.Papers
                )
                .FirstOrDefaultAsync(
                    book =>
                        book.Id ==
                        id,
                    cancellationToken
                );

        if (book is null)
        {
            return null;
        }

        return MapToResponse(
            book
        );
    }

    private static void
        ValidateRequest(
            CreateBookRequest request)
    {
        if (
            string.IsNullOrWhiteSpace(
                request.BookName
            )
        )
        {
            throw new
                BookRequestException(
                    ErrorMessages
                        .BookNameRequired
                );
        }

        if (
            request.Files is null ||
            request.Files.Count != 10
        )
        {
            throw new
                BookRequestException(
                    ErrorMessages
                        .ExactlyTenDocumentsRequired
                );
        }

        foreach (
            var file
            in request.Files
        )
        {
            var extension =
                Path.GetExtension(
                    file.FileName
                );

            if (
                !extension.Equals(
                    ".docx",
                    StringComparison
                        .OrdinalIgnoreCase
                )
            )
            {
                throw new
                    BookRequestException(
                        ErrorMessages
                            .InvalidDocumentType
                    );
            }

            if (file.Length == 0)
            {
                throw new
                    BookRequestException(
                        $"\"{file.FileName}\" " +
                        $"{ErrorMessages.EmptyDocument}"
                    );
            }
        }
    }

    private static string
        CreateSafePdfFileName(
            string bookName)
    {
        var invalidCharacters =
            Path
                .GetInvalidFileNameChars();

        var safeName =
            new string(
                bookName
                    .Trim()
                    .Select(
                        character =>
                            invalidCharacters
                                .Contains(
                                    character
                                )
                                ? '_'
                                : character
                    )
                    .ToArray()
            );

        safeName =
            safeName.Trim();

        safeName =
            safeName.TrimEnd(
                '.'
            );

        if (
            string.IsNullOrWhiteSpace(
                safeName
            )
        )
        {
            safeName =
                "E-Kitap";
        }

        return
            $"{safeName}.pdf";
    }

    private static BookResponse
        MapToResponse(
            Book book)
    {
        return new BookResponse
        {
            Id =
                book.Id,

            Name =
                book.Name,

            Status =
                book.Status
                    .ToString(),

            PdfPath =
                book.PdfPath,

            Papers =
                book.Papers
                    .OrderBy(
                        paper =>
                            paper
                                .OrderIndex
                    )
                    .Select(
                        paper =>
                            new PaperResponse
                            {
                                Id =
                                    paper.Id,

                                FileName =
                                    paper
                                        .FileName,

                                Title =
                                    paper.Title,

                                OrderIndex =
                                    paper
                                        .OrderIndex,

                                StartPage =
                                    paper
                                        .StartPage
                            }
                    )
                    .ToList()
        };
    }
}