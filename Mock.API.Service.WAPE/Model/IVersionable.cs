namespace Mock.API.Service.WAPE.Model
{
    using System;
    public interface IVersionable
    {
        Guid VersionId { get; set; }
    }
}
