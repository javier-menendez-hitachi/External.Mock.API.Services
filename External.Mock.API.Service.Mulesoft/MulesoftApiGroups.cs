namespace External.Mock.API.Service.Mulesoft
{
    using Microsoft.Extensions.Options;
    using System.Net;
    using Polly.Registry;
    using External.Mock.API.Service.Mulesoft.Faker;
    using External.Mock.API.Service.Mulesoft.Callback;
    using External.Mock.API.Service.Mulesoft.Model.Wape;
    using Polly;
    using Polly.Wrap;
    using External.Mock.API.Service.Mulesoft.Model.Response;
    using global::Mock.API.Services.Common.Chaos;
    using global::Mock.API.Services.Common.AspNetCoreHelpers;

    public static partial class MulesoftApiGroups
    {
        public static RouteGroupBuilder Root(this RouteGroupBuilder group)
        {
            // Hello
            group.MapGet("", async (HttpContext httpContext, LinkGenerator links,
                IOptionsSnapshot<AppChaosSettings> chaosOptionsSnapshot, IPolicyRegistry<string> registry) =>
            {
                var response = new MockResponse();

                return await ChaosHandler("Hello", "CircuitBreakerPolicy",httpContext, links, chaosOptionsSnapshot, registry, HttpStatusCode.OK, response);

            }).WithName("Hello");

            // OAuth
            group.MapPost("{id}/oauth2/v2.0/token", async (HttpContext httpContext, LinkGenerator links,
                IOptionsSnapshot<AppChaosSettings> chaosOptionsSnapshot, IPolicyRegistry<string> registry,
                string id) =>
            {
                var response = new OAuthResponse();

                return await ChaosHandler("Hello", "CircuitBreakerPolicy", httpContext, links, chaosOptionsSnapshot, registry, HttpStatusCode.OK, response);

            }).WithName("OAuth");

            return group;
        }

        public static RouteGroupBuilder Wape(this RouteGroupBuilder group)
        {
            // Health Check
            group.MapGet("/health-check", async (HttpContext httpContext, LinkGenerator links,
                IOptionsSnapshot<AppChaosSettings> chaosOptionsSnapshot, IPolicyRegistry<string> registry) =>
            {
                var response = new HealthResponse();

                return await ChaosHandler("WapeHealthCheck", "CircuitBreakerPolicy", httpContext, links, chaosOptionsSnapshot, registry, HttpStatusCode.OK, response);

            }).WithName("WapeHealthCheck");

            // Create Guest
            group.MapPost("/guests/", async (HttpContext httpContext, LinkGenerator links,
                    IOptionsSnapshot<AppChaosSettings> chaosOptionsSnapshot,
                    IOptionsSnapshot<WapeCallbackSettings> wapeCallbackOptionsSnapshot,
                    IPolicyRegistry<string> registry, Guest? body) =>
            {
                var errResponse = new MockResponse
                {
                    IsSuccess = false
                };

                // Check Request
                if (body == null)
                {
                    errResponse.ErrorCode = "1";
                    errResponse.ErrorMessage = "Missing Body";
                    errResponse.StatusCode = 400;

                    return await JsonResponseHelper.GetJsonResponse(httpContext, links, "RegisterGuest_MissingBody", HttpStatusCode.BadRequest, errResponse);
                }
                    
                // Retrieve header with environment name
                var environment = httpContext.Request.Headers["environment"].ToString();
                if (string.IsNullOrEmpty(environment))
                {
                    errResponse.ErrorCode = "2";
                    errResponse.ErrorMessage = "Missing Environment header";
                    errResponse.StatusCode = 400;

                    return await JsonResponseHelper.GetJsonResponse(httpContext, links, "RegisterGuest_MissingEnvironment", HttpStatusCode.BadRequest, errResponse);
                }
                    
                // Retrieve callback url and key
                var callback = wapeCallbackOptionsSnapshot.Value.WapeCallbacks?.Find(x => x.Environment!.Equals(environment, StringComparison.OrdinalIgnoreCase));
                if (callback == null) 
                {
                    errResponse.ErrorCode = "3";
                    errResponse.ErrorMessage = "Missing Update Barcode callback settings";
                    errResponse.Message = "No update barcode callback url and key found for provided environment";
                    errResponse.StatusCode = 424;

                    return await JsonResponseHelper.GetJsonResponse(httpContext, links, "RegisterGuest_MissingCallback", HttpStatusCode.FailedDependency, errResponse);
                }

                // Update Response
                var response = FakeHelper.GenFakeGuest(Guid.NewGuid(), body.Email);

                // Update Barcode Callback (This call is not awaited because we want to simulate the barcode providing process)
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                Task.Run(() => UpdateBarcodeCallbackDelayed(response.Id, callback!))
                   .ContinueWith(t =>
                   {
                       if (t.IsFaulted)
                       {
                           // Handle the exception
                           var exception = t.Exception;
                           // Log the exception or take other actions
                       }
                   }, TaskScheduler.Default);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                return await ChaosHandler("RegisterGuest", "CircuitBreakerPolicy", httpContext, links, chaosOptionsSnapshot, registry, HttpStatusCode.OK, response);

            }).WithName("RegisterGuest");

            // Get Guest
            group.MapGet("/guests/{customerId}", async (HttpContext httpContext, LinkGenerator links, string customerId,
                    IOptionsSnapshot<AppChaosSettings> chaosOptionsSnapshot, IPolicyRegistry<string> registry) =>
            {
                // Check Request
                if (!Guid.TryParse(customerId, out Guid guid))
                    return await JsonResponseHelper.GetJsonResponse(httpContext, links, "GetGuest", HttpStatusCode.BadRequest, null!);

                var response = FakeHelper.GenFakeGuest(guid);

                return await ChaosHandler("GetGuest", "CircuitBreakerPolicy", httpContext, links, chaosOptionsSnapshot, registry, HttpStatusCode.OK, response);

            }).WithName("GetGuest");

            //PATCH / Update guest
            group.MapPatch("/guests/{customerId}", async (HttpContext httpContext, LinkGenerator links, string customerId,
                    IOptionsSnapshot<AppChaosSettings> chaosOptionsSnapshot, IPolicyRegistry<string> registry, Guest? body) =>
            {
                // Check Request
                if (body == null)
                    return await JsonResponseHelper.GetJsonResponse(httpContext, links, "UpdateGuest", HttpStatusCode.BadRequest, null!);

                var response = new MuleResponseTrans();

                return await ChaosHandler("UpdateGuest", "CircuitBreakerPolicy", httpContext, links, chaosOptionsSnapshot, registry, HttpStatusCode.OK, response);

            }).WithName("UpdateGuest");

            // Update guest Last Login Date
            group.MapPost("/guests/{customerId}/login", async (HttpContext httpContext, LinkGenerator links, string customerId,
                    IOptionsSnapshot<AppChaosSettings> chaosOptionsSnapshot, IPolicyRegistry<string> registry, GuestLogin? body) =>
            {

                // Check Request
                if (!Guid.TryParse(customerId, out Guid guid))
                    return await JsonResponseHelper.GetJsonResponse(httpContext, links, "UpdateGuestLastLoginDate", HttpStatusCode.BadRequest, null!);

                if (body == null)
                    return await JsonResponseHelper.GetJsonResponse(httpContext, links, "UpdateGuestLastLoginDate", HttpStatusCode.BadRequest, null!);

                var response = FakeHelper.GenFakeGuest(guid);

                return await ChaosHandler("UpdateGuestLastLoginDate", "CircuitBreakerPolicy", httpContext, links, chaosOptionsSnapshot, registry, HttpStatusCode.OK, response);

            }).WithName("UpdateGuestLastLoginDate");

            // Send Email V2
            group.MapPost("/guests/{customerId}/emails/{templateId}", async (HttpContext httpContext, LinkGenerator links,
                        IOptionsSnapshot<AppChaosSettings> chaosOptionsSnapshot, IPolicyRegistry<string> registry,
                        string customerId, string templateId, SendEmail? body) =>
            {
                // Check Request
                if (body == null)
                    return await JsonResponseHelper.GetJsonResponse(httpContext, links, "SendEmail", HttpStatusCode.BadRequest, null!);

                // Update Response
                var response = new MuleResponseCorrelation();

                return await ChaosHandler("SendEmail", "CircuitBreakerPolicy", httpContext, links, chaosOptionsSnapshot, registry, HttpStatusCode.OK, response);

            }).WithName("SendEmail");

            group.MapPut("/guests/rewards", async (HttpContext httpContext, LinkGenerator links, IOptionsSnapshot<AppChaosSettings> chaosOptionsSnapshot,
                IPolicyRegistry<string> registry, List<Reward>? body) =>
            {
                // Check Request
                if (body == null)
                    return await JsonResponseHelper.GetJsonResponse(httpContext, links, "GuestRewards", HttpStatusCode.BadRequest, null!);

                var response = new MuleResponseCorrelation();

                return await ChaosHandler("GuestRewards", "CircuitBreakerPolicy", httpContext, links, chaosOptionsSnapshot, registry, HttpStatusCode.Accepted, response);

            }).WithName("GuestRewards");

            group.MapPut("/guests/segments", async (HttpContext httpContext, LinkGenerator links, IOptionsSnapshot<AppChaosSettings> chaosOptionsSnapshot,
               IPolicyRegistry<string> registry, List<Segment>? body) =>
            {
                // Check Request
                if (body == null)
                    return await JsonResponseHelper.GetJsonResponse(httpContext, links, "GuestSegments", HttpStatusCode.BadRequest, null!);

                var response = new MuleResponseCorrelation();

                return await ChaosHandler("GuestSegments", "CircuitBreakerPolicy", httpContext, links, chaosOptionsSnapshot, registry, HttpStatusCode.Accepted, response);

            }).WithName("GuestSegments");

            return group;
        }

        public static RouteGroupBuilder WebsiteMembersLoyalty(this RouteGroupBuilder group)
        {
            // Health Check
            group.MapGet("/health-check", async (HttpContext httpContext, LinkGenerator links,
                IOptionsSnapshot<AppChaosSettings> chaosOptionsSnapshot, IPolicyRegistry<string> registry) =>
            {
                var response = new HealthResponse
                {
                    ApiName = "vr-exp-website-members-loyalty-v1-mock",
                    ApiVersion = "1.0.0",
                };

                return await ChaosHandler("WebsiteMembersLoyaltyHealthCheck", "CircuitBreakerPolicy", httpContext, links, chaosOptionsSnapshot, registry, HttpStatusCode.OK, response);

            }).WithName("WebsiteMembersLoyaltyHealthCheck");

            // GET Member info (points)
            group.MapGet("/members/{customerId}", async (HttpContext httpContext, LinkGenerator links,
                IOptionsSnapshot<AppChaosSettings> chaosOptionsSnapshot, IPolicyRegistry<string> registry,
                string customerId) =>
            {
                var response = FakeHelper.GenFakeGuestInfo();

                return await ChaosHandler("GetMemberInfoPoints", "CircuitBreakerPolicy", httpContext, links, chaosOptionsSnapshot, registry, HttpStatusCode.OK, response);

            }).WithName("GetMemberInfoPoints");

            // GET Member Rewards
            group.MapGet("/members/{customerId}/rewards", async (HttpContext httpContext, LinkGenerator links,
                IOptionsSnapshot<AppChaosSettings> chaosOptionsSnapshot, IPolicyRegistry<string> registry,
                string customerId) =>
            {
                var response = FakeHelper.GenFakeRewardsInfo();

                return await ChaosHandler("GetMemberRewards", "CircuitBreakerPolicy", httpContext, links, chaosOptionsSnapshot, registry, HttpStatusCode.OK, response);

            }).WithName("GetMemberRewards");

            // GET Member Treat Categories
            group.MapGet("/members/{customerId}/rewards/categories", async (HttpContext httpContext, LinkGenerator links,
                IOptionsSnapshot<AppChaosSettings> chaosOptionsSnapshot, IPolicyRegistry<string> registry,
                string customerId) =>
            {
                var response = FakeHelper.GenFakeCustomerTreatCategories();

                return await ChaosHandler("GetMemberTreatCategories", "CircuitBreakerPolicy", httpContext, links, chaosOptionsSnapshot, registry, HttpStatusCode.OK, response);

            }).WithName("GetMemberTreatCategories");

            return group;
        }

        public static RouteGroupBuilder WebsiteLoyalty(this RouteGroupBuilder group)
        {
            // Health Check
            group.MapGet("/health-check", async (HttpContext httpContext, LinkGenerator links,
                IOptionsSnapshot<AppChaosSettings> chaosOptionsSnapshot, IPolicyRegistry<string> registry) =>
            {
                var response = new HealthResponse
                {
                    ApiName = "vr-exp-website-loyalty-v1-mock",
                    ApiVersion = "1.0.0",
                };

                return await ChaosHandler("WebsiteLoyaltyHealthCheck", "CircuitBreakerPolicy", httpContext, links, chaosOptionsSnapshot, registry, HttpStatusCode.OK, response);

            }).WithName("WebsiteLoyaltyHealthCheck");

            // GET Segment Info
            group.MapGet("/segment/{segmentId}", async (HttpContext httpContext, LinkGenerator links,
                IOptionsSnapshot<AppChaosSettings> chaosOptionsSnapshot, IPolicyRegistry<string> registry,
                string segmentId) =>
            {
                var response = FakeHelper.GenFakeSegmentInfo();

                return await ChaosHandler("GetSegmentInfo", "CircuitBreakerPolicy", httpContext, links, chaosOptionsSnapshot, registry, HttpStatusCode.OK, response);
            }).WithName("GetSegmentInfo");
            return group;
        }

        private static async Task UpdateBarcodeCallbackDelayed(Guid guid, WapeCallback wapeCallbackOptions)
        {
            var maxDelay = int.Parse(wapeCallbackOptions.MaxDelayMs!);
            await Task.Delay(new Random(Guid.NewGuid().GetHashCode()).Next(4000, maxDelay));
            UpdateBarcodeCallback(guid, wapeCallbackOptions);
        }

        private static void UpdateBarcodeCallback(Guid guid, WapeCallback wapeCallbackOptions)
        {
            var barcodeNumber = FakeHelper.GuidToBigInteger(Guid.NewGuid()).ToString();
            var body =
                $"{{\r\n\r\n    \"guid\": \"{guid}\",\r\n\r\n    \"barcodeNumber\":\"{barcodeNumber}\",\r\n\r\n    \"barcodeUrl\":\"https://barcode.valueretail.com/gensvg?type=DataMatrix&fmt=gif&qz=disable&msg={barcodeNumber}\",\r\n\r\n    \"barcodeOfferId\":\"17453\"\r\n\r\n}}";
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, wapeCallbackOptions?.Url);
            var content = new StringContent(body, null, "application/json");
            request.Content = content;
            request.Headers.Add("ContentType", "application/json");
            request.Headers.Add("Ocp-Apim-Subscription-Key", wapeCallbackOptions?.OcpApimSubscriptionKey);
            _ = client.SendAsync(request);
        }


        static async Task<IResult> ChaosHandler(string operationKey, string policyKey, HttpContext httpContext,
            LinkGenerator links, IOptionsSnapshot<AppChaosSettings> chaosOptionsSnapshot, IPolicyRegistry<string> registry,
            HttpStatusCode httpStatusCode,
            object body = null!, bool responseFromFile = false)
        {
            Task<IResult> response;

            // Create chaos context for this request
            Context context = new Context(operationKey).WithChaosSettings(chaosOptionsSnapshot.Value);

            // Create CircuitBreaker Policy to apply chaos if necessary
            var policy = registry.Get<AsyncPolicyWrap<IResult>>(policyKey);

            // Execute the policy
            response = responseFromFile ?
                policy.ExecuteAsync(ctx => JsonResponseHelper.GetJsonResponseFromFile(httpContext, links, operationKey, httpStatusCode), context)
                : policy.ExecuteAsync(ctx => JsonResponseHelper.GetJsonResponse(httpContext, links, operationKey, httpStatusCode, body), context);

            // Return response
            return await response;
        }

    }
}
