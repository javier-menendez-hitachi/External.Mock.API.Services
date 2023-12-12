using Mock.API.Services.Common.AspNetCoreHelpers;
using Mock.API.Services.Common.Chaos;
using Mock.API.Services.Common.ResiliencePolicy;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Registry;
using Polly.Wrap;
using System.Dynamic;
using System.Net;
using System.Numerics;
using External.Mock.API.Service.Coniq.Model;
using System.Security.Cryptography;

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

// Hello!
app.MapGet("/", async (HttpContext httpContext, LinkGenerator links,
    IOptionsSnapshot<AppChaosSettings> chaosOptionsSnapshot,
    IPolicyRegistry<string> registry) =>
{
    Context context = new Context("hello").WithChaosSettings(chaosOptionsSnapshot.Value);

    var policy = registry.Get<AsyncPolicyWrap<IResult>>("CircuitBreakerPolicy");

    var response = policy.ExecuteAsync(
        ctx => JsonResponseHelper.GetJsonResponseFromFile(httpContext, links, "hello", HttpStatusCode.OK),
        context);

    return await response;
}).WithName("hello");

// Barcode
app.MapGet("/barcode", async (HttpContext httpContext, LinkGenerator links,
    IOptionsSnapshot<AppChaosSettings> chaosOptionsSnapshot, IPolicyRegistry<string> registry) =>
{
    Context context = new Context("get_barcode").WithChaosSettings(chaosOptionsSnapshot.Value);

    var policy = registry.Get<AsyncPolicyWrap<IResult>>("CircuitBreakerPolicy");

    var response = policy.ExecuteAsync(
        async ctx =>
        {
            var responseBody = GetBarcodes(builder.Configuration["Coniq:OfferId"]!);
            return await JsonResponseHelper.GetJsonResponse(httpContext, links, "get_barcode", HttpStatusCode.OK, responseBody);
        },
        context);

    return await response;
}).WithName("get_barcode");

app.MapPost("/barcode", async (HttpContext httpContext, LinkGenerator links,
    IOptionsSnapshot<AppChaosSettings> chaosOptionsSnapshot, IPolicyRegistry<string> registry, ConiqCustomer body) =>
{
    // Check Request
    if (body == null)
        return await JsonResponseHelper.GetJsonResponse(httpContext, links, "post_barcode", HttpStatusCode.BadRequest, null!);  // BadRequest

    Context context = new Context("post_barcode").WithChaosSettings(chaosOptionsSnapshot.Value);

    var policy = registry.Get<AsyncPolicyWrap<IResult>>("CircuitBreakerPolicy");
    var response = policy.ExecuteAsync(
        async ctx =>
        {
            var genBarcodeFromGuid = builder.Configuration.GetValue<bool>("Coniq:CreateBarcodeFromGuid");
            var responseBody = genBarcodeFromGuid ? CreateBarcodeFromGuid(body) : CreateBarcodeInRange(body);
            return await JsonResponseHelper.GetJsonResponse(httpContext, links, "post_barcode", HttpStatusCode.Created, responseBody);
        },
        context);

    return await response;
}).WithName("post_barcode"); // Should return 201

app.MapPost("/barcode/batch", async (HttpContext httpContext, LinkGenerator links,
    IOptionsSnapshot<AppChaosSettings> chaosOptionsSnapshot, IPolicyRegistry<string> registry) =>
{
    Context context = new Context("get_barcode_batch").WithChaosSettings(chaosOptionsSnapshot.Value);

    var policy = registry.Get<AsyncPolicyWrap<IResult>>("CircuitBreakerPolicy");
    var response = policy.ExecuteAsync(
        async ctx =>
        {
            var responseBody = CreateBarcodeBatch(builder.Configuration["Coniq:OfferId"]!);
            return await JsonResponseHelper.GetJsonResponse(httpContext, links, "get_barcode_batch", HttpStatusCode.Created, responseBody);
        },
        context);

    return await response;
}).WithName("get_barcode_batch");

// Customer
app.MapGet("/customer/{id}", async (HttpContext httpContext, LinkGenerator links, string id,
    IOptionsSnapshot<AppChaosSettings> chaosOptionsSnapshot, IPolicyRegistry<string> registry) =>
{
    Context context = new Context("get_customer_by_id").WithChaosSettings(chaosOptionsSnapshot.Value);

    var policy = registry.Get<AsyncPolicyWrap<IResult>>("CircuitBreakerPolicy");
    var response = policy.ExecuteAsync(
        async ctx =>
        {
            var body = GetCustomerById(id);
            return await JsonResponseHelper.GetJsonResponse(httpContext, links, "get_customer_by_id", HttpStatusCode.OK, body);
        },
        context);

    return await response;
}).WithName("get_customer_by_id");

app.MapGet("/customer", async (HttpContext httpContext, LinkGenerator links,
    IOptionsSnapshot<AppChaosSettings> chaosOptionsSnapshot, IPolicyRegistry<string> registry) =>
{
    Context context = new Context("get_customer").WithChaosSettings(chaosOptionsSnapshot.Value);

    var policy = registry.Get<AsyncPolicyWrap<IResult>>("CircuitBreakerPolicy");
    var response = policy.ExecuteAsync(
        async ctx => await JsonResponseHelper.GetJsonResponseFromFile(httpContext, links, "get_customer", HttpStatusCode.OK),
        context);

    return await response;
}).WithName("get_customer");

app.MapPost("/customer", async (HttpContext httpContext, LinkGenerator links,
    IOptionsSnapshot<AppChaosSettings> chaosOptionsSnapshot, IPolicyRegistry<string> registry) =>
{
    Context context = new Context("post_customer").WithChaosSettings(chaosOptionsSnapshot.Value);

    var policy = registry.Get<AsyncPolicyWrap<IResult>>("CircuitBreakerPolicy");
    var response = policy.ExecuteAsync(
    async ctx => await JsonResponseHelper.GetJsonResponseFromFile(httpContext, links, "post_customer", HttpStatusCode.Created),
    context);

    return await response;
}).WithName("post_customer"); // Should return 201

app.MapDelete("/customer/{id}", async (HttpContext httpContext, LinkGenerator links,
    IOptionsSnapshot<AppChaosSettings> chaosOptionsSnapshot, IPolicyRegistry<string> registry) =>
{
    Context context = new Context("delete_customer").WithChaosSettings(chaosOptionsSnapshot.Value);

    var policy = registry.Get<AsyncPolicyWrap<IResult>>("CircuitBreakerPolicy");
    var response = policy.ExecuteAsync(async ctx => await Task.FromResult(Results.NoContent()), context);

    return await response;
}).WithName("delete_customer"); // Should return 204. No response body This request doesn't return a response body

app.MapPut("/customer/{id}", async (HttpContext httpContext, LinkGenerator links, string id,
    IOptionsSnapshot<AppChaosSettings> chaosOptionsSnapshot, IPolicyRegistry<string> registry) =>
{
    Context context = new Context("delete_customer").WithChaosSettings(chaosOptionsSnapshot.Value);

    var policy = registry.Get<AsyncPolicyWrap<IResult>>("CircuitBreakerPolicy");
    var response = policy.ExecuteAsync(
    async ctx =>
    {
        var body = PutCustomerById(id);
        return await JsonResponseHelper.GetJsonResponse(httpContext, links, "put_customer", HttpStatusCode.OK, body);
    },
    context);

    return await response;
}).WithName("put_customer");

app.MapGet("/customers", async (HttpContext httpContext, LinkGenerator links,
    IOptionsSnapshot<AppChaosSettings> chaosOptionsSnapshot, IPolicyRegistry<string> registry) =>
{
    Context context = new Context("get_customers").WithChaosSettings(chaosOptionsSnapshot.Value);

    var policy = registry.Get<AsyncPolicyWrap<IResult>>("CircuitBreakerPolicy");
    var response = policy.ExecuteAsync(
        async ctx => await JsonResponseHelper.GetJsonResponseFromFile(httpContext, links, "get_customers", HttpStatusCode.OK),
        context);

    return await response;
}).WithName("get_customers");

// Subscription
app.MapGet("/subscription", async (HttpContext httpContext, LinkGenerator links,
    IOptionsSnapshot<AppChaosSettings> chaosOptionsSnapshot, IPolicyRegistry<string> registry) =>
{
    Context context = new Context("get_subscription").WithChaosSettings(chaosOptionsSnapshot.Value);

    var policy = registry.Get<AsyncPolicyWrap<IResult>>("CircuitBreakerPolicy");
    var response = policy.ExecuteAsync(
        async ctx => await JsonResponseHelper.GetJsonResponseFromFile(httpContext, links, "get_subscription", HttpStatusCode.OK),
        context);

    return await response;
}).WithName("get_subscription");

app.MapPost("/subscription/{id}/reward", async (HttpContext httpContext, LinkGenerator links,
    IOptionsSnapshot<AppChaosSettings> chaosOptionsSnapshot, IPolicyRegistry<string> registry) =>
{
    Context context = new Context("post_subscription").WithChaosSettings(chaosOptionsSnapshot.Value);

    var policy = registry.Get<AsyncPolicyWrap<IResult>>("CircuitBreakerPolicy");
    var response = policy.ExecuteAsync(
        async ctx => await JsonResponseHelper.GetJsonResponseFromFile(httpContext, links, "post_subscription", HttpStatusCode.Created),
        context);

    return await response;
}).WithName("post_subscription"); // Should return 201

app.MapPut("/subscription/{id}", async (HttpContext httpContext, LinkGenerator links,
    IOptionsSnapshot<AppChaosSettings> chaosOptionsSnapshot, IPolicyRegistry<string> registry) =>
{
    Context context = new Context("get_subscription_by_id").WithChaosSettings(chaosOptionsSnapshot.Value);

    var policy = registry.Get<AsyncPolicyWrap<IResult>>("CircuitBreakerPolicy");
    var response = policy.ExecuteAsync(async ctx => await Task.FromResult(Results.NoContent()), context);

    return await response;
}).WithName("get_subscription_by_id"); // Should return 204. No response body This request doesn't return a response body

// Linked Offers API
app.MapPost("/subscription/linked-offer", async (HttpContext httpContext, LinkGenerator links,
    IOptionsSnapshot<AppChaosSettings> chaosOptionsSnapshot, IPolicyRegistry<string> registry) =>
{
    Context context = new Context("post_linked_offer").WithChaosSettings(chaosOptionsSnapshot.Value);

    var policy = registry.Get<AsyncPolicyWrap<IResult>>("CircuitBreakerPolicy");

    var response = policy.ExecuteAsync(
        ctx => JsonResponseHelper.GetJsonResponseFromFile(httpContext, links, "linked-offer", HttpStatusCode.OK),
        context);

    return await response;
}).WithName("post_linked_offer"); // Should return 204. No response body This request doesn't return a response body

app.MapPost("/subscription/linked-offer/batch", async (HttpContext httpContext, LinkGenerator links,
    IOptionsSnapshot<AppChaosSettings> chaosOptionsSnapshot, IPolicyRegistry<string> registry) =>
{
    Context context = new Context("post_linked_offer_batch").WithChaosSettings(chaosOptionsSnapshot.Value);

    var policy = registry.Get<AsyncPolicyWrap<IResult>>("CircuitBreakerPolicy");
    var response = policy.ExecuteAsync(
        async ctx =>
        {
            var body = GetNewBatchId();
            return await JsonResponseHelper.GetJsonResponse(httpContext, links, "post_linked_offer_batch", HttpStatusCode.Accepted, body);
        },
        context);

    return await response;
}).WithName("post_linked_offer_batch");

app.MapGet("/subscription/linked-offer/batch/{id}", async (HttpContext httpContext, LinkGenerator links, string id,
    IOptionsSnapshot<AppChaosSettings> chaosOptionsSnapshot, IPolicyRegistry<string> registry) =>
{
    Context context = new Context("get_linked_offer_batch").WithChaosSettings(chaosOptionsSnapshot.Value);

    var policy = registry.Get<AsyncPolicyWrap<IResult>>("CircuitBreakerPolicy");
    var response = policy.ExecuteAsync(
        async ctx =>
        {
            var body = GetBatchById(id);
            return await JsonResponseHelper.GetJsonResponse(httpContext, links, "get_linked_offer_batch", HttpStatusCode.OK, body);
        },
        context);

    return await response;
}).WithName("get_linked_offer_batch");

app.MapPost("/subscription/revoke-offer", async (HttpContext httpContext, LinkGenerator links,
    IOptionsSnapshot<AppChaosSettings> chaosOptionsSnapshot, IPolicyRegistry<string> registry) =>
{
    Context context = new Context("post_revoke_offer").WithChaosSettings(chaosOptionsSnapshot.Value);

    var policy = registry.Get<AsyncPolicyWrap<IResult>>("CircuitBreakerPolicy");
    var response = policy.ExecuteAsync(async ctx => await Task.FromResult(Results.Ok(null)), context);

    return await response;
}).WithName("post_revoke_offer");

app.Run();

static object GetCustomerById(string id)
{
    // Create result object
    dynamic result = new ExpandoObject();
    result.id = id;
    result.first_name = "Jim";
    result.last_name = "Smith";
    result.email = "jim.smith@coniq.com";
    result.phone = new Dictionary<string, int>();
    result.phone.Add("country_code", 44);
    result.phone.Add("number", 987654321);
    result.allow_email = false;
    result.allow_phone = false;
    result.allow_push_notification = false;
    result.allow_post = false;
    result.created_datetime_utc = DateTime.UtcNow;
    result.preferred_location_group = new Dictionary<string, object>();
    result.preferred_language = "EN";

    return result;
}

static object PutCustomerById(string id)
{
    // Create result object
    dynamic result = new ExpandoObject();
    result.id = id;
    result.first_name = "Jim";
    result.last_name = "Smith";
    result.email = "jim.smith@coniq.com";
    result.allow_email = false;
    result.allow_phone = false;
    result.allow_push_notification = false;
    result.allow_post = false;
    result.created_datetime_utc = DateTime.UtcNow.AddDays(-1);
    result.modified_datetime_utc = DateTime.UtcNow;
    result.preferred_location_group = new Dictionary<string, object>();

    return result;
}

static object GetNewBatchId()
{
    // Generate batch id
    dynamic result = new ExpandoObject();
    result.batch_id = Guid.NewGuid();

    return result;
}

static object GetBatchById(string id)
{
    // Generate batch id
    dynamic result = new ExpandoObject();
    result.batch_id = id;
    result.start_time = DateTime.UtcNow.AddHours(-2);
    result.completed = 2000;
    result.total = 2000;
    // Get random status value
    string[] status = { "Ongoing", "Complete", "Failed" };
    Random random = new(DateTime.UtcNow.Millisecond);
    result.status = status[random.Next(0, 2)];
    return result;
}

static object GetBarcodes(string offerId)
{
    List<ExpandoObject> result = new();
    Random random = new(DateTime.UtcNow.Millisecond);

    for (int i = 0; i < random.Next(1, 9); i++)
    {
        result.Add(CreateBarcode(offerId));
    }

    return result;
}

static ExpandoObject CreateBarcode(string offerId)
{
    // Generate batch id
    dynamic result = new ExpandoObject();
    Random random = new(DateTime.UtcNow.Millisecond);
    var fakebarcode = GuidToBigInteger(Guid.NewGuid()).ToString();

    result.barcode_number = fakebarcode;
    result.offer_id = offerId;
    result.customer_id = GuidToBigInteger(Guid.NewGuid()).ToString();
    // Get random channel value
    string[] channel = { "PAPER", "MAIL" };
    result.channel = channel[random.Next(0, 1)];
    result.image_url = $"https://barcode.valueretail.com/gensvg?type=DataMatrix&fmt=gif&qz=disable&msg={result.barcode_number}";
    result.created_datetime_utc = DateTime.UtcNow;

    return result;
}

static ExpandoObject CreateBarcodeFromGuid(ConiqCustomer body)
{
    // Generate batch id
    dynamic result = new ExpandoObject();
    var fakebarcode = GuidToBigInteger(Guid.NewGuid()).ToString().Substring(0,38);

    result.barcode_number = fakebarcode;
    result.offer_id = body.offer_id;
    result.customer_id = body.customer_id;
    result.channel = body.channel;
    // Get random channel value
    result.image_url = $"https://barcode.valueretail.com/gensvg?type=DataMatrix&fmt=gif&qz=disable&msg={result.barcode_number}";
    result.created_datetime_utc = DateTime.UtcNow;

    return result;
}

static ExpandoObject CreateBarcodeInRange(ConiqCustomer body)
{
    // Generate batch id
    dynamic result = new ExpandoObject();
    using (var rng = RandomNumberGenerator.Create())
    {
        BigInteger min = 0;
        BigInteger max = 9999999999999999999;

        result.barcode_number = RandomInRange(rng, min, max).ToString();
    }

    result.offer_id = body.offer_id;
    result.customer_id = body.customer_id;
    result.channel = body.channel;
    // Get random channel value
    result.image_url = $"https://barcode.valueretail.com/gensvg?type=DataMatrix&fmt=gif&qz=disable&msg={result.barcode_number}";
    result.created_datetime_utc = DateTime.UtcNow;

    return result;
}

static ExpandoObject CreateBarcodeBatch(string offerId)
{
    // Generate batch id
    dynamic result = new ExpandoObject();
    result.offer_id = offerId;
    Random random = new(DateTime.UtcNow.Millisecond);
    // Get random channel value
    string[] channel = { "PAPER", "MAIL" };
    result.channel = channel[random.Next(0, 1)];
    result.customers = new List<object>();

    for (int i = 0; i < random.Next(1, 9); i++)
    {
        dynamic record = new ExpandoObject();
        record.barcode_number = random.NextInt64(99999999999999);
        record.customer_id = random.NextInt64(99999999);
        result.customers.Add(record);
    }

    return result;
}

/// <summary>
/// Generate a BigInteger given a Guid. Returns a number from 0 to 2^128
/// 0 to 340,282,366,920,938,463,463,374,607,431,768,211,456
/// </summary>
static BigInteger GuidToBigInteger(Guid guid)
{
    BigInteger l_retval = 0;
    byte[] ba = guid.ToByteArray();
    int i = ba.Count();
    foreach (byte b in ba)
    {
        l_retval += b * BigInteger.Pow(256, --i);
    }
    return l_retval;
}

static BigInteger RandomInRange(RandomNumberGenerator rng, BigInteger min, BigInteger max)
{
    if (min > max)
    {
        var buff = min;
        min = max;
        max = buff;
    }

    // offset to set min = 0
    BigInteger offset = -min;
    min = 0;
    max += offset;

    var value = randomInRangeFromZeroToPositive(rng, max) - offset;
    return value;
}

static BigInteger randomInRangeFromZeroToPositive(RandomNumberGenerator rng, BigInteger max)
{
    BigInteger value;
    var bytes = max.ToByteArray();

    // count how many bits of the most significant byte are 0
    // NOTE: sign bit is always 0 because `max` must always be positive
    byte zeroBitsMask = 0b00000000;

    var mostSignificantByte = bytes[bytes.Length - 1];

    // we try to set to 0 as many bits as there are in the most significant byte, starting from the left (most significant bits first)
    // NOTE: `i` starts from 7 because the sign bit is always 0
    for (var i = 7; i >= 0; i--)
    {
        // we keep iterating until we find the most significant non-0 bit
        if ((mostSignificantByte & (0b1 << i)) != 0)
        {
            var zeroBits = 7 - i;
            zeroBitsMask = (byte)(0b11111111 >> zeroBits);
            break;
        }
    }

    do
    {
        rng.GetBytes(bytes);

        // set most significant bits to 0 (because `value > max` if any of these bits is 1)
        bytes[bytes.Length - 1] &= zeroBitsMask;

        value = new BigInteger(bytes);

        // `value > max` 50% of the times, in which case the fastest way to keep the distribution uniform is to try again
    } while (value > max);

    return value;
}