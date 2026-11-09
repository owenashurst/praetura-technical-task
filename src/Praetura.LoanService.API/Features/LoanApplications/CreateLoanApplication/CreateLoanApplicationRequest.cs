namespace Praetura.LoanService.API.Features.LoanApplications.CreateLoanApplication;

/// <summary>
/// Properties are nullable so that a missing value produces a validation error
/// instead of silently binding to a default such as 0.
/// </summary>
public sealed record CreateLoanApplicationRequest(
    string? Name,
    string? Email,
    decimal? MonthlyIncome,
    decimal? RequestedAmount,
    int? TermMonths);
