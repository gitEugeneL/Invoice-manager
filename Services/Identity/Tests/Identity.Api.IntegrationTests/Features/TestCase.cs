using System.Net.Http.Headers;
using System.Text;
using IdentityApi.Contracts;
using IdentityApi.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Identity.Api.IntegrationTests.Features;

public static class TestCase
{
    public static HttpClient CreateTestHttpClient(WebApplicationFactory<Program> factory, string dbname)
    {
        return factory
            .WithWebHostBuilder(builder => 
            {
                builder.ConfigureServices(services =>
                {
                    services.Remove(services.SingleOrDefault(service =>
                        service.ServiceType == typeof(DbContextOptions<AppDbContext>))!);

                    services.AddDbContext<AppDbContext>(options =>
                        options.UseInMemoryDatabase(dbname));
                });
            })
            .CreateClient();
    }
    
    public static async Task<T?> DeserializeResponse<T>(HttpResponseMessage response)
    {
        var jsonResponse = await response.Content.ReadAsStringAsync(); 
        return JsonConvert.DeserializeObject<T>(jsonResponse);
    }
    
    public static async Task<LoginResponse> Login(HttpClient client, string email, string password)
    {
        var model = new LoginRequest(email, password);
        var response = await client.PostAsync("api/v1/auth/login", CreateContext(model));
        var loginResponse = JsonConvert.DeserializeObject<LoginResponse>(await response.Content.ReadAsStringAsync())!;

        client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        return loginResponse;
    }
    
    public static StringContent CreateContext(object o)
    {
        return new StringContent(
            JsonConvert.SerializeObject(o), Encoding.UTF8, "application/json");
    }
}