namespace Praetura.LoanService.API.Features.LoanApplications.CreateLoanApplication;

public interface ICreateLoanApplicationHandler
{
    /// <summary>
    /// Validates the request, and if valid, stores a new loan application with status Pending.
    /// </summary>
    Task<CreateLoanApplicationResult> HandleAsync(
        CreateLoanApplicationRequest request,
        CancellationToken cancellationToken);
}
