namespace LoanApplicant.Models
{
    public class LoanResult
    {
        public string Decision { get; set; }
        public ValidationResult[]? ValidationResults { get; set; }

        public LoanResult()
        {
            Decision = string.Empty;
            ValidationResults = new ValidationResult[] { };
        }
    }
}
