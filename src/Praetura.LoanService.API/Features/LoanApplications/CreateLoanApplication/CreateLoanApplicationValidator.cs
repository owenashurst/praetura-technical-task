using FluentValidation;

namespace Praetura.LoanService.API.Features.LoanApplications.CreateLoanApplication;

public class CreateLoanApplicationValidator : AbstractValidator<CreateLoanApplicationRequest>
{
    // Limits match the database columns.
    private const int MaxNameLength = 200;
    private const int MaxEmailLength = 320;

    public CreateLoanApplicationValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(MaxNameLength);

        RuleFor(x => x.Email)
            .NotEmpty()
            .MaximumLength(MaxEmailLength)
            .EmailAddress();

        RuleFor(x => x.MonthlyIncome)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .GreaterThan(0)
            .PrecisionScale(18, 2, ignoreTrailingZeros: true);

        RuleFor(x => x.RequestedAmount)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .GreaterThan(0)
            .PrecisionScale(18, 2, ignoreTrailingZeros: true);

        RuleFor(x => x.TermMonths)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .GreaterThan(0);
    }
}
