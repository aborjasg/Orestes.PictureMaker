using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

//var identityProvider = builder
//    .AddProject<Projects.OpenIdConnectProvider>("identityprovider")
//    .WithExternalHttpEndpoints();

var apiService = builder.AddProject<Projects.AspireApp_ApiService>("apiservice");

builder.AddProject<Projects.AspireApp_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService);
    

builder.Build().Run();
