using External.Mock.API.Service.ZeroBounce.Model;
using Mock.API.Services.Common.AspNetCoreHelpers;
using Mock.API.Services.Common.Chaos;
using Mock.API.Services.Common.ResiliencePolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Registry;
using Polly.Wrap;
using System.Net;
using System.Web;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Create (and register with DI) a policy registry containing some policies we want to use.
builder.Services.AddPolicyRegistry(new PolicyRegistry
{
    { "CircuitBreakerPolicy",  new CircuitBreaker(new Context("hello")).GetInstance()},
});
// Add ability for the app to populate ChaosSettings from json file (or any other .NET Core configuration source)
builder.Services.Configure<AppChaosSettings>(builder.Configuration.GetSection("ChaosSettings"));

var app = builder.Build();

// Configure the HTTP request pipeline.

// Configure Swagger
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseStaticFiles();

// Add Simmy chaos injection. Wrap every policy in the policy registry in Simmy chaos injectors.
var registry = app.Services.GetRequiredService<IPolicyRegistry<string>>();
registry?.AddChaosInjectors();

// Use CORS
app.UseRouting();

/// ENDPOINTS

/// <summary>
/// Hello!
/// </summary>
app.MapGet("/", async (HttpContext httpContext, LinkGenerator links,
    IOptionsSnapshot<AppChaosSettings> chaosOptionsSnapshot, IPolicyRegistry<string> registry) =>
{
    Context context = new Context("hello").WithChaosSettings(chaosOptionsSnapshot.Value);

    var policy = registry.Get<AsyncPolicyWrap<IResult>>("CircuitBreakerPolicy");

    var response = policy.ExecuteAsync(ctx => JsonResponseHelper.GetJsonResponseFromFile(httpContext, links, "hello", HttpStatusCode.OK), context);

    return await response;
}).WithName("hello");

// Get credits!
app.MapGet("/getcredits", async (HttpContext httpContext, LinkGenerator links,
    IOptionsSnapshot<AppChaosSettings> chaosOptionsSnapshot,
    IPolicyRegistry<string> registry) =>
{
    Context context = new Context("get_credits").WithChaosSettings(chaosOptionsSnapshot.Value);

    var policy = registry.Get<AsyncPolicyWrap<IResult>>("CircuitBreakerPolicy");

    var response = policy.ExecuteAsync(
        async ctx =>
        {
            var responseBody = GetCreditsResult();
            return await JsonResponseHelper.GetJsonResponse(httpContext, links, "get_credits", HttpStatusCode.OK, responseBody);
        },
        context);

    return await response;
}).WithName("get_credits");

// Validate!
app.MapGet("/validate", async (HttpContext httpContext, LinkGenerator links, [FromQuery] string email,
    IOptionsSnapshot<AppChaosSettings> chaosOptionsSnapshot,
    IPolicyRegistry<string> registry) =>
{
    Context context = new Context("validate").WithChaosSettings(chaosOptionsSnapshot.Value);

    var policy = registry.Get<AsyncPolicyWrap<IResult>>("CircuitBreakerPolicy");

    var response = policy.ExecuteAsync(
        async ctx =>
        {
            var emailDecoded = HttpUtility.UrlDecode(email);
            var responseBody = GetValidateEmailResult(emailDecoded);
            return await JsonResponseHelper.GetJsonResponse(httpContext, links, "validate", HttpStatusCode.OK, responseBody);
        },
        context);

    return await response;
}).WithName("validate");

app.Run();

static CreditsResult GetCreditsResult() =>
    new()
    {
        Credits = new Random().Next(-1, 100000)
    };

static ValidateEmailResult GetValidateEmailResult(string email) =>
    new()
    {
        Address = email,
        Status = "valid",
        Sub_Status = "alias_address",
        Free_Email = true,
        Did_You_Mean = null,
        Account = email.Split("@")[0],
        Domain = email.Split("@")[1],
        Domain_Age_Days = "9692",
        Smtp_Provider = "",
        Mx_Found = "true",
        Mx_Record = "mx.example.com",
        Firstname = "zero",
        Lastname = "bounce",
        Gender = "male",
        Processed_At = DateTime.UtcNow.ToLongDateString()
    };