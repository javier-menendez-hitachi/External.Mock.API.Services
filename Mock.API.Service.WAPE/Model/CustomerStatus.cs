namespace Mock.API.Service.WAPE.Model
{
    
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    /// <summary>
    /// This class encapsulates the legacy customer.
    /// </summary>
    [DebuggerDisplay("CustomerId={CustomerId} IsMember={IsActive} IsApproved={IsDeleted} OptInStatus={OptInStatus}")]
    public class CustomerStatus : SecurityVirtualBase
    {
        public Guid CustomerId { get; set; }
        public Guid? MasterCustomerId { get; set; }
        public bool IsActive { get; set; }
        public bool IsRepermission { get; set; }
        public DateTime? DateMembership { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsClearedDelete { get; set; }
        public bool IsLockedOut { get; set; }
        public OptInStatusType? OptInStatus { get; set; }
        public DateTime? DateLastLogin { get; set; }

        public bool IsPrivilege
        {
            get => PropertyBoolGet("IsPrivilege") ?? false;
            set => PropertyBoolSet("IsPrivilege", value ? true : (bool?)null);
        }

        public DateTime? DatePrivilege
        {
            get => PropertyDateTimeGet(nameof(DatePrivilege));
            set => PropertyDateTimeSet(nameof(DatePrivilege), value);
        }

        public DateTime? DateTimePrivilege
        {
            get => PropertyDateTimeGet(nameof(DateTimePrivilege));
            set => PropertyDateTimeSet(nameof(DateTimePrivilege), value);
        }

        public string LastUpdateSource
        {
            get => PropertyGet(nameof(LastUpdateSource));
            set => PropertySet(nameof(LastUpdateSource), value);
        }
    }
}
