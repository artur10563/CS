using Grpc.Core;
using Library.GrpcServer;

namespace LR5_gRPC_Server.Services;

public static class BookExtensions
{
    public static Book AddWithNextId(this List<Book> books, string name, string authorName)
    {
        var book = new Book
        {
            Id = books.Select(b => b.Id).DefaultIfEmpty(0).Max() + 1,
            Name = name,
            AuthorName = authorName
        };
        books.Add(book);
        return book;
    }
}

public class LibraryService : Library.GrpcServer.LibraryService.LibraryServiceBase
{
    private static readonly List<Book> Books = [];

    public override Task<AddBookResponse> AddBook(AddBookRequest request, ServerCallContext context)
    {
        var createdBook = Books.AddWithNextId(request.Name, request.AuthorName);
        return Task.FromResult(new AddBookResponse() { Id = createdBook.Id });
    }

    public override Task<GetBookResponse> GetBook(GetBookRequest request, ServerCallContext context)
    {
        var books = Books.Where(x =>
                x.Name.Contains(request.Name, StringComparison.CurrentCultureIgnoreCase)
                || x.AuthorName.Contains(request.Name, StringComparison.CurrentCultureIgnoreCase))
            .ToList();

        var response = new GetBookResponse();
        response.Books.AddRange(books);

        return Task.FromResult(response);
    }

    public override Task<GetBookResponse> GetBooks(GetBooksRequest request, ServerCallContext context)
    {
        if (request.Page < 1) request.Page = 1;
        if (request.PageSize < 1) request.PageSize = 10;
        
        var books = Books
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var response = new GetBookResponse();
        response.Books.AddRange(books);

        return Task.FromResult(response);
    }
}