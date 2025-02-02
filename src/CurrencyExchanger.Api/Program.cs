using Autofac;
using Autofac.Extensions.DependencyInjection;
using CurrencyExchanger.Api.Models;
using CurrencyExchanger.Api.Services;
using CurrencyExchanger.Api.Validators;
using CurrencyExchanger.Api.Workers;
using CurrencyExchanger.Infrastructure;
using CurrencyExchanger.Infrastructure.GatewayLibrary;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddFluentValidation(config =>
    {
        config.RegisterValidatorsFromAssemblyContaining<AdjustBalanceRequestValidator>();
        config.RegisterValidatorsFromAssemblyContaining<GetBalanceRequestValidator>();
    });

//builder.Services.AddScoped<IWalletService, WalletService>();
// builder.Services.AddSingleton<CurrencyRateCache>();
// builder.Services.AddHttpClient<CurrencyGateway>();
// builder.Services.AddScoped<CurrencyGateway>();
// builder.Services.AddScoped<ICurrencyGateway, CachedCurrencyGateway>();
// Configure Autofac container
// using (var scope = builder.Services.BuildServiceProvider().CreateScope())
// {
//     var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
//     dbContext.Database.EnsureCreated();
// }
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
{
    containerBuilder.Register(context =>
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
        return new AppDbContext(optionsBuilder.Options);
    }).InstancePerLifetimeScope();

    containerBuilder
        .RegisterType<CurrencyRateCache>()
        .SingleInstance();

    containerBuilder
        .RegisterType<CurrencyGateway>()
        .As<ICurrencyGateway>()
        .InstancePerLifetimeScope();

    containerBuilder
        .RegisterType<CachedCurrencyGateway>()
        .As<ICurrencyGateway>()
        .InstancePerLifetimeScope();

    containerBuilder.RegisterType<CurrencyUpdateJob>().As<IHostedService>().SingleInstance();
});

builder.Services.AddHttpClient(); 

builder.Services.Configure<EcbWorker>(builder.Configuration.GetSection("EcbWorker"));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHostedService<CurrencyUpdateJob>();

builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.SetMinimumLevel(LogLevel.Information);
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();