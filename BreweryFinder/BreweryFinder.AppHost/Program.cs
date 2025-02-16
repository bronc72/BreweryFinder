var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache").WithRedisInsight();
var apiService = builder.AddProject<Projects.BreweryFinder_API>("apiservice").WithReference(cache)
 .WaitFor(cache);


builder.AddProject<Projects.BreweryFinder_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WithReference(cache)
    .WaitFor(apiService);

builder.Build().Run();
