namespace External.Mock.API.Service.ZeroBounce.Model
{
    public class ValidateEmailResult
    {
        public string? Address { get; set; }

        public string? Status { get; set; }

        public string? Sub_Status { get; set; }

        public bool Free_Email { get; set; }

        public string? Did_You_Mean { get; set; }

        public string? Account { get; set; }

        public string? Domain { get; set; }

        public string? Domain_Age_Days { get; set; }

        public string? Smtp_Provider { get; set; }

        public string? Mx_Record { get; set; }

        public string? Mx_Found { get; set; }

        public string? Firstname { get; set; }

        public string? Lastname { get; set; }

        public string? Gender { get; set; }

        public string? Country { get; set; }

        public string? Region { get; set; }

        public string? City { get; set; }

        public string? Zipcode { get; set; }

        public string? Processed_At { get; set; }
    }
}