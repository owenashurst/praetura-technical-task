namespace Praetura.LoanService.Domain.Eligibility;

public sealed record EligibilityAssessment(IReadOnlyList<EligibilityRuleResult> Results)
{
    public bool IsEligible => Results.All(r => r.Passed);
}
