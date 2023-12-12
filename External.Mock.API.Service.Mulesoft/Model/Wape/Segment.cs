namespace External.Mock.API.Service.Mulesoft.Model.Wape
{
    using System.Text.Json.Serialization;

    public class Segment
    {
        [JsonPropertyName("guid")]
        public Guid Guid { get; set; }

        [JsonPropertyName("engagementTier")]
        public string? EngagementTier { get; set; }

        [JsonPropertyName("membershipLevel")]
        public string? MembershipLevel { get; set; }
    }
}
