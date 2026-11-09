using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Praetura.LoanService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLoanApplicationStatusIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_LoanApplications_Status_CreatedAt",
                table: "LoanApplications",
                columns: new[] { "Status", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LoanApplications_Status_CreatedAt",
                table: "LoanApplications");
        }
    }
}
