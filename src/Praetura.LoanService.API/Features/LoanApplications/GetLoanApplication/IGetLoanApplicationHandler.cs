namespace Praetura.LoanService.API.Features.LoanApplications.GetLoanApplication;

public interface IGetLoanApplicationHandler
{
    /// <summary>
    /// Gets a loan application by Id with its decision log, or null if no application was found.
    /// </summary>
    Task<LoanApplicationResponse?> HandleAsync(Guid id, CancellationToken cancellationToken);
}
