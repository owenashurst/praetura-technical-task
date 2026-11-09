namespace Praetura.LoanService.Domain.Entities;

public class OutboxMessage
{
    public Guid Id { get; set; }

    // The kind of message, for example "LoanApplicationApproved"
    public required string Type { get; set; }

    // The message body as JSON
    public required string Payload { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? PublishedAt { get; set; }
}
