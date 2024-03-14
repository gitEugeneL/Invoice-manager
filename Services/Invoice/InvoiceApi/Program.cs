using System.Reflection;
using System.Security.Claims;
using System.Text;
using FluentValidation;
using InvoiceApi.Data;
using InvoiceApi.Endpoints;
using InvoiceApi.Repositories;
using InvoiceApi.Repositories.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services
    .AddScoped<IItemRepository, ItemRepository>()
    .AddScoped<IInvoiceRepository, InvoiceRepository>();

/*** FluentValidation files register ***/
builder.Services
    .AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

/*** Database connection ***/
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("SQLite")));
    // options.UseNpgsql(builder.Configuration.GetConnectionString("PSQL")));    

/*** Swagger configuration ***/
    builder.Services.AddSwaggerGen(c =>
    {
        c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
        {
            Description = "JWT Bearer Authorization with refresh token. Example: Bearer {your access token....}",
            In = ParameterLocation.Header,
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey
        });
        c.OperationFilter<SecurityRequirementsOperationFilter>();
    });

/*** JWT  auth configuration ***/
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8
                    .GetBytes(builder.Configuration.GetSection("Authentication:Key").Value!)),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(1) // allowed time deviation, 5min - default
            };
        });

/*** Auth policies ***/
    builder.Services.AddAuthorizationBuilder()
        .AddPolicy("base-policy", policy =>
            policy
                .RequireClaim(ClaimTypes.Email)
                .RequireClaim(ClaimTypes.NameIdentifier));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapInvoiceEndpoints();
app.MapItemEndpoints();

app.UseHttpsRedirection();

app.Run();
