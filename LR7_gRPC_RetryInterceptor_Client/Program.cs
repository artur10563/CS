using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Net.Client;
using LR7_gRPC_RetryInterceptor_Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Greeter = LR7_gRPC_RetryInterceptor.Server.Greeter;

class Program
{
    static async Task Main()
    {
        using var channel = GrpcChannel.ForAddress("http://localhost:7172");

        using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<RetryInterceptor>();

        var options = Options.Create(new RetryOptions
        {
            MaxAttempts = 5,
            InitialDelayMs = 200,
            BackoffFactor = 2.0,
            MaxDelayMs = 5000,
            JitterMs = 50,
            Strategy = DelayStrategy.Exponential
        });

        var interceptor = new RetryInterceptor(options, logger);

        var client = new LR7_gRPC_RetryInterceptor_Client.Greeter.GreeterClient(channel.Intercept(interceptor));

        for (int i = 0; i < 5; i++)
        {
            try
            {
                var reply = await client.SayHelloAsync(new HelloRequest { Name = $"Test-{i}" });
                Console.WriteLine(reply.Message);
            }
            catch (RpcException ex)
            {
                Console.WriteLine($"Call failed: {ex.Status}");
            }
        }
    }
}