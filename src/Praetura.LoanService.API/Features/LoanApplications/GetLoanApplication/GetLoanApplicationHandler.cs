using Praetura.LoanService.Domain.Repositories;

namespace Praetura.LoanService.API.Features.LoanApplications.GetLoanApplication;

public class GetLoanApplicationHandler(ILoanApplicationRepository repository) : IGetLoanApplicationHandler
{
    public async Task<LoanApplicationResponse?> HandleAsync(Guid id, CancellationToken cancellationToken)
    {
        var application = await repository.GetByIdAsync(id, cancellationToken);
        if (application is null)
        {
            return null;
        }

        var decisionLog = application.DecisionLog
            .OrderBy(e => e.EvaluatedAt)
            .Select(e => new DecisionLogEntryResponse(e.RuleName, e.Passed, e.Message, e.EvaluatedAt))
            .ToList();

        return new LoanApplicationResponse(
            application.Id,
            application.Name,
            application.Email,
            application.MonthlyIncome,
            application.RequestedAmount,
            application.TermMonths,
            application.Status.ToString(),
            application.CreatedAt,
            application.ReviewedAt,
            decisionLog);
    }
}
