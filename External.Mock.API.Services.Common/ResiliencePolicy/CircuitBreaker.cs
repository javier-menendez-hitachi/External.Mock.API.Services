namespace Mock.API.Services.Common.ResiliencePolicy
{
    using Microsoft.Extensions.Logging;
    using Polly;
    using Polly.Wrap;

    public class CircuitBreaker : ICircuitBreaker
    {
        private readonly ILogger? _logger;

        private AsyncPolicyWrap? _instance;

        public CircuitBreaker(Context context)
        {
            if (!context.TryGetLogger(out var logger)) return;

            _logger = logger;
        }

        public CircuitBreaker(ILogger logger)
        {
            _logger = logger;
        }

        public AsyncPolicyWrap GetInstance()
        {
            if (_instance == null)
            {
                _instance = CreatePolicy();
            }
            return _instance;
        }

        private AsyncPolicyWrap CreatePolicy()
        {
            return Policy
                .Handle<Exception>()
                .CircuitBreakerAsync(5, TimeSpan.FromSeconds(15),
                    onBreak: (_, _) =>
                    {
                        _logger?.LogError($"# {DateTime.Now:HH:mm:ss} # Open (onBreak)");
                    },
                    onReset: () =>
                    {
                        _logger?.LogInformation($"# {DateTime.Now:HH:mm:ss} # Closed (onReset)");
                    },
                    onHalfOpen: () =>
                    {
                        _logger?.LogWarning($"# {DateTime.Now:HH:mm:ss} # Half Open (onHalfOpen)");
                    })
                .WrapAsync(Policy.BulkheadAsync(12));
        }
    }
}