using LoanApplicant.Models;

namespace LoanApplicant.Services
{
    public interface ILoanServices
    {
        Task<LoanResult> CheckValidationAsync(Models.LoanApplicant loanApplicant); 
    }
}
