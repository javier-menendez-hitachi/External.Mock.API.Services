namespace Mock.API.Services.Common.AspNetCoreHelpers
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Routing;
    using System.Net;
    using System.Text.Json;

    public static class JsonResponseHelper
    {
        public static async Task<IResult> GetJsonResponseFromFile(HttpContext httpContext, LinkGenerator links, string endpointName, HttpStatusCode statusCode)
        {
            // Get Response body from file
            var body = GetJsonFromFile(endpointName);

            return await GetJsonResponse(httpContext, links, endpointName, statusCode, body);
        }

        public static Task<IResult> GetJsonResponse(HttpContext httpContext, LinkGenerator links, string endpointName, HttpStatusCode statusCode, object body)
        {
            // Get URI
            var uri = links.GetUriByName(httpContext, endpointName, null) ?? links.GetUriByName(httpContext, "hello", null);
            IResult result = statusCode switch
            {
                HttpStatusCode.Created => Results.Created(uri!, body),
                HttpStatusCode.Accepted => Results.Accepted(uri!, body),
                HttpStatusCode.BadRequest => Results.BadRequest(body),
                HttpStatusCode.InternalServerError => Results.StatusCode(500),
                _ => Results.Json(body)
            };
            return Task.FromResult(result);
        }

        private static object GetJsonFromFile(string endpointName)
        {
            // Get file
            string jsonString = File.ReadAllText($"./_files/{endpointName}.json");
            return JsonSerializer.Deserialize<object>(jsonString)!;
        }
    }
}