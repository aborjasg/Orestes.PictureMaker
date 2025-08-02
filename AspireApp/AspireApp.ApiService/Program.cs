using AspireApp.ApiService.DataAccess;
using AspireApp.Libraries.DataSource;
using AspireApp.Libraries.Enums;
using AspireApp.Libraries.Models;
using AspireApp.Libraries.PictureMaker;
using AspireApp.ServiceDefaults.Shared;
using Grpc.Core;
using IdentityModel.OidcClient;
using MathNet.Numerics;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using OpenIddict.Validation.AspNetCore;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Mail;
using System.Reflection.Metadata;
using System.Security.Claims;
using static OpenIddict.Abstractions.OpenIddictConstants;
using static OpenIddict.Client.WebIntegration.OpenIddictClientWebIntegrationConstants;
using static System.Net.Mime.MediaTypeNames;


var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();
builder.Services.AddSingleton<ConnectionString>(sp =>
{
    return  new ConnectionString() { PictureMaker_Visualization = builder.Configuration["ConnectionString:PictureMaker_Visualization"]!}; 
});

//builder.Services.AddOpenIddict()
//     .AddClient(options =>
//      {
//          options.UseAspNetCore()
//                .DisableTransportSecurityRequirement();
//      })
//    .AddServer(options =>
//    {
//        options.AllowClientCredentialsFlow().AllowRefreshTokenFlow();
//        options.AllowPasswordFlow().AllowRefreshTokenFlow();

//        // Encryption and signing of tokens
//        options
//            .AddDevelopmentEncryptionCertificate()
//            .AddDevelopmentSigningCertificate()
//            .DisableAccessTokenEncryption();

//        options
//            .SetAuthorizationEndpointUris("/connect/authorize")
//            .SetTokenEndpointUris("/connect/token");

//        // Register the ASP.NET Core host and configure the ASP.NET Core options.
//        options        
//            .UseAspNetCore()
//            .EnableTokenEndpointPassthrough()
//            .EnableAuthorizationEndpointPassthrough();

//        //var identityServer = builder.Configuration.GetSection("IdentityServer");
//        //if (identityServer != null)
//        //{
//        //    options.SetAuthorizationEndpointUris(identityServer["endpoint_authorization"]!)
//        //            //.SetLogoutEndpointUris("connect/logout")
//        //            .SetTokenEndpointUris(identityServer["endpoint_token"]!);

//        //    options.RegisterScopes(Scopes.Email, Scopes.Profile, Scopes.Roles);

//        //    options.AcceptAnonymousClients();
//        //    options.AllowAuthorizationCodeFlow();

//        //    options.UseAspNetCore()
//        //        .DisableTransportSecurityRequirement();
//        //    //.EnableAuthorizationEndpointPassthrough()               
//        //    //.EnableTokenEndpointPassthrough();
//        //}

//    })
//    .AddValidation(options =>
//    {
//        var identityServer = builder.Configuration.GetSection("IdentityServer");
//        if (identityServer != null)
//        {
//            options.SetIssuer(identityServer["server_url"]!);
//            options.AddEncryptionKey(new SymmetricSecurityKey(Convert.FromBase64String(identityServer["client_secret"]!)));
//            options.UseSystemNetHttp();

//            options.UseLocalServer();
//            options.UseAspNetCore();
//        }
//    });

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
});



builder.Services.AddSwaggerGen(swagger=>
{
    swagger.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title="AspireApp.API", Version="v1.0", Description="API endpoints"});
});

//builder.Services
//    .AddAuthentication(options =>
//    {
//        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
//        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
//    })
//    .AddCookie()
//    .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
//    {
//        var oidcConfig = builder.Configuration.GetSection("OpenIdConnect");
//        options.Authority = oidcConfig["Authority"];
//        options.ClientSecret = oidcConfig["ClientSecret"];
//        options.ClientId = oidcConfig["ClientId"];
//        options.ResponseType = OpenIdConnectResponseType.Code;

//        options.Scope.Clear();
//        options.Scope.Add("openid");
//        options.Scope.Add("profile");
//        options.Scope.Add("email");
//        options.Scope.Add("offline_access");

//        options.ClaimActions.Remove("amr");
//        options.ClaimActions.MapUniqueJsonKey("website", "website");

//        options.GetClaimsFromUserInfoEndpoint = true;
//        options.SaveTokens = true;

//        // .NET 9 feature
//        options.PushedAuthorizationBehavior = PushedAuthorizationBehavior.Require;

//        options.TokenValidationParameters.NameClaimType = "name";
//        options.TokenValidationParameters.RoleClaimType = "role";
//    }); 

//var requireAuthPolicy = new AuthorizationPolicyBuilder()
//    .RequireAuthenticatedUser()
//    .Build();

//builder.Services.AddAuthorizationBuilder()
//    .SetFallbackPolicy(requireAuthPolicy);

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

app.MapPost("/getSourceData", (ConnectionString connectionString, DerivedDataFilter filter) =>
{
    var result = new ActionResponse();
    try
    {
        var engine = new DataSourceEngine(filter.Name);
        var derivedData = engine.GetDerivedData();
        result.Message = "OK";
        result.Content = UtilsForMessages.Compress(UtilsForMessages.SerializeObject(derivedData));
    }
    catch (Exception ex)
    {
        result.Type = "Error";
        result.Message = ex.Message;
    }
    result.EndDate = DateTime.Now;
    return result;
});

app.MapPost("/processData", (ConnectionString connectionString, DerivedDataFilter filter) =>
{
    var result = new ActionResponse();
    var record = new RunImage();    
    try
    {
        if (!string.IsNullOrEmpty(filter.CompressedDerivedData))
        {
            var derivedData = UtilsForMessages.DeserializeObject<DerivedData>(UtilsForMessages.Decompress(filter.CompressedDerivedData))!;
            var dataEngine = new DataSourceEngine(derivedData.Name);
            var pictureTemplate = dataEngine.GetPictureTemplate();
            IPlotEngine? plotEngine = pictureTemplate.TestType switch
            {
                enmTestType.heatmapDM => new PlotterNCP(),
                enmTestType.ncps => new PlotterNCP(),
                enmTestType.spectrum => new PlotterSpectrum(),
                enmTestType.energy => new PlotterEnergy(),
                enmTestType.energy_cal => new PlotterEnergyCal(),
                enmTestType.uniformity => new PlotterUniformity(),
                enmTestType.stability => new PlotterStability(),
                _ => null
            };
            if (plotEngine != null)
            {
                var pictureEngine = new PictureEngine(pictureTemplate, derivedData, plotEngine!);
                var plotImage = pictureEngine.MakePicture()!;
                var metadata = new RunMetadata(derivedData.Name, pictureTemplate);
                record.LoadData(derivedData.Name, metadata, derivedData, plotImage);
                record.EndProcess = DateTime.Now;

                result.Message = "OK";
                result.Content = UtilsForMessages.Compress(UtilsForMessages.SerializeObject(record));
            }
            else
                throw new Exception("PlotEngine invalid");
        }
        else
            throw new Exception("DataSource invalid");
    }
    catch (Exception ex)
    {
        result.Type = "Error";
        result.Message = ex.Message;
    }
    result.EndDate = DateTime.Now;    
    return result;
});

app.MapPost("/saveRunImage", (ConnectionString connectionString, RunImage record) =>
{
    var result = new ActionResponse();
    try
    {
        var obj = new TableAccess<RunImage>(connectionString.PictureMaker_Visualization);
        result.Id = obj.SaveRow(record);
        result.Message = "Saved successfully";
    }
    catch (Exception ex)
    {
        result.Type = "Error";
        result.Message = ex.Message;
    }
    result.EndDate = DateTime.Now;
    return result;
});

app.MapGet("/getRunImage/{id}", (ConnectionString connectionString, int id) =>
{
    var result = new ActionResponse();
    var record = new RunImage();
    try
    {
        var obj = new TableAccess<RunImage>(connectionString.PictureMaker_Visualization);
              var temp = obj.GetRow(id);
        if (temp is not null)
            record = temp;
        else
           throw  new Exception("No row");

        record.EndProcess = DateTime.Now;
        result.Message = "OK";
        result.Content = UtilsForMessages.Compress(UtilsForMessages.SerializeObject(record));
    }
    catch (Exception ex)
    {
        result.Type = "Error";
        result.Message = ex.Message;
    }
    result.EndDate = DateTime.Now;
    return result;
});

app.UseSwagger(options => options.SerializeAsV2 = true);
app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "AspireApp API"));

app.MapDefaultEndpoints();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
//app.UseAuthentication();
//app.UseAuthorization();

app.Run();
