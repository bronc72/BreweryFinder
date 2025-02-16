using BreweryFinder.API.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using Scalar.AspNetCore;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

//builder.Services.AddHttpClient();
//Configure Typed HttpClient
//builder.Services.AddHttpClient<IBreweryService, BreweryService>()
//    .ConfigureHttpClient((serviceProvider, client) =>
//    {
//        client.BaseAddress = new Uri("https://api.openbrewerydb.org/");
//client.DefaultRequestHeaders.Add("Accept", "application/json");
//    });

builder.Services.AddHttpClient<IBreweryService, BreweryService>(
    client =>
    {
        client.BaseAddress = new Uri("https://api.openbrewerydb.org/");
        client.DefaultRequestHeaders.Add("Accept", "application/json");
    });

builder.Services.AddScoped<IBreweryService, BreweryService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
