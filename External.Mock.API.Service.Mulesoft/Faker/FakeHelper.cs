namespace External.Mock.API.Service.Mulesoft.Faker
{
    using Bogus;
    using External.Mock.API.Service.Mulesoft.Model.Wape;
    using External.Mock.API.Service.Mulesoft.Model.WebsiteLoyalty;
    using External.Mock.API.Service.Mulesoft.Model.WebsiteMembersLoyalty;
    using System.Collections.Generic;
    using System.Numerics;
    using System.Text.RegularExpressions;

    public static partial class FakeHelper
    {
        private readonly static string[] categoryIds = ["a7W7Z0000009ecdUAA", "a7W7Z0000009ehOUAQ", "a7W7Z0000009ecEUAQ", "a7W7Z0000009ebfUAA", "a7W7Z0000009edbUAA"];
        public static Guest GenFakeGuest(Guid guid, string? email = null)
        {
            // Generate Barcode
            var barcode = GuidToBigInteger(Guid.NewGuid()).ToString();

            // Generate Fake Guest
            var faker = new Faker<Guest>().StrictMode(true)
            .RuleFor(u => u.Id, f => guid)
            .RuleFor(u => u.FirstName, f => f.Name.FirstName())
            .RuleFor(u => u.LastName, f => f.Name.LastName())
            .RuleFor(u => u.Email, f => email ?? f.Internet.Email())
            .RuleFor(u => u.Postcode, f => f.Address.ZipCode())
            .RuleFor(u => u.County, f => f.Address.County())
            .RuleFor(u => u.Country, f => f.Address.Country())
            .RuleFor(u => u.Gender, f => f.PickRandom(new List<string> { "1", "2", "3", "4" }))
            .RuleFor(u => u.PreferredLanguage, f => f.Locale)
            .RuleFor(u => u.OriginalVillage, f => f.PickRandom(new List<string> { "BV", "LV", "LR", "KV" }))
            .RuleFor(u => u.PrimaryVillage, f => f.PickRandom(new List<string> { "BV", "LV", "LR", "KV" }))
            .RuleFor(u => u.LeadSource, f => f.Random.Word())
            .RuleFor(u => u.LeadSourceText, f => f.Random.Word())
            .RuleFor(u => u.DateOfBirth, f => f.Date.Past(80, DateTime.UtcNow))
            .RuleFor(u => u.EmailConsentDate, f => f.Date.Past(2, DateTime.UtcNow))
            .RuleFor(u => u.EmailOptOut, f => f.Random.Bool())
            .RuleFor(u => u.SingleOptIn, f => f.Random.Bool())
            .RuleFor(u => u.IsNative, f => f.Random.Bool())
            .RuleFor(u => u.BarcodeNumber, f => barcode)
            .RuleFor(u => u.BarcodeOfferId, f => "17453")
            .RuleFor(u => u.BarcodeUrl, f => $"https://barcode.valueretail.com/gensvg?type=DataMatrix&fmt=gif&qz=disable&msg={barcode}")
            .RuleFor(u => u.City, f => f.Address.City())
            .RuleFor(u => u.Street, f => f.Address.StreetAddress())
            .RuleFor(u => u.ContactId, f => GuidToBigInteger(Guid.NewGuid()).ToString())
            .RuleFor(u => u.GuestId, f => GenerateSalesforceId())
            .RuleFor(u => u.MembershipActivationStatus, f => "Active")
            .RuleFor(u => u.GuestType, f => "Member")
            .RuleFor(u => u.IsProfessionalBuyer, f => false)
            .RuleFor(u => u.IsAnonymised, f => false)
            .RuleFor(u => u.MobilePhone, f => f.Phone.PhoneNumber())
            .RuleFor(u => u.OtherVillagesOfInterest, f => new List<string>())
            .RuleFor(u => u.MobileOptOut, f => f.Random.Bool())
            .RuleFor(u => u.PostOptOut, f => f.Random.Bool())
            .RuleFor(u => u.MobileConsentDate, f => f.Date.Past(2, DateTime.UtcNow))
            .RuleFor(u => u.PostConsentDate, f => f.Date.Past(2, DateTime.UtcNow))
            .RuleFor(u => u.AnnualPlannedVisits, f => string.Empty)
            .RuleFor(u => u.IsPrivateClient, f => false);

            return faker.Generate();
        }

        public static GuestInfo GenFakeGuestInfo()
        {
            var faker = new Faker<GuestInfo>().StrictMode(true)
            .RuleFor(u => u.TotalCustomerPoints, f => f.Random.Int(0,10000))
            .RuleFor(u => u.CurrentSpendTierId, f => GenerateSalesforceId())
            .RuleFor(u => u.CurrentBehavioralTierId, f => f.PickRandom(new List<string> { "New", "Active", "Lapsing", "Passive", "Dormant" }));

            return faker.Generate();
        }

        public static SegmentInfo GenFakeSegmentInfo()
        {
            var faker = new Faker<SegmentInfo>().StrictMode(true)
            .RuleFor(u => u.AmountTo, f => Math.Round(f.Random.Double(100, 1000), 2))
            .RuleFor(u => u.AmountFrom, f => Math.Round(f.Random.Double(1, 100), 2))
            .RuleFor(u => u.Id, f => GenerateSalesforceId())
            .RuleFor(u => u.Name, f => f.PickRandom(new List<string> { "Level 1", "Level 2", "Level 3" }))
            .RuleFor(u => u.Sequence, f => f.Random.Number(1, 10));

            return faker.Generate();
        }

        public static List<CustomerTreatCategory> GenFakeCustomerTreatCategories()
        {
            List<CustomerTreatCategory> result = [];

            foreach (var id in categoryIds)
            {
                result.Add(GetCustomerTreatCategoryFaker(id).Generate());
            }
            return result;
        }

        public static RewardsInfo GenFakeRewardsInfo()
        {
            var offersNumber = new Random().Next(1, 11);
            return new RewardsInfo { Offers = GetOfferFaker().Generate(offersNumber) };
        }

        private static Faker<CustomerOffer> GetOfferFaker()
        {
            return new Faker<CustomerOffer>()
                .RuleFor(co => co.OfferId, f => f.Random.Uuid().ToString())
                .RuleFor(co => co.OfferIdCode, f => f.Random.AlphaNumeric(10))
                .RuleFor(co => co.Title, f => f.Commerce.ProductName())
                .RuleFor(co => co.Title2, f => f.Commerce.ProductName())
                .RuleFor(co => co.PassbookTitle, f => f.Commerce.ProductName())
                .RuleFor(co => co.Description, f => f.Lorem.Paragraph())
                .RuleFor(co => co.ImageUrl, f => f.Image.PicsumUrl())
                .RuleFor(co => co.Terms, f => f.Lorem.Sentence())
                .RuleFor(co => co.BarcodeUrl, f => f.Image.PicsumUrl())
                .RuleFor(co => co.BarcodeNumber, f => f.Commerce.Ean13())
                .RuleFor(co => co.Conditions, f => f.Lorem.Sentence())
                .RuleFor(co => co.Instructions, f => f.Lorem.Sentence())
                .RuleFor(co => co.IsActive, f => f.Random.Bool())
                .RuleFor(co => co.Redeemed, f => f.Random.Bool())
                .RuleFor(co => co.VillageID, f => f.PickRandom(new List<string> { "BV", "LV", "LR" }))
                .RuleFor(co => co.ConditionsUrl, f => f.Internet.Url())
                .RuleFor(co => co.CategoryId, f => f.PickRandom(categoryIds));
        }

        private static Faker<CustomerTreatCategory> GetCustomerTreatCategoryFaker(string id)
        {
            var faker = new Faker<CustomerTreatCategory>()
                .RuleFor(ctc => ctc.Id, f => id)
                .RuleFor(ctc => ctc.Name, f => f.Commerce.ProductName())
                .RuleFor(ctc => ctc.Description, f => f.Lorem.Sentence())
                .RuleFor(ctc => ctc.Order, f => f.Random.Number(1, 10).ToString())
                .RuleFor(ctc => ctc.IsToHide, f => f.Random.Bool());

            return faker;
        }

        /// <summary>
        /// Generate a BigInteger given a Guid. Returns a number from 0 to 2^128
        /// 0 to 340,282,366,920,938,463,463,374,607,431,768,211,456
        /// </summary>
        public static BigInteger GuidToBigInteger(Guid guid)
        {
            BigInteger l_retval = 0;
            byte[] ba = guid.ToByteArray();
            int i = ba.Length;
            foreach (byte b in ba)
            {
                l_retval += b * BigInteger.Pow(256, --i);
            }
            return l_retval;
        }

        static string GenerateSalesforceId()
        {
            // Generate random Salesforce Id
            var randomString = Convert.ToBase64String(Guid.NewGuid().ToByteArray())[..7];
            var rx = SalesforceIdRegex();
            while (!rx.IsMatch(randomString))
            {
                randomString = Convert.ToBase64String(Guid.NewGuid().ToByteArray())[..7];
            }
            return $"0015E000022{randomString}";
        }

        [GeneratedRegex("[A-Za-z]{7}")]
        private static partial Regex SalesforceIdRegex();
    }
}