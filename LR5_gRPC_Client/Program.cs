using Grpc.Net.Client;
using Library.GrpcClient;

var channel = GrpcChannel.ForAddress("https://localhost:7050");
var client = new LibraryService.LibraryServiceClient(channel);

#region Methods

async Task<GetBookResponse> GetBooksAsync(int page, int pageSize)
{
    return await client.GetBooksAsync(new GetBooksRequest
    {
        Page = page,
        PageSize = pageSize
    });
}

async Task<GetBookResponse> GetBookAsync(string search)
{
    return await client.GetBookAsync(new GetBookRequest
    {
        Name = search
    });
}

async Task<AddBookResponse> AddBookAsync(string name, string authorName)
{
    return await client.AddBookAsync(new AddBookRequest { Name = name, AuthorName = authorName });
}

#endregion

await AddBookAsync("First Book", "TestAuthor");
await AddBookAsync("Second Book", "TestAuthor");
await AddBookAsync("Third", "QwertyZaza");


var booksResponse = await GetBooksAsync(1, 10);

Console.WriteLine("Books:");
foreach (Book b in booksResponse.Books)
{
    Console.WriteLine($"{b.Id}: {b.Name} ({b.AuthorName})");
}

var bookResponse = await GetBookAsync("Book");

Console.WriteLine("\nSearch results:");
foreach (Book b in bookResponse.Books)
{
    Console.WriteLine($"{b.Id}: {b.Name} ({b.AuthorName})");
}