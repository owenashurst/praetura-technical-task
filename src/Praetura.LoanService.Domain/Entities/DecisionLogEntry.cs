namespace Praetura.LoanService.Domain.Entities;

public class DecisionLogEntry
{
    public Guid Id { get; set; }

    public Guid LoanApplicationId { get; set; }

    public required string RuleName { get; set; }

    public bool Passed { get; set; }

    public required string Message { get; set; }

    public DateTime EvaluatedAt { get; set; }

    public LoanApplication? LoanApplication { get; set; }
}
