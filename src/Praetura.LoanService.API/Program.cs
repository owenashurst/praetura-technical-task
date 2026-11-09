using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Praetura.LoanService.API.Configuration;
using Praetura.LoanService.API.ErrorHandling;
using Praetura.LoanService.API.Features.LoanApplications.CreateLoanApplication;
using Praetura.LoanService.API.Features.LoanApplications.GetLoanApplication;
using Praetura.LoanService.API.Features.LoanApplications.ProcessLoanApplications;
using Praetura.LoanService.Domain.Eligibility;
using Praetura.LoanService.Infrastructure;
using Praetura.LoanService.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var connectionString = builder.Configuration.GetConnectionString("LoanService")
    ?? throw new InvalidOperationException("Connection string 'LoanService' is not configured.");

builder.Services.AddInfrastructure(connectionString);

builder.Services.AddOptions<EligibilityOptions>()
    .Bind(builder.Configuration.GetSection(nameof(EligibilityOptions)))
    .ValidateOnStart();
builder.Services.AddSingleton<IValidateOptions<EligibilityOptions>, EligibilityOptionsValidator>();
builder.Services.AddSingleton<IEligibilityEvaluator>(sp =>
    new EligibilityEvaluator(sp.GetRequiredService<IOptions<EligibilityOptions>>().Value));

builder.Services.AddOptions<ProcessingOptions>()
    .Bind(builder.Configuration.GetSection(nameof(ProcessingOptions)))
    .ValidateOnStart();
builder.Services.AddSingleton<IValidateOptions<ProcessingOptions>, ProcessingOptionsValidator>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddScoped<ICreateLoanApplicationHandler, CreateLoanApplicationHandler>();
builder.Services.AddScoped<IGetLoanApplicationHandler, GetLoanApplicationHandler>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<LoanServiceDbContext>();
    dbContext.Database.Migrate();
}

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
