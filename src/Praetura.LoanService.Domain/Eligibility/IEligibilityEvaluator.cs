using Praetura.LoanService.Domain.Entities;

namespace Praetura.LoanService.Domain.Eligibility;

public interface IEligibilityEvaluator
{
    /// <summary>
    /// Evaluates a loan application against every eligibility rule and returns the outcome of each.
    /// </summary>
    /// <param name="application"><see cref="LoanApplication"/>The loan application to evaluate</param>
    /// <returns><see cref="EligibilityAssessment"/>The outcomes</returns>
    EligibilityAssessment Evaluate(LoanApplication application);
}
