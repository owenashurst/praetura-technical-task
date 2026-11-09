namespace Praetura.LoanService.Domain.Eligibility;

public class EligibilityOptions
{
    public decimal MinimumMonthlyIncome { get; set; } = 2_000m;

    public decimal IncomeMultiple { get; set; } = 4m;

    public int MinimumTermMonths { get; set; } = 12;

    public int MaximumTermMonths { get; set; } = 60;
}
