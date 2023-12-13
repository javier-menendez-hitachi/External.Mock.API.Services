using System.Text.Json.Serialization;

namespace External.Mock.API.Service.Mulesoft.Model.Response
{
    public class MockResponse
    {
        [JsonPropertyName("isSuccess")]
        public bool IsSuccess { get; set; } = true;
        
        [JsonPropertyName("errorCode")]
        public string? ErrorCode { get; set; } = "0";

        [JsonPropertyName("errorMessage")]
        public string? ErrorMessage { get; set; } = string.Empty;

        [JsonPropertyName("message")]
        public string? Message { get; set; } = "Welcome to Mulesoft API Mock";
        
        [JsonPropertyName("statusCode")]
        public int StatusCode { get; set; } = 200;
        
        [JsonPropertyName("loggingCategory")]
        public string? LoggingCategory { get; set; } = "MulesoftApiMock";
    }
}
