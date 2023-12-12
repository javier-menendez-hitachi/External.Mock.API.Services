namespace Mock.API.Service.WAPE.Model
{
    using System;

    public class Guest
    {
        public string? guid { get; set; }
        public string? firstName { get; set; }
        public string? lastName { get; set; }
        public string? email { get; set; }
        public string? postcode { get; set; }
        public string? county { get; set; }
        public string? country { get; set; }
        public string? dateOfBirth { get; set; }
        public string? gender { get; set; }
        public string? preferredLanguage { get; set; }
        public string? originalVillage { get; set; }
        public string? primaryVillage { get; set; }
        public string? leadSource { get; set; }
        public string? leadSourceText { get; set; }
        public bool emailOptOut { get; set; }
        public DateTime emailConsentDate { get; set; }
        public bool singleOptIn { get; set; }
        public bool isNative { get; set; }
        public string? barcodeNumber { get; set; }
        public string? barcodeOfferId { get; set; }
        public string? barcodeUrl { get; set; }
    }
}
