namespace Mock.API.Service.WAPE.Model
{

    public class KeyCustomerPreference : IEquatable<KeyCustomerPreference>
    {
        private static readonly KeyCustomerPreferenceMapper _keyMapper = new();

        public KeyCustomerPreference() { }

        public KeyCustomerPreference(Guid customerId, string group, string key)
        {
            CustomerId = customerId;
            Group = group;
            Key = key;
        }

        /// <summary>
        /// The customer External ID
        /// </summary>
        public Guid CustomerId;

        /// <summary>
        /// The preference Group
        /// </summary>
        public string? Group;

        /// <summary>
        /// The preference Key
        /// </summary>
        public string? Key;


        public bool Equals(KeyCustomerPreference? other)
        {
            return CustomerId == other?.CustomerId && Group == other.Group && Key == other.Key;
        }

        public override string ToString()
        {
            return _keyMapper.ToString(this);
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as KeyCustomerPreference);
        }

        public override int GetHashCode()
        {
            return _keyMapper.GetHashCode();
        }
    }

    public class KeyCustomerPreferenceMapper : KeyMapper<KeyCustomerPreference>
    {
        public KeyCustomerPreferenceMapper()
            : base(DeSerialize, Serialize)
        {
        }

        private static string Serialize(KeyCustomerPreference key)
        {
            return $"{key.CustomerId}|{key.Group}|{key.Key}";
        }

        private static KeyCustomerPreference DeSerialize(string key)
        {
            var parts = key.Split('|');

            var returnKey = new KeyCustomerPreference { CustomerId = new Guid(parts[0]), Group = parts[1], Key = parts[2] };
            return returnKey;
        }
    }
}
