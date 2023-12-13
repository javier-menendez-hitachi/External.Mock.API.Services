namespace External.Mock.API.Service.Mulesoft.Model.Wape
{
    using System.Text.Json.Serialization;

    public class SendEmail
    {
        [JsonPropertyName("attributes")]
        public List<SendEmailAttribute>? Attributes { get; set; }
        [JsonPropertyName("toAddress")]
        public string? ToAddress { get; set; }
    }
}


