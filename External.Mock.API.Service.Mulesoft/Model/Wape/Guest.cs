namespace External.Mock.API.Service.Mulesoft.Model.Wape
{
    using System.Text.Json.Serialization;

    public class Guest
    {
        [JsonPropertyName("guid")]
        public Guid Id { get; set; }
        [JsonPropertyName("contactId")]
        public string? ContactId { get; set; }
        [JsonPropertyName("guestId")]
        public string? GuestId { get; set; }
        [JsonPropertyName("firstName")]
        public string? FirstName { get; set; }
        [JsonPropertyName("lastName")]
        public string? LastName { get; set; }
        [JsonPropertyName("email")]
        public string? Email { get; set; }
        [JsonPropertyName("postcode")]
        public string? Postcode { get; set; }
        [JsonPropertyName("county")]
        public string? County { get; set; }
        [JsonPropertyName("country")]
        public string? Country { get; set; }
        [JsonPropertyName("dateOfBirth")]
        public DateTime? DateOfBirth { get; set; }
        [JsonPropertyName("gender")]
        public string? Gender { get; set; }
        [JsonPropertyName("preferredLanguage")]
        public string? PreferredLanguage { get; set; }
        [JsonPropertyName("originalVillage")]
        public string? OriginalVillage { get; set; }
        [JsonPropertyName("primaryVillage")]
        public string? PrimaryVillage { get; set; }
        [JsonPropertyName("leadSource")]
        public string? LeadSource { get; set; }
        [JsonPropertyName("leadSourceText")]
        public string? LeadSourceText { get; set; }
        [JsonPropertyName("emailOptOut")]
        public bool? EmailOptOut { get; set; }
        [JsonPropertyName("emailConsentDate")]
        public DateTime? EmailConsentDate { get; set; }
        [JsonPropertyName("singleOptIn")]
        public bool? SingleOptIn { get; set; }
        [JsonPropertyName("isNative")]
        public bool? IsNative { get; set; }
        [JsonPropertyName("barcodeNumber")]
        public string? BarcodeNumber { get; set; }
        [JsonPropertyName("barcodeOfferId")]
        public string? BarcodeOfferId { get; set; }
        [JsonPropertyName("barcodeUrl")]
        public string? BarcodeUrl { get; set; }
        [JsonPropertyName("isProfessionalBuyer")]
        public bool? IsProfessionalBuyer { get; set; }
        [JsonPropertyName("isAnonymised")]
        public bool? IsAnonymised { get; set; }
        [JsonPropertyName("city")]
        public string? City { get; set; }
        [JsonPropertyName("street")]
        public string? Street { get; set; }
        [JsonPropertyName("mobilePhone")]
        public string? MobilePhone { get; set; }
        [JsonPropertyName("otherVillagesOfInterest")]
        public List<string>? OtherVillagesOfInterest { get; set; }
        [JsonPropertyName("mobileOptOut")]
        public bool? MobileOptOut { get; set; }
        [JsonPropertyName("postOptOut")]
        public bool? PostOptOut { get; set; }
        [JsonPropertyName("mobileConsentDate")]
        public DateTime? MobileConsentDate { get; set; }
        [JsonPropertyName("postConsentDate")]
        public DateTime? PostConsentDate { get; set; }
        [JsonPropertyName("annualPlannedVisits")]
        public string? AnnualPlannedVisits { get; set; }
        [JsonPropertyName("isPrivateClient")]
        public bool? IsPrivateClient { get; set; }
        [JsonPropertyName("membershipActivationStatus")]
        public string? MembershipActivationStatus { get; set; }
        [JsonPropertyName("guestType")]
        public string? GuestType { get; set; }
    }
}


