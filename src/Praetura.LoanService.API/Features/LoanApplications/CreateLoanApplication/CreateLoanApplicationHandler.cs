using FluentValidation;
using Praetura.LoanService.Domain.Entities;
using Praetura.LoanService.Domain.Enums;
using Praetura.LoanService.Domain.Repositories;

namespace Praetura.LoanService.API.Features.LoanApplications.CreateLoanApplication;

public class CreateLoanApplicationHandler(
    IValidator<CreateLoanApplicationRequest> validator,
    ILoanApplicationRepository repository,
    TimeProvider timeProvider) : ICreateLoanApplicationHandler
{
    public async Task<CreateLoanApplicationResult> HandleAsync(
        CreateLoanApplicationRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return CreateLoanApplicationResult.ValidationFailed(validation.ToDictionary());
        }
        
        var application = new LoanApplication
        {
            Id = Guid.NewGuid(),
            Name = request.Name!.Trim(),
            Email = request.Email!.Trim(),
            MonthlyIncome = request.MonthlyIncome!.Value,
            RequestedAmount = request.RequestedAmount!.Value,
            TermMonths = request.TermMonths!.Value,
            Status = LoanApplicationStatus.Pending,
            CreatedAt = timeProvider.GetUtcNow().UtcDateTime
        };

        await repository.AddAsync(application, cancellationToken);

        return CreateLoanApplicationResult.Success(
            new CreateLoanApplicationResponse(application.Id, application.Status.ToString(), application.CreatedAt));
    }
}
