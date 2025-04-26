using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Trace; // Add this using directive for OpenTelemetry

var builder = DistributedApplication.CreateBuilder(args);
var sql = builder.AddSqlServer("sqlserver")
                 .WithLifetime(ContainerLifetime.Persistent);

var db = sql.AddDatabase("breweryfinderdb");

var cache = builder.AddRedis("cache").WithRedisInsight();
var apiService = builder.AddProject<Projects.BreweryFinder_API>("apiservice")
    .WithReference(cache)
    .WithReference(db)
    .WaitFor(db)
    .WaitFor(cache);


builder.AddProject<Projects.BreweryFinder_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WithReference(cache);

builder.Build().Run();
