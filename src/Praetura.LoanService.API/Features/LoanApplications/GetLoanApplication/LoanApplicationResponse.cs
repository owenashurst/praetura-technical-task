namespace Praetura.LoanService.API.Features.LoanApplications.GetLoanApplication;

public sealed record LoanApplicationResponse(
    Guid Id,
    string Name,
    string Email,
    decimal MonthlyIncome,
    decimal RequestedAmount,
    int TermMonths,
    string Status,
    DateTime CreatedAt,
    DateTime? ReviewedAt,
    IReadOnlyList<DecisionLogEntryResponse> DecisionLog);

public sealed record DecisionLogEntryResponse(
    string RuleName,
    bool Passed,
    string Message,
    DateTime EvaluatedAt);
