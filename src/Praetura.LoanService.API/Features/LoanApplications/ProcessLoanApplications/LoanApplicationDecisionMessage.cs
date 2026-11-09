using System.Text.Json;
using Praetura.LoanService.Domain.Entities;
using Praetura.LoanService.Domain.Enums;

namespace Praetura.LoanService.API.Features.LoanApplications.ProcessLoanApplications;

public static class LoanApplicationDecisionMessage
{
    public const string ApprovedType = "LoanApplicationApproved";
    public const string RejectedType = "LoanApplicationRejected";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static OutboxMessage Create(LoanApplication decidedApplication, DateTime createdAt)
    {
        var type = decidedApplication.Status == LoanApplicationStatus.Approved ? ApprovedType : RejectedType;

        var payload = JsonSerializer.Serialize(
            new DecisionPayload(
                decidedApplication.Id,
                decidedApplication.Status.ToString(),
                decidedApplication.ReviewedAt!.Value),
            JsonOptions);

        return new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = type,
            Payload = payload,
            CreatedAt = createdAt
        };
    }

    private sealed record DecisionPayload(Guid LoanApplicationId, string Decision, DateTime ReviewedAt);
}
