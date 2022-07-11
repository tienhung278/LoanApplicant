namespace LoanApplicant.Models
{
    public class LoanApplicant
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public long BusinessNumber { get; set; }
        public long LoanAmount { get; set; }
        public string CitizenshipStatus { get; set; }
        public string TimeTrading { get; set; }
        public string CountryCode { get; set; }
        public string[] Industry { get; set; }

        public LoanApplicant()
        {
            FirstName = string.Empty;
            LastName = string.Empty;
            EmailAddress = string.Empty;
            PhoneNumber = string.Empty;
            CitizenshipStatus = string.Empty;
            TimeTrading = string.Empty;
            CountryCode = string.Empty;
            Industry = new string[] { };
        }        
    }
}
