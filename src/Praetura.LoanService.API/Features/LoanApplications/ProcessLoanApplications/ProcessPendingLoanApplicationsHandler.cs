using Microsoft.Extensions.Options;
using Praetura.LoanService.Domain.Eligibility;
using Praetura.LoanService.Domain.Entities;
using Praetura.LoanService.Domain.Enums;
using Praetura.LoanService.Domain.Repositories;

namespace Praetura.LoanService.API.Features.LoanApplications.ProcessLoanApplications;

public class ProcessPendingLoanApplicationsHandler(
    ILoanApplicationRepository loanApplicationRepository,
    IEligibilityEvaluator evaluator,
    IOptions<ProcessingOptions> options,
    TimeProvider timeProvider,
    ILogger<ProcessPendingLoanApplicationsHandler> logger) : IProcessPendingLoanApplicationsHandler
{
    public async Task<int> HandleAsync(CancellationToken cancellationToken)
    {
        var pending = await loanApplicationRepository.GetPendingAsync(options.Value.BatchSize, cancellationToken);
        if (pending.Count == 0)
        {
            logger.LogDebug("No pending loan applications to process.");
            return 0;
        }

        logger.LogInformation("Processing {Count} pending loan application(s).", pending.Count);

        var decided = 0;
        foreach (var application in pending)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                await DecideAsync(application, cancellationToken);
                decided++;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(
                    ex,
                    "Failed to process loan application {LoanApplicationId}. It remains Pending.",
                    application.Id);
            }
        }

        return decided;
    }

    private async Task DecideAsync(LoanApplication application, CancellationToken cancellationToken)
    {
        var assessment = evaluator.Evaluate(application);
        var evaluatedAt = timeProvider.GetUtcNow().UtcDateTime;

        var decisionLog = assessment.Results
            .Select(result => new DecisionLogEntry
            {
                Id = Guid.NewGuid(),
                LoanApplicationId = application.Id,
                RuleName = result.RuleName,
                Passed = result.Passed,
                Message = result.Message,
                EvaluatedAt = evaluatedAt
            })
            .ToList();

        application.Status = assessment.IsEligible ? LoanApplicationStatus.Approved : LoanApplicationStatus.Rejected;
        application.ReviewedAt = evaluatedAt;

        var outboxMessage = LoanApplicationDecisionMessage.Create(application, evaluatedAt);

        await loanApplicationRepository.SaveDecisionAsync(application, decisionLog, outboxMessage, cancellationToken);

        logger.LogInformation(
            "Loan application {LoanApplicationId} was {Status}.",
            application.Id,
            application.Status);
    }
}
