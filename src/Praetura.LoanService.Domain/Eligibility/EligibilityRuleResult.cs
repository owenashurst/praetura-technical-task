namespace Praetura.LoanService.Domain.Eligibility;

public sealed record EligibilityRuleResult(string RuleName, bool Passed, string Message);
