namespace Mock.API.Service.WAPE.Model
{

    public class KeyMapperAttribute : Attribute
    {
        public KeyMapperAttribute(Type keyMapper)
        {
            if (keyMapper == null)
                throw new ArgumentNullException("keyMapper");

            if (!keyMapper.IsSubclassOf(typeof(KeyMapper)))
                throw new ArgumentOutOfRangeException("keyMapper", "Type should be a subclass of KeyMapper");

            KeyMapper = keyMapper;
        }

        public Type KeyMapper { get; private set; }
    }
}
