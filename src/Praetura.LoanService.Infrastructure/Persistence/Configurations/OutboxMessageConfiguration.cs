using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Praetura.LoanService.Domain.Entities;

namespace Praetura.LoanService.Infrastructure.Persistence.Configurations;

public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Type)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(m => m.Payload)
            .IsRequired();

        builder.Property(m => m.CreatedAt)
            .IsRequired();

        // Only unpublished messages are ever queried, so index just those.
        builder.HasIndex(m => m.CreatedAt)
            .HasDatabaseName("IX_OutboxMessages_CreatedAt_Unpublished")
            .HasFilter("\"PublishedAt\" IS NULL");
    }
}
