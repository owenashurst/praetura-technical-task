using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Praetura.LoanService.Domain.Entities;

namespace Praetura.LoanService.Infrastructure.Persistence.Configurations;

public class DecisionLogEntryConfiguration : IEntityTypeConfiguration<DecisionLogEntry>
{
    public void Configure(EntityTypeBuilder<DecisionLogEntry> builder)
    {
        builder.ToTable("DecisionLogEntries");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.RuleName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Message)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.EvaluatedAt)
            .IsRequired();
    }
}
