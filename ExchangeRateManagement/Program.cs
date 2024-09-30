using ExchangeRateManagement.Domain.Interfaces.Services;
using ExchangeRateManagement.Domain.Middlewares;
using ExchangeRateManagement.Infra.Repositories;
using ExchangeRateManagement.Service.Services;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();

builder.Services.AddDbContext<ExchangeRateContext>();

builder.Services.AddScoped<ICurrencyPairService, CurrencyPairService>();
builder.Services.AddScoped<ICurrencyPairIntegrationService, CurrencyPairIntegrationService>();
builder.Services.AddSingleton<IMessagingService, MessagingService>();
builder.Services.AddSingleton<IConnectionFactory>(sp =>
{
    return new ConnectionFactory()
    {
        HostName = "localhost", 
        UserName = "guest",
        Password = "guest", 
    };
});

builder.Services.AddDbContext<ExchangeRateContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ExchangeRateContext>();
    var pendingMigrations = dbContext.Database.GetPendingMigrations();

    if (pendingMigrations.Any())
    {
        dbContext.Database.Migrate();
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
