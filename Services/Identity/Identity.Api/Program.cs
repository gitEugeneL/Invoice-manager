using System.Reflection;
using FluentValidation;
using Identity.Api.Data;
using Identity.Api.Endpoints;
using Identity.Api.Repositories;
using Identity.Api.Repositories.Interfaces;
using Identity.Api.Security;
using Identity.Api.Security.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services
    .AddScoped<IPasswordManager, PasswordManager>()
    .AddScoped<ITokenManager, TokenManager>()
    .AddScoped<IUserRepository, UserRepository>();

/*** FluentValidation files register ***/
builder.Services
    .AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

/*** Database connection ***/
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PSQL")));    
    // options.UseSqlite(builder.Configuration.GetConnectionString("SQLite")));

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

/*** Add Endpoints ***/
app.MapAuthEndpoints();

app.UseHttpsRedirection();

app.Run();

public abstract partial class Program { } // for tests