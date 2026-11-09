using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Praetura.LoanService.Infrastructure.Persistence;

namespace Praetura.LoanService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddDbContext<LoanServiceDbContext>(options => options.UseSqlite(connectionString));

        return services;
    }
}
