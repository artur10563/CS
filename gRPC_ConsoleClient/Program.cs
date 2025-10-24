using Grpc.Core;
using Grpc.Net.Client;

namespace gRPC_ConsoleClient;

class Program
{
    public static async Task RunExamplesAsync()
    {
        using var channel = GrpcChannel.ForAddress(ServerUrl);
        var client = new Greeter.GreeterClient(channel);

        #region Unary - Single Request, Single Response

        var singleGreeting = client.SayHello(new HelloRequest { Name = "103563Artur10563" });
        Console.WriteLine(singleGreeting.Message);

        #endregion

        #region Server Streaming - Single input multiple output

        var call = client.StreamHello(new HelloRequest { Name = "Artur10563" });
        await foreach (var reply in call.ResponseStream.ReadAllAsync())
        {
            Console.WriteLine($"{reply.Message} - {reply.CalculateSize()}");
        }

        #endregion

        #region Client Streaming - Multiple input single output

        using var collectHellos = client.CollectHellos();
        var names = new string[] { "artur10563", "arlur10563", "arnur10563" };

        foreach (var name in names)
        {
            await collectHellos.RequestStream.WriteAsync(new HelloRequest { Name = name });
        }

        // End sending data
        await collectHellos.RequestStream.CompleteAsync();

        // Await the call to get results
        Console.WriteLine($"{(await collectHellos).Message}");


        await foreach (var status in call.ResponseStream.ReadAllAsync())
        {
            Console.WriteLine($"Server reported: {status.Message}%");
        }

        #endregion

        #region Bidirectional Streaming - Send and Receive at the same time

        var biCall = client.CollectAndStreamHellos();

        // Start sending in background
        var sending = Task.Run(async () =>
        {
            foreach (var name in names)
            {
                await biCall.RequestStream.WriteAsync(new HelloRequest { Name = name });
            }

            await biCall.RequestStream.CompleteAsync();
        });

        // Read responses from the server as they arrive
        await foreach (var response in biCall.ResponseStream.ReadAllAsync())
        {
            Console.WriteLine(response.Message);
        }

        // Wait for sending to finish
        await sending;

        #endregion
    }

    private static readonly string ServerUrl = "http://localhost:5018";

    static async Task Main(string[] args)
    {
        // await RunExamplesAsync();
        Console.Read();
    }
}