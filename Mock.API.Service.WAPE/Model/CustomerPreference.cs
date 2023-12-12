namespace Mock.API.Service.WAPE.Model
{

    public class CustomerPreference : EntityBase<KeyCustomerPreference>
    {
        public CustomerPreference()
        {
        }

        public override KeyCustomerPreference Id
        {
            get => new(CustomerId, Group!, Key!);
            set { }
        }

        public Guid CustomerId { get; set; }

        public string? Group { get; set; }

        public string? Key { get; set; }

        public string? Value { get; set; }

        public CustomerPreference(Guid id, string group, string key, string value)
        {
            CustomerId = id;
            Group = group;
            Key = key;
            Value = value;
        }
    }
}
