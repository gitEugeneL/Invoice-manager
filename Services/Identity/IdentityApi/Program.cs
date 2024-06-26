using System.Reflection;
using Carter;
using FluentValidation;
using IdentityApi.Data;
using IdentityApi.Middleware;
using IdentityApi.Security;
using IdentityApi.Security.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services
    .AddScoped<IPasswordService, PasswordService>()
    .AddScoped<ITokenService, TokenService>();

/*** FluentValidation files register ***/
builder.Services
    .AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

/*** Database connection ***/
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PSQL")));    
    // options.UseSqlite(builder.Configuration.GetConnectionString("SQLite")));

/*** MediatR configuration ***/
builder.Services.AddMediatR(config =>
    config.RegisterServicesFromAssembly(typeof(Program).Assembly));

/*** Carter configuration ***/
builder.Services.AddCarter();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

/*** Update develop database ***/
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetService<AppDbContext>()!;
    context.Database.EnsureCreated();
}

app.UseSwagger();
app.UseSwaggerUI();

app.MapCarter();

app.UseHttpsRedirection();

app.UseExceptionHandler();

app.Run();

public abstract partial class Program { } // for tests