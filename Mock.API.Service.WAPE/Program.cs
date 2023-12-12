using Mock.API.Service.WAPE.Model;
using Mock.API.Services.Common.AspNetCoreHelpers;
using Mock.API.Services.Common.Chaos;
using Mock.API.Services.Common.ResiliencePolicy;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Registry;
using Polly.Wrap;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMvcCore();

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

app.UsePathBase(new PathString("/api"));
app.UseRouting();

var root = app.MapGroup("/").AddEndpointFilterFactory(RequestAuditor);
var customer = root.MapGroup("/v3/customer");
var customerPreferences = root.MapGroup("/v3/customerpreferences");
var signIn = root.MapGroup("/v1/signin");

// Hello
root.MapGet("", 
    async(HttpContext httpContext, LinkGenerator links, IOptionsSnapshot <AppChaosSettings> chaosOptionsSnapshot, IPolicyRegistry<string> registry) =>
    await ChaosHandler("Hello", "CircuitBreakerPolicy", httpContext, links, chaosOptionsSnapshot, registry, null!, true))
    .WithName("Hello");

// RegisterCustomer
customer.MapPost("", async (HttpContext httpContext, LinkGenerator links,
    IOptionsSnapshot<AppChaosSettings> chaosOptionsSnapshot, IPolicyRegistry<string> registry, Customer? body) => {
        // Check Request
        if (body == null)
            return await JsonResponseHelper.GetJsonResponse(httpContext, links, "RegisterCustomer", HttpStatusCode.BadRequest, null!);  // BadRequest

        // Update Response
        body.EmailPermissionDateTime = DateTime.UtcNow;
        body.ChangingMessageDisplayDateTime = DateTime.UtcNow;
        body.OriginalRegistrationDate = DateTime.UtcNow;
        body.VersionId = Guid.NewGuid();
        body.DateCreated = DateTime.UtcNow;
        body.DateUpdated = DateTime.UtcNow;

        return await ChaosHandler("RegisterCustomer", "CircuitBreakerPolicy", httpContext, links, chaosOptionsSnapshot, registry, body);
    })
    .WithName("RegisterCustomer");

// ConfirmEmail
customer.MapPost("/confirmemail/{customerResetId}", async (HttpContext httpContext, LinkGenerator links,
    string customerResetId, IOptionsSnapshot<AppChaosSettings> chaosOptionsSnapshot, IPolicyRegistry<string> registry) => {
        var token = Guid.NewGuid();

        // Check Request
        if (!Guid.TryParse(customerResetId, out token))
            return await JsonResponseHelper.GetJsonResponse(httpContext, links, "ConfirmEmail", HttpStatusCode.BadRequest, null!);  // BadRequest

        return await ChaosHandler("ConfirmEmail", "CircuitBreakerPolicy", httpContext, links, chaosOptionsSnapshot, registry);
    })
    .WithName("ConfirmEmail");

// UpdateProfile
customer.MapPut("", async (HttpContext httpContext, LinkGenerator links,
    IOptionsSnapshot<AppChaosSettings> chaosOptionsSnapshot, IPolicyRegistry<string> registry, Customer? body) => {
        // Check Request
        if (body == null)
            return await JsonResponseHelper.GetJsonResponse(httpContext, links, "UpdateProfile", HttpStatusCode.BadRequest, null!);  // BadRequest

        // Update Response
        body.EmailPermissionDateTime = DateTime.UtcNow;
        body.ChangingMessageDisplayDateTime = DateTime.UtcNow;
        body.VersionId = Guid.NewGuid();
        body.DateUpdated = DateTime.UtcNow;

        return await ChaosHandler("UpdateProfile", "CircuitBreakerPolicy", httpContext, links, chaosOptionsSnapshot, registry, body);
    })
    .WithName("UpdateProfile");

// GetCustomer
customer.MapGet("/{customerId}", async (HttpContext httpContext, LinkGenerator links, string customerId,
    IOptionsSnapshot<AppChaosSettings> chaosOptionsSnapshot, IPolicyRegistry<string> registry) => {

        var guidCustomerId = Guid.NewGuid();

        // Check Request
        if (!Guid.TryParse(customerId, out guidCustomerId))
            return await JsonResponseHelper.GetJsonResponse(httpContext, links, "GetCustomer", HttpStatusCode.BadRequest, null!);  // BadRequest

        return await ChaosHandler("GetCustomer", "CircuitBreakerPolicy", httpContext, links, chaosOptionsSnapshot, registry, null!, true);
    })
    .WithName("GetCustomer");

// ResetPasswordRequest
customer.MapPost("/passwordresetrequest/{email}", async (HttpContext httpContext, LinkGenerator links, string email,
    IOptionsSnapshot<AppChaosSettings> chaosOptionsSnapshot, IPolicyRegistry<string> registry) => {
        if (string.IsNullOrEmpty(email))
            return await JsonResponseHelper.GetJsonResponse(httpContext, links, "ResetPasswordRequest", HttpStatusCode.BadRequest, null!);  // BadRequest

        return await ChaosHandler("ResetPasswordRequest", "CircuitBreakerPolicy", httpContext, links, chaosOptionsSnapshot, registry);
    })
    .WithName("ResetPasswordRequest");

// ResetPassword
customer.MapPost("/passwordreset/{customerResetId}", async (HttpContext httpContext, LinkGenerator links,
    string customerResetId, ResetPassword body, IOptionsSnapshot<AppChaosSettings> chaosOptionsSnapshot, IPolicyRegistry<string> registry) => {
        var token = Guid.NewGuid();

        // Check Request
        if (!Guid.TryParse(customerResetId, out token))
            return await JsonResponseHelper.GetJsonResponse(httpContext, links, "ResetPassword", HttpStatusCode.BadRequest, null!);  // BadRequest
        if (body == null)
            return await JsonResponseHelper.GetJsonResponse(httpContext, links, "ResetPassword", HttpStatusCode.BadRequest, null!);  // BadRequest

        return await ChaosHandler("ResetPassword", "CircuitBreakerPolicy", httpContext, links, chaosOptionsSnapshot, registry);
    })
    .WithName("ResetPassword");

// ChangePassword
customer.MapPost("/passwordchange/{customerId}", async (HttpContext httpContext, LinkGenerator links,
    string customerId, ResetPassword body, IOptionsSnapshot<AppChaosSettings> chaosOptionsSnapshot, IPolicyRegistry<string> registry) => {
        var token = Guid.NewGuid();

        // Check Request
        if (!Guid.TryParse(customerId, out token))
            return await JsonResponseHelper.GetJsonResponse(httpContext, links, "ChangePassword", HttpStatusCode.BadRequest, null!);  // BadRequest
        if (body == null)
            return await JsonResponseHelper.GetJsonResponse(httpContext, links, "ChangePassword", HttpStatusCode.BadRequest, null!);  // BadRequest

        return await ChaosHandler("ChangePassword", "CircuitBreakerPolicy", httpContext, links, chaosOptionsSnapshot, registry);
    }).WithName("ChangePassword");

// EmailPassword
customer.MapPost("/emailchange/{customerId}", async (HttpContext httpContext, LinkGenerator links,
    string customerId, EmailChange body, IOptionsSnapshot<AppChaosSettings> chaosOptionsSnapshot, IPolicyRegistry<string> registry) => {
        var token = Guid.NewGuid();

        // Check Request
        if (!Guid.TryParse(customerId, out token))
            return await JsonResponseHelper.GetJsonResponse(httpContext, links, "ChangeEmail", HttpStatusCode.BadRequest, null!);  // BadRequest
        if (body == null)
            return await JsonResponseHelper.GetJsonResponse(httpContext, links, "ChangeEmail", HttpStatusCode.BadRequest, null!);  // BadRequest

        return await ChaosHandler("ChangeEmail", "CircuitBreakerPolicy", httpContext, links, chaosOptionsSnapshot, registry);
    }).WithName("ChangeEmail");

// MulesoftUpdateBarcodeCallback
customer.MapPost("/updatebarcode/callback", async (HttpContext httpContext, LinkGenerator links,
    IOptionsSnapshot<AppChaosSettings> chaosOptionsSnapshot, IPolicyRegistry<string> registry, Guest? body) => {
        // Check Request
        if (body == null)
            return await JsonResponseHelper.GetJsonResponse(httpContext, links, "MulesoftUpdateBarcodeCallback", HttpStatusCode.BadRequest, null!);  // BadRequest

        return await ChaosHandler("MulesoftUpdateBarcodeCallback", "CircuitBreakerPolicy", httpContext, links, chaosOptionsSnapshot, registry);
    }).WithName("MulesoftUpdateBarcodeCallback");

// MulesoftUpdateCallback
customer.MapPut("/callback", async (HttpContext httpContext, LinkGenerator links,
    IOptionsSnapshot<AppChaosSettings> chaosOptionsSnapshot, IPolicyRegistry<string> registry, Guest? body) => {
        // Check Request
        if (body == null)
            return await JsonResponseHelper.GetJsonResponse(httpContext, links, "MulesoftUpdateCallback", HttpStatusCode.BadRequest, null!);  // BadRequest

        return await ChaosHandler("MulesoftUpdateCallback", "CircuitBreakerPolicy", httpContext, links, chaosOptionsSnapshot, registry);
    }).WithName("MulesoftUpdateCallback");

// UpdateCustomerPreferences
customerPreferences.MapPost("/{customerId}", async (HttpContext httpContext, LinkGenerator links,
    IOptionsSnapshot<AppChaosSettings> chaosOptionsSnapshot, IPolicyRegistry<string> registry, CustomerPreferencesUpdate? body) => {
        // Check Request
        if (body == null)
            return await JsonResponseHelper.GetJsonResponse(httpContext, links, "UpdateCustomerPreferences", HttpStatusCode.BadRequest, null!);  // BadRequest

        return await ChaosHandler("UpdateCustomerPreferences", "CircuitBreakerPolicy", httpContext, links, chaosOptionsSnapshot, registry, body);
    })
    .WithName("UpdateCustomerPreferences");

// SignIn
signIn.MapPost("", async (SignInRequest request, HttpContext httpContext, LinkGenerator links,
    IOptionsSnapshot<AppChaosSettings> chaosOptionsSnapshot, IPolicyRegistry<string> registry) => {
        // Check Request
        if (request == null)
            return await JsonResponseHelper.GetJsonResponse(httpContext, links, "SignIn", HttpStatusCode.BadRequest, null!);  // BadRequest

        return await ChaosHandler("SignIn", "CircuitBreakerPolicy", httpContext, links, chaosOptionsSnapshot, registry, request);
    })
    .WithName("SignIn");

app.Run();

static EndpointFilterDelegate RequestAuditor(EndpointFilterFactoryContext handlerContext, EndpointFilterDelegate next)
{
    var loggerFactory = handlerContext.ApplicationServices.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger("RequestAuditor");

    return (invocationContext) =>
    {
        logger.LogInformation($"[*] Received a request for {invocationContext.HttpContext.Request.Method} {invocationContext.HttpContext.Request.Path}");
        return next(invocationContext);
    };
}

async Task<IResult> ChaosHandler(string operationKey, string policyKey, HttpContext httpContext, 
    LinkGenerator links, IOptionsSnapshot<AppChaosSettings> chaosOptionsSnapshot, IPolicyRegistry<string> registry, 
    object body = null!, bool responseFromFile = false) 
{
    Task<IResult> response;

    Context context = new Context(operationKey).WithChaosSettings(chaosOptionsSnapshot.Value);

    var policy = registry.Get<AsyncPolicyWrap<IResult>>(policyKey);

    if (responseFromFile)
        response = policy.ExecuteAsync(ctx => JsonResponseHelper.GetJsonResponseFromFile(httpContext, links, operationKey, HttpStatusCode.OK), context);
    else
        response = policy.ExecuteAsync(ctx => JsonResponseHelper.GetJsonResponse(httpContext, links, operationKey, HttpStatusCode.OK, body), context);

    return await response;
};