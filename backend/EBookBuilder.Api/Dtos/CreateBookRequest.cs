using Microsoft.AspNetCore.Http;

namespace EBookBuilder.Api.DTOs;

public class CreateBookRequest
{
    public string BookName { get; set; } = string.Empty;

    public List<IFormFile> Files { get; set; } = [];
}