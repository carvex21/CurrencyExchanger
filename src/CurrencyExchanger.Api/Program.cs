using CurrencyExchanger.Api.Models;
using CurrencyExchanger.Api.Services;
using CurrencyExchanger.Api.Validators;
using CurrencyExchanger.Api.Workers;
using CurrencyExchanger.Infrastructure;
using CurrencyExchanger.Infrastructure.GatewayLibrary;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers()
    .AddFluentValidation(config =>
    {
        // Register validators
        config.RegisterValidatorsFromAssemblyContaining<AdjustBalanceRequestValidator>();
        config.RegisterValidatorsFromAssemblyContaining<GetBalanceRequestValidator>();
    });

// Register WalletService
builder.Services.AddScoped<IWalletService, WalletService>();

// Configure options for EcbWorker
builder.Services.Configure<EcbWorker>(builder.Configuration.GetSection("EcbWorker"));

// Register AppDbContext with SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register CurrencyRateCache as a shared singleton
builder.Services.AddSingleton<CurrencyRateCache>();

// Register HttpClient for CurrencyGateway
builder.Services.AddHttpClient<CurrencyGateway>();

builder.Services.AddScoped<CurrencyGateway>();
builder.Services.AddScoped<ICurrencyGateway, CachedCurrencyGateway>();

// Register the CurrencyUpdateJob as a hosted service
builder.Services.AddHostedService<CurrencyUpdateJob>();

// Ensure the database is created
using (var scope = builder.Services.BuildServiceProvider().CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureCreated();
}

// Add logging
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.SetMinimumLevel(LogLevel.Information);
});

// Add Swagger for API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
