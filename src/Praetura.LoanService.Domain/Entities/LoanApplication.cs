using Praetura.LoanService.Domain.Enums;

namespace Praetura.LoanService.Domain.Entities;

public class LoanApplication
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public required string Email { get; set; }

    public decimal MonthlyIncome { get; set; }

    public decimal RequestedAmount { get; set; }

    public int TermMonths { get; set; }

    public LoanApplicationStatus Status { get; set; } = LoanApplicationStatus.Pending;

    public DateTime CreatedAt { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public ICollection<DecisionLogEntry> DecisionLog { get; set; } = new List<DecisionLogEntry>();
}
