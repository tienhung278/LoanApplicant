using LoanApplicant.Models;
using LoanApplicant.Services;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LoanApplicant.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoansController : ControllerBase
    {
        private readonly ILoanServices loanServices;

        public LoansController(ILoanServices loanServices)
        {
            this.loanServices = loanServices;
        }

        [HttpPost]
        public IActionResult Post(Models.LoanApplicant value)
        {
            var result = loanServices.CheckValidation(value);

            return Ok(result);
        }
    }
}
