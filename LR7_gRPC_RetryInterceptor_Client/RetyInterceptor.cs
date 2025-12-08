using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LR7_gRPC_RetryInterceptor_Client
{
    public class RetryInterceptor : Interceptor
    {
        private readonly RetryOptions _options;
        private readonly ILogger<RetryInterceptor> _logger;
        private readonly Random _rng = new();

        public RetryInterceptor(IOptions<RetryOptions> options, ILogger<RetryInterceptor> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        private int ComputeDelayMs(int attempt)
        {
            if (attempt <= 1) return 0;

            var delay = _options.Strategy switch
            {
                DelayStrategy.Fixed => _options.InitialDelayMs,
                DelayStrategy.Linear => _options.InitialDelayMs * (attempt - 1),
                DelayStrategy.Exponential => _options.InitialDelayMs * Math.Pow(_options.BackoffFactor, attempt - 2),
                _ => throw new ArgumentOutOfRangeException()
            };

            delay = Math.Min(delay, _options.MaxDelayMs);

            if (_options.JitterMs > 0)
            {
                var jitter = _rng.Next(-_options.JitterMs, _options.JitterMs + 1);
                delay = Math.Max(0, delay + jitter);
            }

            return (int)delay;
        }

        private bool ShouldRetry(Exception ex)
        {
            if (ex is RpcException rpc)
            {
                return _options.RetryOnStatusCodes.Contains(rpc.Status.StatusCode);
            }

            return _options.AdditionalRetryPredicate?.Invoke(ex) ?? false;
        }

        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
            TRequest request,
            ClientInterceptorContext<TRequest, TResponse> context,
            AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            var responseTask = RetryUnaryAsync(request, context, continuation, CancellationToken.None);
            var firstCall = continuation(request, context);
            return new AsyncUnaryCall<TResponse>(
                responseTask,
                firstCall.ResponseHeadersAsync,
                firstCall.GetStatus,
                firstCall.GetTrailers,
                firstCall.Dispose);
        }

        private async Task<TResponse> RetryUnaryAsync<TRequest, TResponse>(
            TRequest request,
            ClientInterceptorContext<TRequest, TResponse> context,
            AsyncUnaryCallContinuation<TRequest, TResponse> continuation,
            CancellationToken cancellationToken)
            where TRequest : class
            where TResponse : class
        {
            Exception lastEx = null;
            var max = Math.Max(1, _options.MaxAttempts);

            for (var attempt = 1; attempt <= max; attempt++)
            {
                try
                {
                    _logger.LogInformation("Attempt {Attempt}/{Max} for {Method}", attempt, max, context.Method.FullName);
                    var call = continuation(request, context);
                    var response = await call.ResponseAsync.ConfigureAwait(false);
                    return response;
                }
                catch (Exception ex) when (!(ex is OperationCanceledException))
                {
                    lastEx = ex;
                    if (!ShouldRetry(ex) || attempt == max)
                        throw;

                    int delay = ComputeDelayMs(attempt + 1);
                    _logger.LogWarning(ex, "Retrying {Method} after {Delay}ms", context.Method.FullName, delay);
                    await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                }
            }

            throw lastEx ?? new Exception("gRPC retry failed without exception.");
        }
    }
}
