using External.Mock.API.Service.Mulesoft;
using External.Mock.API.Service.Mulesoft.Callback;
using Mock.API.Services.Common.Chaos;
using Mock.API.Services.Common.ResiliencePolicy;
using Polly;
using Polly.Registry;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMvcCore();
builder.Configuration.AddJsonFile("appsettings.json", false, true);

// Create (and register with DI) a policy registry containing some policies we want to use.
builder.Services.AddPolicyRegistry(new PolicyRegistry
{
    { "CircuitBreakerPolicy",  new CircuitBreaker(new Context("hello")).GetInstance()},
});
// Add ability for the app to populate ChaosSettings from json file (or any other .NET Core configuration source)
builder.Services.Configure<AppChaosSettings>(builder.Configuration.GetSection("ChaosSettings"));
builder.Services.Configure<WapeCallbackSettings>(builder.Configuration.GetSection("WapeCallbackSettings"));

// Build App
var app = builder.Build();

// Configure the HTTP request pipeline.

// Configure Swagger
app.UseSwagger();
app.UseSwaggerUI();

// Add Simmy chaos injection. Wrap every policy in the policy registry in Simmy chaos injectors.
var registry = app.Services.GetRequiredService<IPolicyRegistry<string>>();
registry?.AddChaosInjectors();

// Add HTTP to HTTPS redirection
app.UseHttpsRedirection();
// Add Routing
app.UseRouting();

// Use groups to define endpoints
var root = app.MapGroup("/").Root().AddEndpointFilterFactory(RequestAuditor);
root.MapGroup("wape/v2").Wape();
root.MapGroup("website-members-loyalty/v1").WebsiteMembersLoyalty();
root.MapGroup("website-loyalty/v1").WebsiteLoyalty();

// Run the App
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