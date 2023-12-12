namespace External.Mock.API.Service.Mulesoft.Model.Wape
{
    using System.Text.Json.Serialization;

    public class Reward
    {
        [JsonPropertyName("guid")]
        public Guid Guid { get; set; }

        [JsonPropertyName("rewardId")]
        public Guid RewardId { get; set; }

        [JsonPropertyName("allocationDate")]
        public DateTime? AllocationDate { get; set; }

        [JsonPropertyName("coniqOfferId ")]
        public string? ConiqOfferId { get; set; }
    }
}
