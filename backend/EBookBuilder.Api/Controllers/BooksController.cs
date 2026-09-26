using EBookBuilder.Api.DTOs;
using EBookBuilder.Api.Exceptions;
using EBookBuilder.Api.Services.Books;
using Microsoft.AspNetCore.Mvc;

namespace EBookBuilder.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly IBookService _bookService;

    private readonly ILogger<BooksController>
        _logger;

    public BooksController(
        IBookService bookService,
        ILogger<BooksController> logger)
    {
        _bookService = bookService;
        _logger = logger;
    }

    /*
     * GET
     * /api/books/1
     */

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var book =
            await _bookService.GetByIdAsync(
                id,
                cancellationToken
            );

        if (book is null)
        {
            return NotFound(
                new
                {
                    message =
                        ErrorMessages.BookNotFound
                }
            );
        }

        return Ok(book);
    }

    /*
     * POST
     * /api/books
     */

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Create(
        [FromForm] CreateBookRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result =
                await _bookService.CreateAsync(
                    request,
                    cancellationToken
                );

            return CreatedAtAction(
                nameof(GetById),
                new
                {
                    id = result.Id
                },
                result
            );
        }
        catch (BookRequestException exception)
        {
            return BadRequest(
                new
                {
                    message =
                        exception.Message
                }
            );
        }
        catch (Exception exception)
        {
            /*
             * Kullanıcıya teknik exception
             * göstermiyoruz.
             */

            _logger.LogError(
                exception,
                "Kitap oluşturulurken beklenmeyen bir hata oluştu."
            );

            return StatusCode(
                StatusCodes
                    .Status500InternalServerError,
                new
                {
                    message =
                        ErrorMessages
                            .UnexpectedError
                }
            );
        }
    }
}