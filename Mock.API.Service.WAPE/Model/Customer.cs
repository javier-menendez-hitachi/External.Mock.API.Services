namespace Mock.API.Service.WAPE.Model
{
    
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    /// <summary>
    /// This class encapsulates the legacy customer.
    /// </summary>
    [DebuggerDisplay("Id={Id} Email={Email}")]
    public class Customer : SecurityVirtualBase
    {
        public const string CnReferences = "References";

        public Customer()
        {
            mCollections[CnReferences] = new Dictionary<string, string>();
        }

        protected IEnumerable<string> ReferencesGet(string key)
        {
            key = key.ToUpperInvariant();
            return from kvp in mCollections[CnReferences] where kvp.Key.StartsWith(key) select kvp.Value;
        }

        protected void ReferenceSet(string key, string? value, bool unique = true)
        {
            string actualKey = string.Format("{0}|{1}", key.ToUpperInvariant(), (value ?? "").ToUpper());

            if (string.IsNullOrEmpty(value))
                value = null;

            if (unique)
            {
                var keys = mCollections[CnReferences].Keys.Where(k => k.StartsWith(key.ToUpperInvariant())).ToList();
                keys.ForEach(k => mCollections[CnReferences].Remove(k));
            }

            CollectionSet(CnReferences, actualKey, value!);
        }

        
        /// <summary>
        /// This is the email reference field.
        /// </summary>
        public virtual string Email
        {
            get => ReferencesGet("Email").FirstOrDefault()!;
            set => ReferenceSet("Email", value?.Trim());
        }

        public virtual string SalesforceId
        {
            get => ReferencesGet("SalesforceId").FirstOrDefault()!;
            set => ReferenceSet("SalesforceId", value);
        }

        public string BarcodeNumber
        {
            get => ReferencesGet("BarcodeNumber").FirstOrDefault()!;
            set => ReferenceSet("BarcodeNumber", value);
        }

        //<summary>
        // This is the customer id reference field(s)
        //</summary>
        public virtual string[] CustomerIds => ReferencesGet("CustomerId").ToArray();

        public string FacebookId
        {
            get => ReferencesGet("FacebookId").FirstOrDefault()!;
            set => ReferenceSet("FacebookId", value);
        }

        public string AppleId
        {
            get => ReferencesGet("AppleId").FirstOrDefault()!;
            set => ReferenceSet("AppleId", value);
        }

        /// <summary>
        /// This is the social network provider.
        /// </summary>
        public virtual string? Provider
        {
            get;
            set;
        }

        /// <summary>
        /// Set if the user came from privilege or social network.
        /// </summary>
        public virtual bool IsNativeRegistration
        {
            get;
            set;
        }

        public int? MaximumOdsId
        {
            get
            {
                if (CustomerIds.Length == 0)
                    return null;

                var customerId = CustomerIds.Max(cid => int.TryParse(cid, out var custId) ? custId : 0);

                return customerId > 0 ? customerId : (int?)null;
            }
            set
            {
                // Left empty as should not be called. Only required for avro serialization
            }
        }

        /// <summary>
        /// This represents the ODS Customer ID that is marked as the non-retired duplicate - not necessarily to Maximum ODS Id!
        /// </summary>
        public int? ActiveOdsId
        {
            get
            {
                var activeOdsIdProperty = PropertyGet("ActiveOdsId");
                if (string.IsNullOrEmpty(activeOdsIdProperty))
                    return null;

                return int.TryParse(activeOdsIdProperty, out var activeOdsId) ? activeOdsId : (int?)null;
            }
            set => PropertySet("ActiveOdsId", value?.ToString()!);
        }

        public bool CustomerIdSet(string value)
        {
            if (string.IsNullOrEmpty(value))
                return false;

            var item = ReferencesGet("CustomerId").FirstOrDefault(k => k == value);
            if (item == null)
            {
                ReferenceSet("CustomerId", value, false);
                return true;
            }

            return false;
        }
        
        /// <summary>
        /// This is the source system.
        /// </summary>
        public string? SourceSystem
        {
            get;
            set;
        }

        /// <summary>
        /// This is a CustomerId Hash reference field.
        /// </summary>
        public string CustomerIdHash
        {
            get => PropertyGet("CustomerIdHash");
            set => PropertySet("CustomerIdHash", value);
        }

        /// <summary>
        /// This is a Customer's Lead Source.
        /// </summary>
        public string LeadSource
        {
            get => PropertyGet("LeadSource");
            set => PropertySet("LeadSource", value);
        }

        /// <summary>
        /// This is a salutation the user has set on their account.
        /// </summary>
        public virtual string? NameTitle
        {
            get;
            set;
        }
        /// <summary>
        /// This is the first or given name of the user.
        /// </summary>
        public virtual string? NameGiven
        {
            get;
            set;
        }
        /// <summary>
        /// This is the family name or surname of the user.
        /// </summary>
        public virtual string? NameFamily
        {
            get;
            set;
        }
        /// <summary>
        /// This is a profile image url of the user.
        /// </summary>
        public virtual string ProfileImage
        {
            get => PropertyGet("ProfileImage");
            set => PropertySet("ProfileImage", value);
        }
        /// <summary>
        /// This is a comment the user has set on their account.
        /// </summary>
        public virtual string Comment
        {
            get => PropertyGet("Comment");
            set => PropertySet("Comment", value);
        }

        /// <summary>
        /// This is a gender the user has set on their account.
        /// </summary>
        public virtual string? Gender
        {
            get;
            set;
        }
        /// <summary>
        /// This is a nationality the user has set on their account.
        /// </summary>
        public virtual string Nationality
        {
            get => PropertyGet("Nationality");
            set => PropertySet("Nationality", value);
        }
        /// <summary>
        /// This is a date of birth the user has set on their account.
        /// </summary>
        public virtual DateTime? DateOfBirth
        {
            get;
            set;
        }


        /// <summary>
        /// This is a Customer Card Id the user has set on their account.
        /// </summary>
        public virtual string CustomerCardId
        {
            get => PropertyGet("CustomerCardId");
            set => PropertySet("CustomerCardId", value);
        }

        /// <summary>
        /// This is a Primary Village the user has set on their account.
        /// </summary>
        public virtual string? VillagePrimary
        {
            get;
            set;
        }

        /// <summary>
        /// This is a Customer Language the user has set on their account.
        /// </summary>
        public virtual string? Language
        {
            get;
            set;
        }

        public virtual string? AddressLine1
        {
            get;
            set;
        }

        public virtual string? AddressLine2
        {
            get;
            set;
        }

        public virtual string? City
        {
            get;
            set;
        }

        public virtual string? County
        {
            get;
            set;
        }

        public virtual string? Country
        {
            get;
            set;
        }

        public virtual string? PostCode
        {
            get;
            set;
        }


        public virtual string PhoneNumber
        {
            get => PropertyGet("PhoneNumber");
            set => PropertySet("PhoneNumber", value);
        }

        public virtual string Province
        {
            get => PropertyGet("Province");
            set => PropertySet("Province", value);
        }

        public virtual bool? EmailDeliveryStatus
        {
            get => PropertyBoolGet("EmailDeliveryStatus");
            set => PropertyBoolSet("EmailDeliveryStatus", value);
        }


        public virtual bool? OptOutEmail
        {
            get => PropertyBoolGet("OptOutEmail");
            set => PropertyBoolSet("OptOutEmail", value);
        }

        public virtual bool? OptOutMobile
        {
            get => PropertyBoolGet("OptOutMobile");
            set => PropertyBoolSet("OptOutMobile", value);
        }

        public virtual bool? OptOutSms
        {
            get => PropertyBoolGet("OptOutSms");
            set => PropertyBoolSet("OptOutSms", value);
        }

        public virtual bool? OptOutPhone
        {
            get => PropertyBoolGet("OptOutPhone");
            set => PropertyBoolSet("OptOutPhone", value);
        }

        public virtual bool? OptOutPostal
        {
            get => PropertyBoolGet("OptOutPostal");
            set => PropertyBoolSet("OptOutPostal", value);
        }

        public virtual bool? OptOutGeo
        {
            get => PropertyBoolGet("OptOutGeo");
            set => PropertyBoolSet("OptOutGeo", value);
        }

        public virtual DateTime? GeoPermissionDateTime
        {
            get => PropertyDateTimeGet("GeoPermissionDateTime");
            set => PropertyDateTimeSet("GeoPermissionDateTime", value);
        }

        public virtual DateTime? EmailPermissionDateTime
        {
            get => PropertyDateTimeGet("EmailPermissionDateTime");
            set => PropertyDateTimeSet("EmailPermissionDateTime", value);
        }

        public virtual DateTime? PostalPermissionDateTime
        {
            get => PropertyDateTimeGet("PostalPermissionDateTime");
            set => PropertyDateTimeSet("PostalPermissionDateTime", value);
        }

        public virtual DateTime? MobilePermissionDateTime
        {
            get => PropertyDateTimeGet("MobilePermissionDateTime");
            set => PropertyDateTimeSet("MobilePermissionDateTime", value);
        }

        public virtual DateTime? SmsPermissionDateTime
        {
            get => PropertyDateTimeGet("SmsPermissionDateTime");
            set => PropertyDateTimeSet("SmsPermissionDateTime", value);
        }

        public virtual DateTime? PhonePermissionDateTime
        {
            get => PropertyDateTimeGet("PhonePermissionDateTime");
            set => PropertyDateTimeSet("PhonePermissionDateTime", value);
        }

        public virtual DateTime? TandCDateTime
        {
            get => PropertyDateTimeGet("TandCDateTime");
            set => PropertyDateTimeSet("TandCDateTime", value);
        }

        public virtual DateTime? ChangingMessageDisplayDateTime
        {
            get => PropertyDateTimeGet("ChangingMessageDisplayDateTime");
            set => PropertyDateTimeSet("ChangingMessageDisplayDateTime", value);
        }

        public virtual bool? IsGuest
        {
            get => PropertyBoolGet("IsGuest");
            set => PropertyBoolSet("IsGuest", value);
        }

        /// <summary>
        /// This method displays the appropriate name.
        /// </summary>
        public virtual string DisplayName => $"{NameGiven} {NameFamily}".Trim();

        public string BarcodeUrl
        {
            get => PropertyGet("BarcodeUrl");
            set => PropertySet("BarcodeUrl", value);
        }

        public string BarcodeOfferId
        {
            get => PropertyGet("BarcodeOfferId");
            set => PropertySet("BarcodeOfferId", value);
        }

        /// <summary>
        /// The source that created/updated this record last
        /// </summary>

        public string LastUpdateSource
        {
            get => PropertyGet(nameof(LastUpdateSource));
            set => PropertySet(nameof(LastUpdateSource), value);
        }

        public virtual DateTime? ProfileUpdated { get; set; }

        public virtual DateTime? OriginalRegistrationDate { get; set; }

        public virtual string? OriginalVillage { get; set; }

        public List<Tuple<string, string>> GetReferences()
        {
            return (from kvp in mCollections[CnReferences]
                    let index = kvp.Key.IndexOf('|', 0)
                    let key = index > -1 ? kvp.Key.Substring(0, index) : kvp.Key
                    select new Tuple<string, string>(key, kvp.Value)).ToList();
        }

        /// <summary>
        /// Whether the registration process goes with a single or two steps process
        /// </summary>
        public virtual bool SingleOptIn { get; set; }
        public virtual bool IsPrivateClient { get; set; }
        public virtual bool IsPersonalShopper { get; set; }
    }
}
