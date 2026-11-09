using Praetura.LoanService.Domain.Entities;
using static System.FormattableString;

namespace Praetura.LoanService.Domain.Eligibility;

public class EligibilityEvaluator(EligibilityOptions eligibilityOptions) : IEligibilityEvaluator
{
    private const string MinimumIncomeRule = "MinimumIncome";
    private const string MaximumLoanAmountRule = "MaximumLoanAmount";
    private const string TermRangeRule = "TermRange";

    private readonly EligibilityOptions _eligibilityOptions =
        eligibilityOptions ?? throw new ArgumentNullException(nameof(eligibilityOptions));

    public EligibilityAssessment Evaluate(LoanApplication application)
    {
        ArgumentNullException.ThrowIfNull(application);

        var results = new[]
        {
            CheckMinimumIncome(application),
            CheckMaximumLoanAmount(application),
            CheckTermRange(application)
        };

        return new EligibilityAssessment(Array.AsReadOnly(results));
    }

    private EligibilityRuleResult CheckMinimumIncome(LoanApplication application)
    {
        var minimum = _eligibilityOptions.MinimumMonthlyIncome;
        var passed = application.MonthlyIncome >= minimum;

        return new EligibilityRuleResult(
            MinimumIncomeRule,
            passed,
            passed
                ? Invariant($"Monthly income £{application.MonthlyIncome:N2} meets the minimum of £{minimum:N2}.")
                : Invariant($"Monthly income £{application.MonthlyIncome:N2} is below the minimum of £{minimum:N2}."));
    }

    private EligibilityRuleResult CheckMaximumLoanAmount(LoanApplication application)
    {
        var multiple = _eligibilityOptions.IncomeMultiple;
        var maximumAmount = application.MonthlyIncome * multiple;
        var passed = application.RequestedAmount <= maximumAmount;

        return new EligibilityRuleResult(
            MaximumLoanAmountRule,
            passed,
            passed
                ? Invariant($"Requested amount £{application.RequestedAmount:N2} is within the maximum of £{maximumAmount:N2} ({multiple}x monthly income).")
                : Invariant($"Requested amount £{application.RequestedAmount:N2} exceeds the maximum of £{maximumAmount:N2} ({multiple}x monthly income)."));
    }

    private EligibilityRuleResult CheckTermRange(LoanApplication application)
    {
        var min = _eligibilityOptions.MinimumTermMonths;
        var max = _eligibilityOptions.MaximumTermMonths;
        var passed = application.TermMonths >= min && application.TermMonths <= max;

        return new EligibilityRuleResult(
            TermRangeRule,
            passed,
            passed
                ? Invariant($"Term of {application.TermMonths} months is within the allowed range of {min}-{max} months.")
                : Invariant($"Term of {application.TermMonths} months is outside the allowed range of {min}-{max} months."));
    }
}
