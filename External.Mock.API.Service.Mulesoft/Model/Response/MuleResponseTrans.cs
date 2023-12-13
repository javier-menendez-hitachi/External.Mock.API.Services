using System.Text.Json.Serialization;

namespace External.Mock.API.Service.Mulesoft.Model.Response
{
    public class MuleResponseTrans
    {
        [JsonPropertyName("recordId")]
        public Guid RecordId { get; set; } = Guid.NewGuid();
        
        [JsonPropertyName("transactionId")]
        public Guid TransactionId { get; set; } = Guid.NewGuid();

        [JsonPropertyName("status")]
        public string? Status { get; set; } = "Success";

        [JsonPropertyName("message")]
        public string? Message { get; set; } = "This is a Mock";
        
    }
}
