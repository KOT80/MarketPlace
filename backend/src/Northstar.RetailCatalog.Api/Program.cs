using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Northstar.RetailCatalog.Api.Infrastructure;
using Northstar.RetailCatalog.Application;
using Northstar.RetailCatalog.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

if (app.Configuration.GetValue("Database:MigrateOnStartup", false))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<Northstar.RetailCatalog.Infrastructure.Data.RetailCatalogDbContext>();
    await db.Database.MigrateAsync();
}

app.Run();

public partial class Program;
