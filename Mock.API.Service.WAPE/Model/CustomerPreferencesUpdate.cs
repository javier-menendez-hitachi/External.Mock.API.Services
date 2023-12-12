namespace Mock.API.Service.WAPE.Model
{

    public class CustomerPreferencesUpdate
    {
        public IEnumerable<CustomerPreference>? OldPreferences { get; set; }
        public IEnumerable<CustomerPreference>? NewPreferences { get; set; }
    }
}
