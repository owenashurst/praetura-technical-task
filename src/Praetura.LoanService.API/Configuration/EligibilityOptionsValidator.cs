using Microsoft.Extensions.Options;
using Praetura.LoanService.Domain.Eligibility;

namespace Praetura.LoanService.API.Configuration;

/// <summary>
/// Fails application startup if the eligibility limits are nonsensical, instead of failing later in the background job.
/// </summary>
public class EligibilityOptionsValidator : IValidateOptions<EligibilityOptions>
{
    public ValidateOptionsResult Validate(string? name, EligibilityOptions options)
    {
        var failures = new List<string>();

        if (options.MinimumMonthlyIncome <= 0)
        {
            failures.Add($"{nameof(options.MinimumMonthlyIncome)} must be greater than 0.");
        }

        if (options.IncomeMultiple <= 0)
        {
            failures.Add($"{nameof(options.IncomeMultiple)} must be greater than 0.");
        }

        if (options.MinimumTermMonths < 1)
        {
            failures.Add($"{nameof(options.MinimumTermMonths)} must be at least 1.");
        }

        if (options.MinimumTermMonths > options.MaximumTermMonths)
        {
            failures.Add($"{nameof(options.MinimumTermMonths)} must not exceed {nameof(options.MaximumTermMonths)}.");
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }
}
