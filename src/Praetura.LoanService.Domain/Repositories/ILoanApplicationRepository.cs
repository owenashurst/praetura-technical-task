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
}
