using LoanApplicant.Models;
using Microsoft.Extensions.Caching.Memory;
using System.Text.RegularExpressions;

namespace LoanApplicant.Services.Concretes
{
    public class LoanServices : ILoanServices
    {
        private const string DECISION_QUALIFIED = "Qualified";
        private const string DECISION_UNQUALIFIED = "Unqualified";
        private const string DECISION_UNKNOWN = "Unknown";
        private const string CITIZENSHIP_CITIZEN = "Citizen";
        private const string CITIZENSHIP_PERMANENT_RESIDENT = "Permanent Resident";

        private readonly IConfiguration configuration;
        private readonly IMemoryCache memoryCache;
        private readonly long loanAmountMax;
        private readonly long loanAmountMin;
        private readonly int timeTradingMax;
        private readonly int timeTradingMin;
        private static readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);

        public LoanServices(IConfiguration configuration, IMemoryCache memoryCache)
        {
            this.configuration = configuration;
            this.memoryCache = memoryCache;
            this.loanAmountMax = this.configuration.GetValue<long>("LoanAmount:Max");
            this.loanAmountMin = this.configuration.GetValue<long>("LoanAmount:Min");
            this.timeTradingMax = this.configuration.GetValue<int>("TimeTrading:Max");
            this.timeTradingMin = this.configuration.GetValue<int>("TimeTrading:Mix");
        }

        public async Task<LoanResult> CheckValidationAsync(Models.LoanApplicant loanApplicant)
        {
            var validationResults = new List<ValidationResult>();

            if (string.IsNullOrEmpty(loanApplicant.FirstName))
            {
                validationResults.Add(new ValidationResult
                {
                    Rule = "firstName",
                    Message = "First Name is reqired",
                    Decison = DECISION_UNQUALIFIED
                });
            }
            else if (string.IsNullOrEmpty(loanApplicant.LastName))
            {
                validationResults.Add(new ValidationResult
                {
                    Rule = "lastName",
                    Message = "Last Name is reqired",
                    Decison = DECISION_UNQUALIFIED
                });
            }

            if (string.IsNullOrEmpty(loanApplicant.EmailAddress))
            {
                validationResults.Add(new ValidationResult
                {
                    Rule = "emailAddress",
                    Message = "Email Address is reqired",
                    Decison = DECISION_UNQUALIFIED
                });
            }
            else if (string.IsNullOrEmpty(loanApplicant.PhoneNumber))
            {
                validationResults.Add(new ValidationResult
                {
                    Rule = "phoneNumber",
                    Message = "Phone Number is reqired",
                    Decison = DECISION_UNQUALIFIED
                });
            }
            
            if (!string.IsNullOrEmpty(loanApplicant.PhoneNumber))
            {
                var regex = @"^(\+?\(61\)|\(\+?61\)|\+?61|\(0[1-9]\)|0[1-9])?( ?-?[0-9]){7,9}$";
                var match = Regex.Match(loanApplicant.PhoneNumber, regex);
                if (!match.Success)
                {
                    validationResults.Add(new ValidationResult
                    {
                        Rule = "phoneNumber",
                        Message = "Phone Number is invalid",
                        Decison = DECISION_UNQUALIFIED
                    });
                }
            }

            if (loanApplicant.BusinessNumber.ToString().Length != 11)
            {
                validationResults.Add(new ValidationResult
                {
                    Rule = "businessNumber",
                    Message = "Business Number is invalid",
                    Decison = DECISION_UNQUALIFIED
                });
            }
            else
            {
                bool isValid = await CheckBusinessNumberValidationAsync(loanApplicant.BusinessNumber);
                if (!isValid)
                {
                    validationResults.Add(new ValidationResult
                    {
                        Rule = "businessNumber",
                        Message = "Business Number is invalid",
                        Decison = DECISION_UNQUALIFIED
                    });
                }
            }

            if (loanApplicant.LoanAmount <= loanAmountMin || loanApplicant.LoanAmount >= loanAmountMax)
            {
                validationResults.Add(new ValidationResult
                {
                    Rule = "loanAmount",
                    Message = "Loan Amount is invalid",
                    Decison = DECISION_UNQUALIFIED
                });
            }

            if (!loanApplicant.CitizenshipStatus.Contains(CITIZENSHIP_CITIZEN, StringComparison.InvariantCultureIgnoreCase) &&
                !loanApplicant.CitizenshipStatus.Contains(CITIZENSHIP_PERMANENT_RESIDENT, StringComparison.InvariantCultureIgnoreCase))
            {
                validationResults.Add(new ValidationResult
                {
                    Rule = "citizenshipStatus",
                    Message = "Citizenship Status is unknown",
                    Decison = DECISION_UNKNOWN
                });
            }

            if (int.TryParse(loanApplicant.TimeTrading, out int result))
            {
                if (result < timeTradingMin || result > timeTradingMax)
                {
                    validationResults.Add(new ValidationResult
                    {
                        Rule = "timeTrading",
                        Message = "Time Trading is invalid",
                        Decison = DECISION_UNQUALIFIED
                    });
                }
            }
            else
            {
                validationResults.Add(new ValidationResult
                {
                    Rule = "timeTrading",
                    Message = "Time Trading is unknown",
                    Decison = DECISION_UNKNOWN
                });
            }

            if (!loanApplicant.CountryCode.Contains("AU", StringComparison.InvariantCultureIgnoreCase))
            {
                validationResults.Add(new ValidationResult
                {
                    Rule = "countryCode",
                    Message = "Country Code is unknown",
                    Decison = DECISION_UNKNOWN
                });
            }

            if (loanApplicant.Industry.Any(v => !v.Contains("Allowed Industry", StringComparison.InvariantCultureIgnoreCase)))
            {
                if (loanApplicant.Industry.Any(v => v.Contains("Banned Industry", StringComparison.InvariantCultureIgnoreCase)))
                {
                    validationResults.Add(new ValidationResult
                    {
                        Rule = "industry",
                        Message = "Industry is unqualified",
                        Decison = DECISION_UNQUALIFIED
                    });
                }
                else
                {
                    validationResults.Add(new ValidationResult
                    {
                        Rule = "industry",
                        Message = "Industry is unknown",
                        Decison = DECISION_UNKNOWN
                    });
                }                
            }

            var loanResult = new LoanResult();

            if (validationResults.Count == 0)
            {
                loanResult.Decision = DECISION_QUALIFIED;
                loanResult.ValidationResults = null;
            }
            else if (validationResults.Any(r => r.Decison == DECISION_UNQUALIFIED))
            {
                loanResult.Decision = DECISION_UNQUALIFIED;
                loanResult.ValidationResults = validationResults.Where(r => r.Decison == DECISION_UNQUALIFIED).ToArray();
            }
            else if (validationResults.Any(r => r.Decison == DECISION_UNKNOWN))
            {
                loanResult.Decision = DECISION_UNKNOWN;
                loanResult.ValidationResults = validationResults.Where(r => r.Decison == DECISION_UNKNOWN).ToArray();
            }

            return loanResult;
        }

        private async Task<bool> CheckBusinessNumberValidationAsync(long businessNumber)
        {
            bool isAvailable = memoryCache.TryGetValue(businessNumber, out bool isValid);
            if (isAvailable)
            {
                return isValid;
            }

            try
            {
                await semaphore.WaitAsync();
                isAvailable = memoryCache.TryGetValue(businessNumber, out isValid);
                if (isAvailable)
                {
                    return isValid;
                }
                Thread.Sleep(3000);
                isValid = true;
                memoryCache.Set(businessNumber, isValid);
            }
            catch
            {
                throw;
            }
            finally
            {
                semaphore.Release();
            }

            return isValid;
        }
    }
}
