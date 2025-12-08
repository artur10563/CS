

using LR7_gRPC_RetryInterceptor.Services;

namespace LR7_gRPC_RetryInterceptor;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddGrpc();
        
        builder.WebHost.ConfigureKestrel(options =>
        {
            options.ListenLocalhost(7172, o => o.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2);
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        app.MapGrpcService<GreeterService>();
        app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

        app.Run();
    }
}