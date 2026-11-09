using Microsoft.AspNetCore.Mvc;
using Praetura.LoanService.API.Features.LoanApplications.CreateLoanApplication;

namespace Praetura.LoanService.API.Features.LoanApplications;

[ApiController]
[Route("loan-applications")]
public class LoanApplicationsController(ICreateLoanApplicationHandler createHandler) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(CreateLoanApplicationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateLoanApplicationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await createHandler.HandleAsync(request, cancellationToken);
        if (!result.IsSuccess)
        {
            return ValidationProblem(new ValidationProblemDetails(result.ValidationErrors!));
        }

        var response = result.Response!;

        // TODO: switch to CreatedAtAction once the Get action exists.
        return Created($"/loan-applications/{response.Id}", response);
    }
}
