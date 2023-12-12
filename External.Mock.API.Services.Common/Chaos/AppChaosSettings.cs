namespace Mock.API.Services.Common.Chaos
{
    using System.Collections.Generic;
    using System.Linq;

    public class AppChaosSettings
    {
        public List<OperationChaosSetting>? OperationChaosSettings { get; set; }

        public OperationChaosSetting? GetSettingsFor(string operationKey) => OperationChaosSettings?.SingleOrDefault(i => i.OperationKey == operationKey);
    }
}