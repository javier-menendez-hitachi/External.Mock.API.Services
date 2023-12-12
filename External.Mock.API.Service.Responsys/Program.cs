using Mock.API.Services.Common.AspNetCoreHelpers;
using Mock.API.Services.Common.Chaos;
using Mock.API.Services.Common.ResiliencePolicy;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Registry;
using Polly.Wrap;
using System.Dynamic;
using System.Net;
using System.Text.RegularExpressions;

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

// Add Simmy chaos injection. Wrap every policy in the policy registry in Simmy chaos injectors.
var registry = app.Services.GetRequiredService<IPolicyRegistry<string>>();
registry?.AddChaosInjectors();

app.UseHttpsRedirection();

// Hello
app.MapGet("/", async (HttpContext httpContext, LinkGenerator links,
    IOptionsSnapshot<AppChaosSettings> chaosOptionsSnapshot, IPolicyRegistry<string> registry) =>
{
    Context context = new Context("hello").WithChaosSettings(chaosOptionsSnapshot.Value);

    var policy = registry.Get<AsyncPolicyWrap<IResult>>("CircuitBreakerPolicy");

    var response = policy.ExecuteAsync(ctx => JsonResponseHelper.GetJsonResponseFromFile(httpContext, links, "hello", HttpStatusCode.OK), context);

    return await response;
}).WithName("hello");

// Get Token
app.MapPost($"/rest/api/v{builder.Configuration["Responsys:Version"]}/auth/token",
    async (IOptionsSnapshot<AppChaosSettings> chaosOptionsSnapshot, IPolicyRegistry<string> registry) =>
{
    Context context = new Context("get_auth_token").WithChaosSettings(chaosOptionsSnapshot.Value);

    var policy = registry.Get<AsyncPolicyWrap<IResult>>("CircuitBreakerPolicy");

    var response = policy.ExecuteAsync(ctx => Task.FromResult(GetAuthToken(builder.Configuration["Responsys:EndPoint"]!)), context);

    return await response;
}).WithName("get_auth_token");

// Get all Lists
app.MapGet($"/rest/api/v{builder.Configuration["Responsys:Version"]}/lists",
    async (HttpContext httpContext, LinkGenerator links, IOptionsSnapshot<AppChaosSettings> chaosOptionsSnapshot, IPolicyRegistry<string> registry) =>
{
    Context context = new Context("get_list_all").WithChaosSettings(chaosOptionsSnapshot.Value);

    var policy = registry.Get<AsyncPolicyWrap<IResult>>("CircuitBreakerPolicy");

    var response = policy.ExecuteAsync(ctx => JsonResponseHelper.GetJsonResponseFromFile(httpContext, links, "get_list_all", HttpStatusCode.OK), context);

    return await response;
}).WithName("get_list_all");

// Get Members From List
app.MapGet($"/rest/api/v{builder.Configuration["Responsys:Version"]}/lists/{{listName}}",
    async (HttpContext httpContext, LinkGenerator links, IOptionsSnapshot<AppChaosSettings> chaosOptionsSnapshot, IPolicyRegistry<string> registry) =>
{
    Context context = new Context("get_list_query").WithChaosSettings(chaosOptionsSnapshot.Value);

    var policy = registry.Get<AsyncPolicyWrap<IResult>>("CircuitBreakerPolicy");

    var response = policy.ExecuteAsync(ctx => JsonResponseHelper.GetJsonResponseFromFile(httpContext, links, "get_list_query", HttpStatusCode.OK), context);

    return await response;
}).WithName("get_list_query");

// Execute Create/Update Member
app.MapPost($"/rest/api/v{builder.Configuration["Responsys:Version"]}/lists/{{listName}}",
    async (HttpContext httpContext, LinkGenerator links, IOptionsSnapshot<AppChaosSettings> chaosOptionsSnapshot, IPolicyRegistry<string> registry) =>
{
    Context context = new Context("post_upsert_member").WithChaosSettings(chaosOptionsSnapshot.Value);

    var policy = registry.Get<AsyncPolicyWrap<IResult>>("CircuitBreakerPolicy");

    var response = policy.ExecuteAsync(ctx => JsonResponseHelper.GetJsonResponseFromFile(httpContext, links, "post_upsert_member", HttpStatusCode.OK), context);

    return await response;
}).WithName("post_upsert_member");

// Create Profile Extension
app.MapPost($"/rest/api/v{builder.Configuration["Responsys:Version"]}/lists/{{listName}}/listExtensions/{{extension}}",
    async (HttpContext httpContext, LinkGenerator links, IOptionsSnapshot<AppChaosSettings> chaosOptionsSnapshot, IPolicyRegistry<string> registry) =>
{
    Context context = new Context("post_create_profile").WithChaosSettings(chaosOptionsSnapshot.Value);

    var policy = registry.Get<AsyncPolicyWrap<IResult>>("CircuitBreakerPolicy");

    var response = policy.ExecuteAsync(ctx => JsonResponseHelper.GetJsonResponseFromFile(httpContext, links, "post_create_profile", HttpStatusCode.OK), context);

    return await response;
}).WithName("post_create_profile");

// Get all Events
app.MapGet($"/rest/api/v{builder.Configuration["Responsys:Version"]}/events",
    async (HttpContext httpContext, LinkGenerator links, IOptionsSnapshot<AppChaosSettings> chaosOptionsSnapshot, IPolicyRegistry<string> registry) =>
{
    Context context = new Context("get_events_all").WithChaosSettings(chaosOptionsSnapshot.Value);

    var policy = registry.Get<AsyncPolicyWrap<IResult>>("CircuitBreakerPolicy");

    var response = policy.ExecuteAsync(ctx => JsonResponseHelper.GetJsonResponseFromFile(httpContext, links, "get_events_all", HttpStatusCode.OK), context);

    return await response;
}).WithName("get_events_all");

// Execute Trigger Program/Email
app.MapPost($"/rest/api/v{builder.Configuration["Responsys:Version"]}/events/{{eventName}}",
    async (HttpContext httpContext, LinkGenerator links, IOptionsSnapshot<AppChaosSettings> chaosOptionsSnapshot, IPolicyRegistry<string> registry) =>
{
    Context context = new Context("post_events_trigger_custom").WithChaosSettings(chaosOptionsSnapshot.Value);

    var policy = registry.Get<AsyncPolicyWrap<IResult>>("CircuitBreakerPolicy");

    var response = policy.ExecuteAsync(ctx => JsonResponseHelper.GetJsonResponseFromFile(httpContext, links, "post_events_trigger_custom", HttpStatusCode.OK), context);

    return await response;
}).WithName("post_events_trigger_custom");

// Get Campaigns
app.MapGet($"/rest/api/v{builder.Configuration["Responsys:Version"]}/campaigns",
    async (HttpRequest request, HttpContext httpContext, LinkGenerator links,
    IOptionsSnapshot<AppChaosSettings> chaosOptionsSnapshot, IPolicyRegistry<string> registry) =>
{
    Context context = new Context("get_campaigns").WithChaosSettings(chaosOptionsSnapshot.Value);

    var policy = registry.Get<AsyncPolicyWrap<IResult>>("CircuitBreakerPolicy");

    var response = policy.ExecuteAsync(ctx => GetCapmaignsResponse(httpContext, links, request, "get_campaigns"), context);

    return await response;
}).WithName("get_campaigns");

// Execute Trigger Campaign
app.MapPost($"/rest/api/v{builder.Configuration["Responsys:Version"]}/campaigns/{{campaignName}}/email",
async (HttpContext httpContext, LinkGenerator links, IOptionsSnapshot<AppChaosSettings> chaosOptionsSnapshot, IPolicyRegistry<string> registry) =>
{
    Context context = new Context("post_campaigns_trigger_email").WithChaosSettings(chaosOptionsSnapshot.Value);

    var policy = registry.Get<AsyncPolicyWrap<IResult>>("CircuitBreakerPolicy");

    var response = policy.ExecuteAsync(ctx => JsonResponseHelper.GetJsonResponseFromFile(httpContext, links, "post_campaigns_trigger_email", HttpStatusCode.OK), context);

    return await response;
}).WithName("post_campaigns_trigger_email");

app.Run();

static IResult GetAuthToken(string endPoint)
{
    // Create result object
    dynamic result = new ExpandoObject();
    // Generate random Auth Token
    var plainTextBytes = System.Text.Encoding.UTF8.GetBytes($"{Guid.NewGuid()}{Guid.NewGuid()}");
    var randomString = Convert.ToBase64String(plainTextBytes)[..52];
    var rx = new Regex("[A-Za-z0-9]{52}");
    while (!rx.IsMatch(randomString))
    {
        plainTextBytes = System.Text.Encoding.UTF8.GetBytes($"{Guid.NewGuid()}{Guid.NewGuid()}");
        randomString = Convert.ToBase64String(plainTextBytes)[..52];
    }
    result.authToken = randomString;
    result.issuedAt = DateTime.UtcNow.Ticks;
    result.endPoint = endPoint;

    return Results.Json(result);
}

static Task<IResult> GetCapmaignsResponse(HttpContext httpContext, LinkGenerator links, HttpRequest request, string prefix)
{
    string? campaignType = request.QueryString.HasValue ? request.Query["type"] : "email";
    string[] validTypes = { "email", "sms", "mms", "push", "pushio" };

    // Set default value when not valid type is informed
    if (!validTypes.Contains(campaignType))
        campaignType = "email";

    string endpointName = $"{prefix}_{campaignType}";

    return JsonResponseHelper.GetJsonResponseFromFile(httpContext, links, endpointName, HttpStatusCode.OK);
}