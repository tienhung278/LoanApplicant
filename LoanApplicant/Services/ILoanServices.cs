using LoanApplicant.Models;

namespace LoanApplicant.Services
{
    public interface ILoanServices
    {
        LoanResult CheckValidation(Models.LoanApplicant loanApplicant); 
    }
}
