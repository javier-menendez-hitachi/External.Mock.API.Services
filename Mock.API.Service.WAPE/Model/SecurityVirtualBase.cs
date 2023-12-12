namespace Mock.API.Service.WAPE.Model
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.Serialization;
    
    public abstract class SecurityVirtualBase : EntityBase<Guid>, IVersionable
    {
        protected Dictionary<string, Dictionary<string, string>> mCollections { get; set; }

        private const string cnProperties = "Properties";

        protected SecurityVirtualBase()
        {
            mCollections = new Dictionary<string, Dictionary<string, string>> { [cnProperties] = new Dictionary<string, string>() };
            Id = Guid.NewGuid();
            VersionId = Guid.NewGuid();
        }

        /// <summary>
        /// This is the version set by the datastore
        /// </summary>
        public Guid VersionId { get; set; }

        public virtual string? Signature { get; set; }

        public string PropertyGet(string key)
        {
            return CollectionGet(cnProperties, key);
        }

        public void PropertySet(string key, string value)
        {
            CollectionSet(cnProperties, key, value);
        }

        public bool? PropertyBoolGet(string key)
        {
            var stringValue = PropertyGet(key);
            if (string.IsNullOrEmpty(stringValue))
                return null;

            return "1".Equals(stringValue);
        }

        public void PropertyBoolSet(string key, bool? value)
        {
            if (!value.HasValue)
            {
                PropertySet(key, null!);
                return;
            }

            PropertySet(key, value.Value ? "1" : "0");
        }

        public DateTime? PropertyDateTimeGet(string key)
        {
            var stringValue = PropertyGet(key);
            if (string.IsNullOrEmpty(stringValue))
                return null;

            return DateTime.TryParse(stringValue, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out DateTime dateTimeValue) ? dateTimeValue : default(DateTime?);
        }

        public void PropertyDateTimeSet(string key, DateTime? value)
        {
            if (!value.HasValue)
            {
                PropertySet(key, null!);
                return;
            }

            PropertySet(key, value.Value.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss.fffffffZ"));
        }

        protected void CollectionSet(string coll, string key, string value)
        {
            key = key.ToUpper();
            if (value == null)
            {
                if (mCollections[coll].ContainsKey(key))
                    mCollections[coll].Remove(key);
                return;
            }
            mCollections[coll][key] = value;
        }

        protected string CollectionGet(string coll, string key)
        {
            key = key.ToUpper();

            if (!mCollections[coll].ContainsKey(key))
                return null!;

            return mCollections[coll][key];
        }

        public virtual void CollectionLoad(string coll, IEnumerable<KeyValuePair<string, string>> data)
        {
            var id = mCollections[coll];
            if (id == null)
                return;

            id.Clear();
            foreach (var dataItem in data)
            {
                id.Add(dataItem.Key, dataItem.Value);
            }
        }

        public virtual List<string> CollectionKeys()
        {
            return mCollections.Keys.ToList();
        }

        public virtual List<KeyValuePair<string, string>> Collection(string id)
        {
            if (mCollections.ContainsKey(id))
                return mCollections[id].ToList();

            throw new NotSupportedException();
        }
    }
}
