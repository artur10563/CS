// using LR4_gRPC_Chat_Server.Services;

using LR4_gRPC_Chat_Server.Services;

namespace LR4_gRPC_Chat_Server;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddGrpc();

        // CORS to allow gRPC requests from any origin
        builder.Services.AddCors(o => o.AddPolicy("AllowAll", policyBuilder =>
        {
            policyBuilder.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()
                .WithExposedHeaders("Grpc-Status", "Grpc-Message",
                    "Grpc-Encoding", "Grpc-Accept-Encoding",
                    "Grpc-Status-Details-Bin");
        }));

        var app = builder.Build();
        app.UseCors("AllowAll");

        app.UseGrpcWeb(new GrpcWebOptions() { DefaultEnabled = true });

        app.MapGrpcService<ChatServiceImpl>().RequireCors();

        app.MapGet("/", () => "All gRPC service are supported by default in " +
                              "this example, and are callable from browser apps using the " +
                              "gRPC-Web protocol");
        app.Run();
    }
}