var builder = DistributedApplication.CreateBuilder(args);

var database = builder.AddPostgres("postgres").AddDatabase("shopdb");
var cache = builder.AddRedis("cache");

builder.AddProject<Projects.Api>("api")
    .WithReference(database)
    .WithReference(cache)
    .WaitFor(database);

builder.Build().Run();
