var builder = DistributedApplication.CreateBuilder(args);

// Run PostgreSQL in a container with a persistent data volume.
var postgres = builder.AddPostgres("postgres")
    .WithDataVolume();

var moviesdb = postgres.AddDatabase("moviesdb");

// Start the API, hand it the database connection, and wait for the
// database to be ready before the API boots.
builder.AddProject<Projects.MovieManagement_Api>("api")
    .WithReference(moviesdb)
    .WaitFor(moviesdb);

builder.Build().Run();
