namespace Mock.API.Services.Common.Chaos
{
    public class OperationChaosSetting
    {
        public string? OperationKey { get; set; }

        public bool Enabled { get; set; }

        public double InjectionRate { get; set; }

        public int StatusCode { get; set; }

        public int LatencyMs { get; set; }

        public string? Exception { get; set; }
    }
}