using EBookBuilder.Api.DTOs;

namespace EBookBuilder.Api.Services.Books;

public interface IBookService
{
    Task<BookResponse> CreateAsync(
        CreateBookRequest request,
        CancellationToken cancellationToken
    );

    Task<BookResponse?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken
    );
}