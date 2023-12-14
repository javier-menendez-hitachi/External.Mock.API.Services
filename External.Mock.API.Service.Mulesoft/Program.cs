namespace External.Mock.API.Service.Mulesoft
{
    using External.Mock.API.Service.Mulesoft.Callback;
    using External.Mock.API.Service.Mulesoft.Model.Wape;
    using Polly.Registry;
    using Polly;
    using System.Text.Json.Serialization;
    using External.Mock.API.Service.Mulesoft.Model.Response;
    using External.Mock.API.Service.Mulesoft.Model.WebsiteLoyalty;
    using External.Mock.API.Service.Mulesoft.Model.WebsiteMembersLoyalty;
    using global::Mock.API.Services.Common.ResiliencePolicy;
    using global::Mock.API.Services.Common.Chaos;

    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateSlimBuilder(args);

            // Configure the HTTP JSON serializer options to allow serialization AOT
            builder.Services.ConfigureHttpJsonOptions(options =>
            {
                // Add context for each Type to allow serialization AOT
                options.SerializerOptions.TypeInfoResolverChain.Add(MockResponseJsonSerializerContext.Default);
                options.SerializerOptions.TypeInfoResolverChain.Add(HealthResponseJsonSerializerContext.Default);
                options.SerializerOptions.TypeInfoResolverChain.Add(OAuthResponseJsonSerializerContext.Default);
                options.SerializerOptions.TypeInfoResolverChain.Add(GuestJsonSerializerContext.Default);
                options.SerializerOptions.TypeInfoResolverChain.Add(GuestLoginJsonSerializerContext.Default);
                options.SerializerOptions.TypeInfoResolverChain.Add(RewardJsonSerializerContext.Default);
                options.SerializerOptions.TypeInfoResolverChain.Add(SegmentJsonSerializerContext.Default);
                options.SerializerOptions.TypeInfoResolverChain.Add(SegmentInfoJsonSerializerContext.Default);
                options.SerializerOptions.TypeInfoResolverChain.Add(CustomerOfferJsonSerializerContext.Default);
                options.SerializerOptions.TypeInfoResolverChain.Add(CustomerTreatCategoryJsonSerializerContext.Default);
                options.SerializerOptions.TypeInfoResolverChain.Add(CustomerTreatCategoryListJsonSerializerContext.Default);
                options.SerializerOptions.TypeInfoResolverChain.Add(GuestInfoJsonSerializerContext.Default);
                options.SerializerOptions.TypeInfoResolverChain.Add(RewardsInfoJsonSerializerContext.Default);
                options.SerializerOptions.TypeInfoResolverChain.Add(SendEmailJsonSerializerContext.Default);
                options.SerializerOptions.TypeInfoResolverChain.Add(MuleResponseTransJsonSerializerContext.Default);
                options.SerializerOptions.TypeInfoResolverChain.Add(MuleResponseCorrelationJsonSerializerContext.Default);
            });

            // Configuring Swagger/OpenAPI 
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddMvcCore();

            // Add this to listen and apply appsettings.json changes live
            builder.Configuration.AddJsonFile(path: "appsettings.json", optional: false, reloadOnChange: true);

            // Create (and register with DI) a policy registry containing some policies we want to use.
            builder.Services.AddPolicyRegistry(new PolicyRegistry
            {
                { "CircuitBreakerPolicy",  new CircuitBreaker(new Context("hello")).GetInstance()},
            });

            // Add ability for the app to populate ChaosSettings from json file (or any other .NET Core configuration source)
            builder.Services.Configure<AppChaosSettings>(builder.Configuration.GetSection("ChaosSettings"));
            builder.Services.Configure<WapeCallbackSettings>(builder.Configuration.GetSection("WapeCallbackSettings"));

            var app = builder.Build();
            
            // Enable the Developer Exception Page to get more detailed error messages
            app.UseDeveloperExceptionPage();

            // Use Swagger
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

            app.Run();
        }

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
    }


    [JsonSerializable(typeof(MockResponse))]
    internal partial class MockResponseJsonSerializerContext : JsonSerializerContext { }

    [JsonSerializable(typeof(HealthResponse))]
    internal partial class HealthResponseJsonSerializerContext : JsonSerializerContext { }

    [JsonSerializable(typeof(OAuthResponse))]
    internal partial class OAuthResponseJsonSerializerContext : JsonSerializerContext { }


    [JsonSerializable(typeof(Guest))]
    internal partial class GuestJsonSerializerContext : JsonSerializerContext { }
    
    [JsonSerializable(typeof(GuestLogin))]
    internal partial class GuestLoginJsonSerializerContext : JsonSerializerContext { }
    
    [JsonSerializable(typeof(List<Reward>))]
    internal partial class RewardJsonSerializerContext : JsonSerializerContext { }
    
    [JsonSerializable(typeof(List<Segment>))]
    internal partial class SegmentJsonSerializerContext : JsonSerializerContext { }
    

    [JsonSerializable(typeof(SegmentInfo))]
    internal partial class SegmentInfoJsonSerializerContext : JsonSerializerContext { }


    [JsonSerializable(typeof(CustomerOffer))]
    internal partial class CustomerOfferJsonSerializerContext : JsonSerializerContext { }

    [JsonSerializable(typeof(CustomerTreatCategory))]
    internal partial class CustomerTreatCategoryJsonSerializerContext : JsonSerializerContext { }

    [JsonSerializable(typeof(GuestInfo))]
    internal partial class GuestInfoJsonSerializerContext : JsonSerializerContext { }

    [JsonSerializable(typeof(RewardsInfo))]
    internal partial class RewardsInfoJsonSerializerContext : JsonSerializerContext { }

    [JsonSerializable(typeof(SendEmail))]
    internal partial class SendEmailJsonSerializerContext : JsonSerializerContext { }

    [JsonSerializable(typeof(MuleResponseTrans))]
    internal partial class MuleResponseTransJsonSerializerContext : JsonSerializerContext { }

    [JsonSerializable(typeof(MuleResponseCorrelation))]
    internal partial class MuleResponseCorrelationJsonSerializerContext : JsonSerializerContext { }

    [JsonSerializable(typeof(List<CustomerTreatCategory>))]
    internal partial class CustomerTreatCategoryListJsonSerializerContext : JsonSerializerContext { }

}
