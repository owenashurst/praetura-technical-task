using Praetura.LoanService.Domain.Eligibility;
using Praetura.LoanService.Domain.Entities;

namespace Praetura.LoanService.UnitTests.Eligibility;

public class EligibilityEvaluatorTests
{
    private const string MinimumIncomeRule = "MinimumIncome";
    private const string MaximumLoanAmountRule = "MaximumLoanAmount";
    private const string TermRangeRule = "TermRange";

    // monthlyIncome, requestedAmount, termMonths, expected failed rules in evaluation order
    public static TheoryData<decimal, decimal, int, string[]> EligibilityCases => new()
    {
        // Valid application
        { 3_500m, 8_000m, 36, [] },

        // Minimum income boundary
        { 2_000m, 1_000m, 24, [] },
        { 1_999.99m, 1_000m, 24, [MinimumIncomeRule] },

        // Maximum amount boundary: 4 x 3,000 = 12,000
        { 3_000m, 12_000m, 24, [] },
        { 3_000m, 12_000.01m, 24, [MaximumLoanAmountRule] },

        // Term boundaries
        { 3_000m, 5_000m, 12, [] },
        { 3_000m, 5_000m, 60, [] },
        { 3_000m, 5_000m, 11, [TermRangeRule] },
        { 3_000m, 5_000m, 61, [TermRangeRule] },

        // Every rule fails
        { 1_000m, 5_000m, 6, [MinimumIncomeRule, MaximumLoanAmountRule, TermRangeRule] }
    };

    [Theory]
    [MemberData(nameof(EligibilityCases))]
    public void GIVEN_Evaluate_WHEN_ApplicationIsAssessed_THEN_OnlyTheExpectedRulesFail(
        decimal monthlyIncome,
        decimal requestedAmount,
        int termMonths,
        string[] expectedFailedRules)
    {
        var evaluator = new EligibilityEvaluator(new EligibilityOptions());
        var application = CreateApplication(monthlyIncome, requestedAmount, termMonths);

        var assessment = evaluator.Evaluate(application);

        var failedRules = assessment.Results.Where(r => !r.Passed).Select(r => r.RuleName);
        
        Assert.Equal(expectedFailedRules, failedRules);
        Assert.Equal(expectedFailedRules.Length == 0, assessment.IsEligible);
        Assert.Equal(3, assessment.Results.Count);
    }

    private static LoanApplication CreateApplication(decimal monthlyIncome, decimal requestedAmount, int termMonths) =>
        new()
        {
            Id = Guid.NewGuid(),
            Name = "Alice Example",
            Email = "alice@example.com",
            MonthlyIncome = monthlyIncome,
            RequestedAmount = requestedAmount,
            TermMonths = termMonths,
            CreatedAt = new DateTime(2025, 5, 27, 10, 30, 0, DateTimeKind.Utc)
        };
}
