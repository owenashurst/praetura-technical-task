using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Praetura.LoanService.API.Features.LoanApplications.ProcessLoanApplications;
using Praetura.LoanService.Domain.Eligibility;
using Praetura.LoanService.Domain.Entities;
using Praetura.LoanService.Domain.Enums;
using Praetura.LoanService.Domain.Repositories;

namespace Praetura.LoanService.UnitTests.ProcessLoanApplications;

public class ProcessPendingLoanApplicationsHandlerTests
{
    private static readonly DateTimeOffset Now = new(2025, 5, 27, 12, 0, 0, TimeSpan.Zero);

    private readonly ILoanApplicationRepository _repository = Substitute.For<ILoanApplicationRepository>();
    private readonly IEligibilityEvaluator _evaluator = new EligibilityEvaluator(new EligibilityOptions());

    [Fact]
    public async Task GIVEN_HandleAsync_WHEN_ApplicationIsEligible_THEN_ItIsApprovedLoggedAndAnnounced()
    {
        var application = CreateApplication(monthlyIncome: 3_500m, requestedAmount: 8_000m, termMonths: 36);
        SetPending(application);

        var handler = CreateHandler(_evaluator);
        var decided = await handler.HandleAsync(CancellationToken.None);

        Assert.Equal(1, decided);
        Assert.Equal(LoanApplicationStatus.Approved, application.Status);
        Assert.Equal(Now.UtcDateTime, application.ReviewedAt);
        await _repository.Received(1).SaveDecisionAsync(
            application,
            Arg.Is<IReadOnlyCollection<DecisionLogEntry>>(log =>
                log.Count == 3 &&
                log.All(e => e.Passed && e.LoanApplicationId == application.Id && e.EvaluatedAt == Now.UtcDateTime)),
            Arg.Is<OutboxMessage>(message =>
                message.Type == LoanApplicationDecisionMessage.ApprovedType &&
                message.CreatedAt == Now.UtcDateTime &&
                message.PublishedAt == null &&
                message.Payload.Contains(application.Id.ToString())),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GIVEN_HandleAsync_WHEN_ApplicationIsNotEligible_THEN_ItIsRejectedLoggedAndAnnounced()
    {
        var application = CreateApplication(monthlyIncome: 1_000m, requestedAmount: 5_000m, termMonths: 6);
        SetPending(application);

        var handler = CreateHandler(_evaluator);
        await handler.HandleAsync(CancellationToken.None);

        Assert.Equal(LoanApplicationStatus.Rejected, application.Status);
        Assert.Equal(Now.UtcDateTime, application.ReviewedAt);
        await _repository.Received(1).SaveDecisionAsync(
            application,
            Arg.Is<IReadOnlyCollection<DecisionLogEntry>>(log => log.Count == 3 && log.All(e => !e.Passed)),
            Arg.Is<OutboxMessage>(message =>
                message.Type == LoanApplicationDecisionMessage.RejectedType &&
                message.Payload.Contains(application.Id.ToString())),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GIVEN_HandleAsync_WHEN_EvaluationFailsForOneApplication_THEN_TheRestOfTheBatchIsStillDecided()
    {
        var failing = CreateApplication(monthlyIncome: 3_500m, requestedAmount: 8_000m, termMonths: 36);
        var healthy = CreateApplication(monthlyIncome: 3_500m, requestedAmount: 8_000m, termMonths: 36);
        SetPending(failing, healthy);

        var evaluator = Substitute.For<IEligibilityEvaluator>();
        evaluator.Evaluate(Arg.Any<LoanApplication>()).Returns(call =>
        {
            var application = call.Arg<LoanApplication>();
            return application.Id == failing.Id
                ? throw new InvalidOperationException("Evaluation failed.")
                : _evaluator.Evaluate(application);
        });
        var handler = CreateHandler(evaluator);

        var decided = await handler.HandleAsync(CancellationToken.None);

        Assert.Equal(1, decided);
        await _repository.DidNotReceive().SaveDecisionAsync(
            failing,
            Arg.Any<IReadOnlyCollection<DecisionLogEntry>>(),
            Arg.Any<OutboxMessage>(),
            Arg.Any<CancellationToken>());
        await _repository.Received(1).SaveDecisionAsync(
            healthy,
            Arg.Any<IReadOnlyCollection<DecisionLogEntry>>(),
            Arg.Any<OutboxMessage>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GIVEN_HandleAsync_WHEN_SavingOneApplicationFails_THEN_TheRestOfTheBatchIsStillDecided()
    {
        var failing = CreateApplication(monthlyIncome: 3_500m, requestedAmount: 8_000m, termMonths: 36);
        var healthy = CreateApplication(monthlyIncome: 3_500m, requestedAmount: 8_000m, termMonths: 36);
        SetPending(failing, healthy);

        _repository
            .SaveDecisionAsync(
                failing,
                Arg.Any<IReadOnlyCollection<DecisionLogEntry>>(),
                Arg.Any<OutboxMessage>(),
                Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Save failed."));

        var handler = CreateHandler(_evaluator);
        var decided = await handler.HandleAsync(CancellationToken.None);

        Assert.Equal(1, decided);
        await _repository.Received(1).SaveDecisionAsync(
            healthy,
            Arg.Any<IReadOnlyCollection<DecisionLogEntry>>(),
            Arg.Any<OutboxMessage>(),
            Arg.Any<CancellationToken>());
    }

    private ProcessPendingLoanApplicationsHandler CreateHandler(IEligibilityEvaluator evaluator) =>
        new(
            _repository,
            evaluator,
            Options.Create(new ProcessingOptions { BatchSize = 10 }),
            new FixedTimeProvider(Now),
            NullLogger<ProcessPendingLoanApplicationsHandler>.Instance);

    private void SetPending(params LoanApplication[] applications) =>
        _repository
            .GetPendingAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<LoanApplication>>(applications));

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

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
