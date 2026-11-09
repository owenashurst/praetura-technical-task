using Praetura.LoanService.Domain.Entities;

namespace Praetura.LoanService.Domain.Repositories;

public interface ILoanApplicationRepository
{
    /// <summary>
    /// Stores a new loan application.
    /// </summary>
    /// <param name="loanApplication"><see cref="LoanApplication"/>The loan application to save</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task AddAsync(LoanApplication loanApplication, CancellationToken cancellationToken);

    /// <summary>
    /// Gets a loan application with its decision log entries, or null if it does not exist.
    /// </summary>
    /// <param name="id"><see cref="Guid"/>The Id of the loan to retrieve</param>
    /// <param name="cancellationToken"></param>
    /// <returns><see cref="LoanApplication"/> A loan application if it exists, otherwise null</returns>
    Task<LoanApplication?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves a batch of pending loan applications, up to the specified batch size.
    /// </summary>
    /// <param name="batchSize"><see cref="int"/>The batch size to retrieve</param>
    /// <param name="cancellationToken"></param>
    /// <returns><see cref="IReadOnlyList{T}"/>A list of pending loan applications</returns>
    Task<IReadOnlyList<LoanApplication>> GetPendingAsync(int batchSize, CancellationToken cancellationToken);

    /// <summary>
    /// Saves the decision for a reviewed application, its new status, its decision log entries and the outbox
    /// message announcing the decision.
    /// </summary>
    /// <param name="loanApplication"><see cref="LoanApplication"/>The loan application to save</param>
    /// <param name="decisionLog"><see cref="IReadOnlyCollection{T}"/>A list of decision logs for the loan application</param>
    /// <param name="outboxMessage"><see cref="OutboxMessage"/>The message to publish about the decision</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task SaveDecisionAsync(
        LoanApplication loanApplication,
        IReadOnlyCollection<DecisionLogEntry> decisionLog,
        OutboxMessage outboxMessage,
        CancellationToken cancellationToken);
}
