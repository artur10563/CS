using Grpc.Core;
using LR7_gRPC_RetryInterceptor.Server;

namespace LR7_gRPC_RetryInterceptor.Services
{
    public class GreeterService : Greeter.GreeterBase
    {
        private static readonly Random _rng = new();

        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            if (_rng.NextDouble() < 0.5)
            {
                throw new RpcException(new Status(StatusCode.Unavailable, "Random failure"));
            }

            return Task.FromResult(new HelloReply
            {
                Message = $"Hello, {request.Name}! (Succeeded)"
            });
        }
    }
}