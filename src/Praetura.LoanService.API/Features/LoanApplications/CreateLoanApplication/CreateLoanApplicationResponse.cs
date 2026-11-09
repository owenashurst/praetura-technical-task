namespace Praetura.LoanService.API.Features.LoanApplications.CreateLoanApplication;

public sealed record CreateLoanApplicationResponse(Guid Id, string Status, DateTime CreatedAt);
