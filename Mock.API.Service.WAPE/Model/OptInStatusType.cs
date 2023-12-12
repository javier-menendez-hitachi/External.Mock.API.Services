namespace Mock.API.Service.WAPE.Model
{
    public enum OptInStatusType
    {
        /// <summary>
        /// Opted out.
        /// </summary>
        OptOut = 0,

        /// <summary>
        /// Single Opt-In, used before GDPR came into force and now redundant.
        /// </summary>
        Single = 1,

        /// <summary>
        /// Double Opt-In, used once GDPR came into force.
        /// </summary>
        Double = 2,

        /// <summary>
        /// Awaiting activation, indicates that the customer has not completed the Privilege Sign-Up process.
        /// </summary>
        Pending = 3
    }
}
