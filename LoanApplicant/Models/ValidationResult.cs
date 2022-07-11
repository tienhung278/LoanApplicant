namespace LoanApplicant.Models
{
    public class ValidationResult
    {
        public string Rule { get; set; }
        public string Message { get; set; }
        public string Decison { get; set; }

        public ValidationResult()
        {
            Rule = string.Empty;
            Message = string.Empty;
            Decison = string.Empty;
        }
    }
}
