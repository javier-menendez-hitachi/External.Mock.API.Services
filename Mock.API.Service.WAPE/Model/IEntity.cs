namespace Mock.API.Service.WAPE.Model
{
    using System;

    public interface IEntity<K> where K : IEquatable<K>
    {
        K Id { get; set; }
        DateTime DateCreated { get; set; }
        DateTime? DateUpdated { get; set; }
    }
}
