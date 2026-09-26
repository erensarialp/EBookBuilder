namespace EBookBuilder.Api.Exceptions;

public class BookRequestException : Exception
{
    public BookRequestException(string message)
        : base(message)
    {
    }
}