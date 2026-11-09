namespace Praetura.LoanService.API.Features.LoanApplications.CreateLoanApplication;

public sealed class CreateLoanApplicationResult
{
    private CreateLoanApplicationResult(
        CreateLoanApplicationResponse? response,
        IDictionary<string, string[]>? validationErrors)
    {
        Response = response;
        ValidationErrors = validationErrors;
    }

    public CreateLoanApplicationResponse? Response { get; }

    public IDictionary<string, string[]>? ValidationErrors { get; }

    public bool IsSuccess => Response is not null;

    public static CreateLoanApplicationResult Success(CreateLoanApplicationResponse response) =>
        new(response, null);

    public static CreateLoanApplicationResult ValidationFailed(IDictionary<string, string[]> errors) =>
        new(null, errors);
}
