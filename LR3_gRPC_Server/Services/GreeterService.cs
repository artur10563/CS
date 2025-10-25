using Grpc.Core;
using gRPC_Server;

namespace gRPC_Server.Services;

public class GreeterService : Greeter.GreeterBase
{
    private readonly ILogger<GreeterService> _logger;

    public GreeterService(ILogger<GreeterService> logger)
    {
        _logger = logger;
    }

    public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
    {
        return Task.FromResult(new HelloReply
        {
            Message = "Hello " + request.Name
        });
    }

    public override async Task StreamHello(HelloRequest request, IServerStreamWriter<HelloReply> responseStream, ServerCallContext context)
    {
        for (int i = 0; i < 10; i++)
        {
            await responseStream.WriteAsync(new HelloReply
            {
                Message = $"Hello {request.Name} at {DateTime.Now}! Request #{i + 1}"
            });

            await Task.Delay(1000);
        }
    }

    public override async Task<HelloReply> CollectHellos(IAsyncStreamReader<HelloRequest> requestStream, ServerCallContext context)
    {
        var names = new List<string>();
        await foreach (var request in requestStream.ReadAllAsync())
        {
            names.Add(request.Name);
        }

        return new HelloReply { Message = $"Hello to {names.Count} people! Hello, {string.Join(",", names)}" };
    }

    public override async Task CollectAndStreamHellos(IAsyncStreamReader<HelloRequest> requestStream, IServerStreamWriter<HelloReply> responseStream, ServerCallContext context)
    {
        await foreach (var request in requestStream.ReadAllAsync())
        {
            var name = request.Name;

            await responseStream.WriteAsync(new HelloReply { Message = $"Processing name {name} at {DateTime.Now}." });
            
            // Simulate processing data
            await Task.Delay(1000);
            
            await responseStream.WriteAsync(new HelloReply { Message = $"Hello {name} at {DateTime.Now}!" });
            
        }
    }
}