using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using InvoiceApi.Data;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace InvoiceApi.IntegrationTests.Features;

public static class TestCase
{
    private static readonly string FakeAuthKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(50));
    
    public static HttpClient CreateTestHttpClient(WebApplicationFactory<Program> factory, string dbname)
    {
        return factory
            .WithWebHostBuilder(builder => 
            {
                builder.ConfigureServices(services =>
                {
                    // fake auth
                    services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
                    {
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(FakeAuthKey)),
                            ValidateIssuer = false,
                            ValidateAudience = false,
                            ValidateLifetime = false
                        };
                    });
                    
                    // db in memory
                    services.Remove(services.SingleOrDefault(service =>
                        service.ServiceType == typeof(DbContextOptions<AppDbContext>))!);
                    services.AddDbContext<AppDbContext>(options =>
                        options.UseInMemoryDatabase(dbname));

                    // masstransit in memory
                    services.AddMassTransitTestHarness();
                });
                
            })
            .CreateClient();
    }

    public static string CreateFakeToken(Guid userId, string userEmail)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Email, userEmail)
        };
        
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(FakeAuthKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
        
        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(10),
            SigningCredentials = credentials
        };
        var handler = new JwtSecurityTokenHandler();
        var token = handler.CreateToken(descriptor);

        return handler.WriteToken(token);
    }
    
    public static void IncludeTokenInRequest(HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }
    
    public static async Task<T> DeserializeResponse<T>(HttpResponseMessage response)
    {
        var jsonResponse = await response.Content.ReadAsStringAsync(); 
        return JsonConvert.DeserializeObject<T>(jsonResponse)!;
    }
    
    public static StringContent CreateContext(object o)
    {
        return new StringContent(
            JsonConvert.SerializeObject(o), Encoding.UTF8, "application/json");
    }
}