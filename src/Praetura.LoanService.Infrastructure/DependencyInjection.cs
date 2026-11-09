using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Praetura.LoanService.Domain.Messaging;
using Praetura.LoanService.Domain.Repositories;
using Praetura.LoanService.Infrastructure.Messaging;
using Praetura.LoanService.Infrastructure.Persistence;
using Praetura.LoanService.Infrastructure.Persistence.Repositories;

namespace Praetura.LoanService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddDbContext<LoanServiceDbContext>(options => options.UseSqlite(connectionString));
        services.AddScoped<ILoanApplicationRepository, LoanApplicationRepository>();
        services.AddScoped<IOutboxRepository, OutboxRepository>();
        services.AddSingleton<IMessagePublisher, LoggingMessagePublisher>();

        return services;
    }
}
