using Grpc.Core;
using gRPC_Server;

namespace gRPC_Server.Services;

public class WeatherServicee : gRPC_Server.WeatherService.WeatherServiceBase
{
    public override async Task SubscribeToCity(GetWeatherRequest request, IServerStreamWriter<WeatherResponse> responseStream, ServerCallContext context)
    {
        var random = new Random();
        var conditions = new[] { "Sunny", "Rainy", "Cloudy", "Snowy", "Windy" };

        try
        {
            while (!context.CancellationToken.IsCancellationRequested)
            {
                var response = new WeatherResponse
                {
                    City = request.City,
                    Temperature = Math.Round(random.NextDouble() * 40 - 10, 1),
                    Humidity = random.Next(30, 90),
                    Condition = conditions[random.Next(conditions.Length)],
                    Timestamp = DateTime.Now.ToString("HH:mm:ss")
                };

                await responseStream.WriteAsync(response);

                await Task.Delay(TimeSpan.FromSeconds(3), context.CancellationToken);
            }
        }
        catch (TaskCanceledException)
        {
            Console.WriteLine($"Client unsubscribed from city: {request.City}");
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }
}