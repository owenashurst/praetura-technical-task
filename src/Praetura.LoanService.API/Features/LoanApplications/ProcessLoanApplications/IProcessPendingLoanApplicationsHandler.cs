namespace Praetura.LoanService.API.Features.LoanApplications.ProcessLoanApplications;

public interface IProcessPendingLoanApplicationsHandler
{
    /// <summary>
    /// Retrieves a batch of pending loan applications and records the decision for each.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns><see cref="int"/>The number of applications that were decided.</returns>
    Task<int> HandleAsync(CancellationToken cancellationToken);
}
