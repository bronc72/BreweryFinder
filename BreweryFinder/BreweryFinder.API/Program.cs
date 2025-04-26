using BreweryFinder.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddExceptionHandler(options =>
{
    options.ExceptionHandlingPath = "/error";
    options.CreateScopeForErrors = true; // Adjusted to use a valid property
});

builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.AddConsole();
    loggingBuilder.AddDebug();
});

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

// Add services to the container.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

builder.AddRedisDistributedCache("cache");
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


builder.Services.AddHttpClient<IBreweryService, BreweryService>(
    client =>
    {
        client.BaseAddress = new Uri("https://api.openbrewerydb.org/v1/breweries");
        client.DefaultRequestHeaders.Add("Accept", "application/json");
    });



builder.Services.AddScoped<IBreweryService, BreweryService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Servers = [];
    });
}

app.UseExceptionHandler("/info/error");


app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
