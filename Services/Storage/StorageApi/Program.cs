using MassTransit;
using Minio;
using StorageApi.Consumers;
using StorageApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IStorageService, StorageService>();

/*** MinIO fileStorage configuration ***/
builder.Services.AddMinio(options =>
{
    options.WithEndpoint(builder.Configuration["MinIOStorage:Endpoint"]);
    options.WithCredentials(
        builder.Configuration["MinIOStorage:AccessKey"],
        builder.Configuration["MinIOStorage:SecretKey"]
    );
    options.WithSSL(false);
    options.Build();
});

/*** MasTransit Rabbit-MQ configuration ***/
builder.Services.AddMassTransit(busConfigurator =>
{
    busConfigurator.SetKebabCaseEndpointNameFormatter();
    busConfigurator.AddConsumer<SaveFileConsumer>();
    
    busConfigurator.UsingRabbitMq((context, configurator) =>
    {
        configurator.Host(new Uri(builder.Configuration["MessageBroker:Host"]!), host =>
        {
            host.Username(builder.Configuration["MessageBroker:Username"]);
            host.Password(builder.Configuration["MessageBroker:Password"]);
        });
        configurator.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.Run();