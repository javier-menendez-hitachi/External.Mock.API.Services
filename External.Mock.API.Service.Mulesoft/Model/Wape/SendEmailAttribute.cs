namespace External.Mock.API.Service.Mulesoft.Model.Wape
{
    using System.Text.Json.Serialization;

    public class SendEmailAttribute
    {
        [JsonPropertyName("attributeName")]
        public string? AttributeName { get; set; }
        [JsonPropertyName("attributeValue")]
        public string? AttributeValue { get; set; }
    }
}


