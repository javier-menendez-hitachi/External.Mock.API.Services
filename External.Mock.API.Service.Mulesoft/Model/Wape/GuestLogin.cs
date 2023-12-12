namespace External.Mock.API.Service.Mulesoft.Model.Wape
{
    using System.Text.Json.Serialization;

    public class GuestLogin
    {
        [JsonPropertyName("lastLoginDate")]
        public DateTime LastLoginDate { get; set; } = DateTime.UtcNow;
    }
}
