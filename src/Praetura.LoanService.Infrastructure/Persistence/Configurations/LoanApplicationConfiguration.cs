using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Praetura.LoanService.Domain.Entities;

namespace Praetura.LoanService.Infrastructure.Persistence.Configurations;

public class LoanApplicationConfiguration : IEntityTypeConfiguration<LoanApplication>
{
    public void Configure(EntityTypeBuilder<LoanApplication> builder)
    {
        builder.ToTable("LoanApplications");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.Email)
            .IsRequired()
            .HasMaxLength(320);

        builder.Property(a => a.MonthlyIncome)
            .HasPrecision(18, 2);

        builder.Property(a => a.RequestedAmount)
            .HasPrecision(18, 2);

        // Store the enum as readable text
        builder.Property(a => a.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(a => a.CreatedAt)
            .IsRequired();

        // Supports the background job's query for retrieving pending applications
        builder.HasIndex(a => new { a.Status, a.CreatedAt });

        builder.HasMany(a => a.DecisionLog)
            .WithOne(e => e.LoanApplication)
            .HasForeignKey(e => e.LoanApplicationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
