﻿namespace Mock.API.Services.Common.Chaos
{
    using Microsoft.AspNetCore.Http;
    using Polly;
    using Polly.Contrib.Simmy;
    using Polly.Contrib.Simmy.Latency;
    using Polly.Contrib.Simmy.Outcomes;
    using Polly.Registry;
    using Polly.Wrap;

    public static class SimmyExtensions
    {
        private static OperationChaosSetting GetOperationChaosSettings(this Context context) => context.GetChaosSettings()?.GetSettingsFor(context.OperationKey)!;

        private static readonly Task<bool> NotEnabled = Task.FromResult(false);

        private static readonly Task<double> NoInjectionRate = Task.FromResult<double>(0);

        private static readonly Task<Exception> NoExceptionResult = Task.FromResult<Exception>(null!);

        private static readonly Task<IResult> NoResult = Task.FromResult<IResult>(null!);

        private static readonly Task<TimeSpan> NoLatency = Task.FromResult(TimeSpan.Zero);

        /// <summary>
        /// Add chaos-injection policies to every policy returning <see cref="IAsyncPolicy{HttpResponseMessage}"/>
        /// in the supplied <paramref name="registry"/>
        /// </summary>
        /// <param name="registry">The <see cref="IPolicyRegistry{String}"/> whose policies should be decorated with chaos policies.</param>
        /// <returns>The policy registry.</returns>
        public static IPolicyRegistry<string> AddChaosInjectors(this IPolicyRegistry<string> registry)
        {
            foreach (KeyValuePair<string, IsPolicy> policyEntry in registry)
            {
                if (policyEntry.Value is AsyncPolicyWrap policy)
                {
                    registry[policyEntry.Key] = policy
                            .WrapAsync(MonkeyPolicy.InjectExceptionAsync(with =>
                                with.Fault(GetException)
                                    .InjectionRate(GetInjectionRate)
                                    .EnabledWhen(GetEnabled)))
                            .WrapAsync(MonkeyPolicy.InjectResultAsync<IResult>(with =>
                                with.Result(GetResult)
                                    .InjectionRate(GetInjectionRate)
                                    .EnabledWhen(GetResultEnabled)))
                            .WrapAsync(MonkeyPolicy.InjectLatencyAsync<IResult>(with =>
                                with.Latency(GetLatency)
                                    .InjectionRate(GetInjectionRate)
                                    .EnabledWhen(GetEnabled)));
                }
            }

            return registry;
        }

        private static Task<bool> GetEnabled(Context context, CancellationToken token)
        {
            OperationChaosSetting chaosSettings = context.GetOperationChaosSettings();
            if (chaosSettings == null) return NotEnabled;

            return Task.FromResult(chaosSettings.Enabled);
        }

        private static Task<Double> GetInjectionRate(Context context, CancellationToken token)
        {
            OperationChaosSetting chaosSettings = context.GetOperationChaosSettings();
            if (chaosSettings == null) return NoInjectionRate;

            return Task.FromResult(chaosSettings.InjectionRate);
        }

        private static Task<Exception> GetException(Context context, CancellationToken token)
        {
            OperationChaosSetting chaosSettings = context.GetOperationChaosSettings();
            if (chaosSettings == null) return NoExceptionResult;

            string exceptionName = chaosSettings.Exception!;
            if (String.IsNullOrWhiteSpace(exceptionName)) return NoExceptionResult;

            try
            {
                Type exceptionType = Type.GetType(exceptionName)!;
                if (exceptionType == null) return NoExceptionResult;

                if (!typeof(Exception).IsAssignableFrom(exceptionType)) return NoExceptionResult;

                var instance = Activator.CreateInstance(exceptionType);
                return Task.FromResult(instance as Exception)!;
            }
            catch
            {
                return NoExceptionResult;
            }
        }

        private static Task<bool> GetResultEnabled(Context context, CancellationToken token)
        {
            if (GetResult(context, CancellationToken.None) == NoResult) return NotEnabled;

            return GetEnabled(context, token);
        }

        private static Task<IResult> GetResult(Context context, CancellationToken token)
        {
            OperationChaosSetting chaosSettings = context.GetOperationChaosSettings();
            if (chaosSettings == null) return NoResult;

            int statusCode = chaosSettings.StatusCode;

            if (statusCode < 200) return NoResult;

            try
            {
                IResult result = Results.StatusCode(statusCode);
                return Task.FromResult(result);
            }
            catch
            {
                return NoResult;
            }
        }

        private static Task<TimeSpan> GetLatency(Context context, CancellationToken token)
        {
            OperationChaosSetting chaosSettings = context.GetOperationChaosSettings();
            if (chaosSettings == null) return NoLatency;

            int milliseconds = chaosSettings.LatencyMs;
            if (milliseconds <= 0) return NoLatency;

            return Task.FromResult(TimeSpan.FromMilliseconds(milliseconds));
        }
    }
}