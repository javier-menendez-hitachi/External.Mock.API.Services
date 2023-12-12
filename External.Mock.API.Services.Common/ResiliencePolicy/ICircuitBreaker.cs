namespace Mock.API.Services.Common.ResiliencePolicy
{
    using Polly.Wrap;

    public interface ICircuitBreaker
    {
        public AsyncPolicyWrap GetInstance();
    }
}