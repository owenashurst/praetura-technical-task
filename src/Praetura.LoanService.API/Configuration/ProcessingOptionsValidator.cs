using Microsoft.Extensions.Options;
using Praetura.LoanService.API.Features.LoanApplications.ProcessLoanApplications;

namespace Praetura.LoanService.API.Configuration;

public class ProcessingOptionsValidator : IValidateOptions<ProcessingOptions>
{
    public ValidateOptionsResult Validate(string? name, ProcessingOptions options)
    {
        var failures = new List<string>();

        if (options.IntervalSeconds < 1)
        {
            failures.Add($"{nameof(options.IntervalSeconds)} must be at least 1.");
        }

        if (options.BatchSize < 1)
        {
            failures.Add($"{nameof(options.BatchSize)} must be at least 1.");
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }
}
