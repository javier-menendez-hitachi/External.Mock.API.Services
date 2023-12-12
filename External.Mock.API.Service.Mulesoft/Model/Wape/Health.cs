namespace External.Mock.API.Service.Mulesoft.Model.Wape
{
    using System;
    using System.Text.Json.Serialization;

    public class Health
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = "Success";

        [JsonPropertyName("dateTime")]
        public DateTime DateTime { get; set; } = DateTime.UtcNow;

        [JsonPropertyName("apiName")]
        public string ApiName { get; set; } = "vr-exp-wape-v2-mock";

        [JsonPropertyName("apiVersion")]
        public string ApiVersion { get; set; } = "2.0.0-MOCK";
    }
}
