namespace Mock.API.Service.WAPE.Model
{
    public interface IKeyMapper<K>
    {
        K ToKey(string value);
        string ToString(K key);
    }

    public class KeyMapper
    {
        static readonly Dictionary<Type, KeyMapper> sMaps;

        static KeyMapper()
        {
            sMaps = new Dictionary<Type, KeyMapper>
            {
                {typeof(string), new KeyMapper<string>(s => s, s => s)},
                {typeof(Guid), new KeyMapper<Guid>(s => new Guid(s), s => s.ToString("N"))},
                {typeof(int), new KeyMapper<int>(int.Parse, s => s.ToString())},
                {typeof(long), new KeyMapper<long>(long.Parse, s => s.ToString())},
                {typeof(double), new KeyMapper<double>(double.Parse, s => s.ToString())}
            };

        }

        public static KeyMapper<KT> Add<KT>(KeyMapper<KT> mapper)
        {
            sMaps[typeof(KT)] = mapper;
            return mapper;
        }

        public static KeyMapper<KT>? Resolve<KT>()
        {
            var type = typeof(KT);

            if (sMaps.TryGetValue(type, out KeyMapper? value))
                return value as KeyMapper<KT>;

            //Ok, do attribute lookup

            var attr = type.GetCustomAttributes(false)
                .OfType<KeyMapperAttribute>().FirstOrDefault();

            if (attr != null)
            {
                KeyMapper<KT>? mapper = Activator.CreateInstance(attr.KeyMapper) as KeyMapper<KT>;
                return Add(mapper!);
            }

            throw new Exception(type.Name);
        }
    }

    public class KeyMapper<K>(Func<string, K> deserialize, Func<K, string> serialize) : KeyMapper, IKeyMapper<K>
    {
        protected readonly Func<string, K> mDeserialize = deserialize;
        protected readonly Func<K, string> mSerialize = serialize;

        public K ToKey(string value)
        {
            return mDeserialize(value);
        }

        public string ToString(K value)
        {
            return mSerialize(value);
        }
    }
}
