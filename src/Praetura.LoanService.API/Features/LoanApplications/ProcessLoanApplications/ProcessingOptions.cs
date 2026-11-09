namespace Praetura.LoanService.API.Features.LoanApplications.ProcessLoanApplications;

/// <summary>
/// Settings for the background job that assesses pending loan applications.
/// </summary>
public class ProcessingOptions
{
    /// <summary>Seconds to wait between runs.</summary>
    public int IntervalSeconds { get; set; } = 60;

    /// <summary>Maximum number of pending applications processed in one run.</summary>
    public int BatchSize { get; set; } = 100;
}
