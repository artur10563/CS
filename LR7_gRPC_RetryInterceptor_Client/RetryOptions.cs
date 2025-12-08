using Grpc.Core;

namespace LR7_gRPC_RetryInterceptor_Client;

public enum DelayStrategy
{
    Fixed,
    Linear,
    Exponential
}

public class RetryOptions
{
    public int MaxAttempts { get; set; } = 3;
    public int InitialDelayMs { get; set; } = 200;
    public int MaxDelayMs { get; set; } = 10000;
    public double BackoffFactor { get; set; } = 2.0;
    public int JitterMs { get; set; } = 100;
    public DelayStrategy Strategy { get; set; } = DelayStrategy.Exponential;
    public HashSet<StatusCode> RetryOnStatusCodes { get; set; } = [StatusCode.Unavailable, StatusCode.DeadlineExceeded, StatusCode.ResourceExhausted];
    public Func<Exception, bool>? AdditionalRetryPredicate { get; set; } = null;
}