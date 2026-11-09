using Microsoft.AspNetCore.Mvc;
using Praetura.LoanService.API.Features.LoanApplications.CreateLoanApplication;
using Praetura.LoanService.API.Features.LoanApplications.GetLoanApplication;

namespace Praetura.LoanService.API.Features.LoanApplications;

[ApiController]
[Route("loan-applications")]
public class LoanApplicationsController(
    ICreateLoanApplicationHandler createHandler,
    IGetLoanApplicationHandler getHandler) : ControllerBase
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

        return CreatedAtAction(nameof(Get), new { id = response.Id }, response);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(LoanApplicationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var response = await getHandler.HandleAsync(id, cancellationToken);
        return response is null ? NotFound() : Ok(response);
    }
}
