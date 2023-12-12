using External.Mock.API.Service.Salesforce.Model;
using Mock.API.Services.Common.AspNetCoreHelpers;
using Mock.API.Services.Common.Chaos;
using Mock.API.Services.Common.ResiliencePolicy;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Polly;
using Polly.Registry;
using Polly.Wrap;
using System.Dynamic;
using System.Net;
using System.Text.Json;
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

// Inject IHttpClientFactory
builder.Services.AddHttpClient("SalesforceToken", httpClient =>
{
    httpClient.BaseAddress = new Uri(builder.Configuration["Salesforce:TokenUrl"]!);

    httpClient.DefaultRequestHeaders.Add(HeaderNames.UserAgent, "Salesforce-Mock-API");
});

var app = builder.Build();

// Configure the HTTP request pipeline.

// Configure Swagger
app.UseSwagger();
app.UseSwaggerUI();

// Add Simmy chaos injection. Wrap every policy in the policy registry in Simmy chaos injectors.
var registry = app.Services.GetRequiredService<IPolicyRegistry<string>>();
registry?.AddChaosInjectors();

app.UseHttpsRedirection();

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

/// TOKEN

/// <summary>
/// GetAuthToken
/// </summary>
app.MapPost("/oauth2/token", async (HttpContext httpContext, LinkGenerator links, HttpRequest request, IHttpClientFactory httpClientFactory,
    IOptionsSnapshot<AppChaosSettings> chaosOptionsSnapshot, IPolicyRegistry<string> registry) =>
{
    Task<IResult> response;
    // Get mockToken value
    _ = bool.TryParse(builder.Configuration["Salesforce:MockToken"], out bool mockToken);

    Context context = new Context("get_auth_token").WithChaosSettings(chaosOptionsSnapshot.Value);
    var policy = registry.Get<AsyncPolicyWrap<IResult>>("CircuitBreakerPolicy");

    // Check if mock token is disabled and query string has value
    if (!mockToken && request.QueryString.HasValue)
        // Get response from service
        response = policy.ExecuteAsync(ctx => GetAuthTokenFromService(httpClientFactory, request), context);
    else
        // Get response from file
        response = policy.ExecuteAsync(ctx => JsonResponseHelper.GetJsonResponseFromFile(httpContext, links, "get_auth_token", HttpStatusCode.OK), context);

    return await response;
}).WithName("get_auth_token");

/// QUERIES

/// <summary>
/// Build responses for ReadSalesforceByCustomerId, ReadCosts and CountCosts queries
/// </summary>
app.MapGet("/data/v52.0/query", async (HttpContext httpContext, LinkGenerator links, HttpRequest request,
    IOptionsSnapshot<AppChaosSettings> chaosOptionsSnapshot, IPolicyRegistry<string> registry) =>
{
    Task<IResult> response;
    Context context = new Context("queries").WithChaosSettings(chaosOptionsSnapshot.Value);
    var policy = registry.Get<AsyncPolicyWrap<IResult>>("CircuitBreakerPolicy");

    // Check if query string has value
    if (request.QueryString.HasValue)
        // Get response from method
        response = policy.ExecuteAsync(ctx => GetQueryResponse(httpContext, links, request), context);
    else
        // Get response from file
        response = policy.ExecuteAsync(ctx => JsonResponseHelper.GetJsonResponseFromFile(httpContext, links, "count_costs", HttpStatusCode.OK), context);

    return await response;
}).WithName("queries");

/// ACCOUNT

/// <summary>
/// CheckSalesforceApiStatus
/// </summary>
app.MapGet("/data/v54.0/sobjects/Account", async (HttpContext httpContext, LinkGenerator links,
    IOptionsSnapshot<AppChaosSettings> chaosOptionsSnapshot, IPolicyRegistry<string> registry) =>
{
    Context context = new Context("check_salesforce_api_status").WithChaosSettings(chaosOptionsSnapshot.Value);

    var policy = registry.Get<AsyncPolicyWrap<IResult>>("CircuitBreakerPolicy");

    var response = policy.ExecuteAsync(ctx => JsonResponseHelper.GetJsonResponseFromFile(httpContext, links, "check_salesforce_api_status", HttpStatusCode.OK), context);

    return await response;
}).WithName("check_salesforce_api_status");

/// <summary>
/// CreateAccountAsync
/// </summary>
app.MapPost("/data/v54.0/sobjects/Account", async (HttpContext httpContext, LinkGenerator links, SalesforceAccountRequest body,
    IOptionsSnapshot<AppChaosSettings> chaosOptionsSnapshot, IPolicyRegistry<string> registry) =>
{
    Context context = new Context("create_account_async").WithChaosSettings(chaosOptionsSnapshot.Value);

    var policy = registry.Get<AsyncPolicyWrap<IResult>>("CircuitBreakerPolicy");

    var response = policy.ExecuteAsync(ctx =>
    {
        var responseBody = CreateAccount();
        return JsonResponseHelper.GetJsonResponse(httpContext, links, "create_account_async", HttpStatusCode.Created, responseBody);
    }, context);

    return await response;
}).WithName("create_account_async");

/// <summary>
/// UpdateAccountAsync
/// </summary>
app.MapMethods("/data/v54.0/sobjects/Account/{salesforceId}", new[] { "PATCH" },
    async (HttpContext httpContext, LinkGenerator links, string salesforceId, SalesforceAccountRequest body,
    IOptionsSnapshot<AppChaosSettings> chaosOptionsSnapshot, IPolicyRegistry<string> registry) =>
{
    Context context = new Context("update_account_async").WithChaosSettings(chaosOptionsSnapshot.Value);

    var policy = registry.Get<AsyncPolicyWrap<IResult>>("CircuitBreakerPolicy");
    var response = policy.ExecuteAsync(async ctx => await Task.FromResult(Results.NoContent()), context);

    return await response;
}).WithName("update_account_async");

/// <summary>
/// UpsertAccountByCustomerIdAsync
/// </summary>
app.MapMethods("/data/v54.0/sobjects/Account/Customer_GUID__c/{customerId}", new[] { "PATCH" },
    async (HttpContext httpContext, LinkGenerator links, string customerId, SalesforceAccountRequest body,
    IOptionsSnapshot<AppChaosSettings> chaosOptionsSnapshot, IPolicyRegistry<string> registry) =>
{
    Context context = new Context("update_account_by_customer_id_async").WithChaosSettings(chaosOptionsSnapshot.Value);

    var policy = registry.Get<AsyncPolicyWrap<IResult>>("CircuitBreakerPolicy");

    var response = policy.ExecuteAsync(ctx =>
    {
        var responseBody = UpdateAccountByCustomerId();
        return JsonResponseHelper.GetJsonResponse(httpContext, links, "update_account_by_customer_id_async", HttpStatusCode.OK, responseBody);
    }, context);

    return await response;
}).WithName("update_account_by_customer_id_async");

/// MARKETING

/// <summary>
/// ProcessMarketingActivity
/// </summary>
app.MapPut("/apexrest/MarketingActivities", async (HttpContext httpContext, LinkGenerator links, object body,
    IOptionsSnapshot<AppChaosSettings> chaosOptionsSnapshot, IPolicyRegistry<string> registry) =>
{
    Context context = new Context("process_marketing_activities").WithChaosSettings(chaosOptionsSnapshot.Value);

    var policy = registry.Get<AsyncPolicyWrap<IResult>>("CircuitBreakerPolicy");

    var response = policy.ExecuteAsync(ctx => JsonResponseHelper.GetJsonResponseFromFile(httpContext, links, "process_marketing_activities", HttpStatusCode.InternalServerError), context);

    return await response;
}).WithName("process_marketing_activities");

/// <summary>
/// ProcessMarketingCampaign
/// </summary>
app.MapPut("/apexrest/MarketingCampaigns", async (HttpContext httpContext, LinkGenerator links, object body,
    IOptionsSnapshot<AppChaosSettings> chaosOptionsSnapshot, IPolicyRegistry<string> registry) =>
{
    Context context = new Context("process_marketing_campaigns").WithChaosSettings(chaosOptionsSnapshot.Value);

    var policy = registry.Get<AsyncPolicyWrap<IResult>>("CircuitBreakerPolicy");

    var response = policy.ExecuteAsync(ctx => JsonResponseHelper.GetJsonResponseFromFile(httpContext, links, "process_marketing_campaigns", HttpStatusCode.InternalServerError), context);

    return await response;
}).WithName("process_marketing_campaigns");

/// COSTS

/// <summary>
/// UpdateCostsAsync
/// </summary>
app.MapMethods("/data/v50.0/composite/sobjects/Cost__c/Cost_Key__c", new[] { "PATCH" },
    async (HttpContext httpContext, LinkGenerator links, object body,
    IOptionsSnapshot<AppChaosSettings> chaosOptionsSnapshot, IPolicyRegistry<string> registry) =>
{
    Context context = new Context("update_costs").WithChaosSettings(chaosOptionsSnapshot.Value);

    var policy = registry.Get<AsyncPolicyWrap<IResult>>("CircuitBreakerPolicy");

    var response = policy.ExecuteAsync(ctx => JsonResponseHelper.GetJsonResponseFromFile(httpContext, links, "update_costs", HttpStatusCode.OK), context);

    return await response;
}).WithName("update_costs");

/// <summary>
/// DeleteCostsAsync
/// </summary>
app.MapDelete("/data/v52.0/composite/sobjects", async (HttpContext httpContext, LinkGenerator links,
    IOptionsSnapshot<AppChaosSettings> chaosOptionsSnapshot, IPolicyRegistry<string> registry) =>
{
    Context context = new Context("update_costs").WithChaosSettings(chaosOptionsSnapshot.Value);

    var policy = registry.Get<AsyncPolicyWrap<IResult>>("CircuitBreakerPolicy");

    var response = policy.ExecuteAsync(ctx => JsonResponseHelper.GetJsonResponseFromFile(httpContext, links, "delete_costs", HttpStatusCode.BadRequest), context);

    return await response;
}).WithName("delete_costs");

app.Run();

async static Task<IResult> GetAuthTokenFromService(IHttpClientFactory httpClientFactory, HttpRequest request)
{
    var httpClient = httpClientFactory.CreateClient("SalesforceToken");
    var httpResponseMessage = await httpClient.GetAsync($"services/oauth2/token{request.QueryString.Value}");
    string jsonString = await httpResponseMessage.Content.ReadAsStringAsync();
    var result = JsonSerializer.Deserialize<object>(jsonString);
    return Results.Json(result);
}

static Task<IResult> GetQueryResponse(HttpContext httpContext, LinkGenerator links, HttpRequest request)
{
    // Decode query string from URL
    string queryString = request.Query["q"].ToString();
    // Create dynamic response
    Task<IResult> result = queryString switch
    {
        string s when s.StartsWith("SELECT id, Customer_GUID__c") => Task.FromResult(ReadSalesforceByCustomerId(queryString)),
        string s when s.StartsWith("SELECT id, Cost_Key__c") => JsonResponseHelper.GetJsonResponseFromFile(httpContext, links, "read_costs", HttpStatusCode.OK),
        _ => JsonResponseHelper.GetJsonResponseFromFile(httpContext, links, "count_costs", HttpStatusCode.OK),
    };
    return result;
}

static IResult ReadSalesforceByCustomerId(string decodedQueryString)
{
    // Get customer Id
    string customerGuid = decodedQueryString[decodedQueryString.IndexOf("'")..].Replace("'", "");
    // Gen Salesforce Id
    var salesforceId = GenerateSalesforceId();

    // Create record object
    dynamic record = new ExpandoObject();
    record.attributes = new Dictionary<string, string>();
    record.attributes.Add("type", "Account");
    record.attributes.Add("url", $"/services/data/v52.0/sobjects/Account/{salesforceId}");
    record.Id = salesforceId;
    record.Customer_GUID__c = customerGuid;

    // Create result object
    dynamic result = new ExpandoObject();
    result.totalSize = 1;
    result.done = true;
    result.records = new List<object>();
    result.records.Add(record);

    return Results.Json(result);
}

static string GenerateSalesforceId()
{
    // Generate random Salesforce Id
    var randomString = Convert.ToBase64String(Guid.NewGuid().ToByteArray())[..7];
    var rx = new Regex("[A-Za-z]{7}");
    while (!rx.IsMatch(randomString))
    {
        randomString = Convert.ToBase64String(Guid.NewGuid().ToByteArray())[..7];
    }
    return $"0015E000022{randomString}";
}

static object CreateAccount()
{
    dynamic result = new ExpandoObject();
    result.id = GenerateSalesforceId();
    result.success = true;
    result.errors = new List<string>();

    return result;
}

static object UpdateAccountByCustomerId()
{
    dynamic result = new ExpandoObject();
    result.id = GenerateSalesforceId();
    result.success = true;
    result.errors = new List<string>();
    result.created = false;
    return result;
}