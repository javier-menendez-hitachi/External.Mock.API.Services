using Microsoft.AspNetCore.Http.HttpResults;
using Mock.API.Service.WAPE.Extensions;
using System.Reflection;

namespace Mock.API.Service.WAPE.Model
{

    public class SignInRequest
    {
        public string? client_id { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? grant_type { get; set; }
        public string? ReferenceType { get; set; }

        public static async ValueTask<SignInRequest?> BindAsync(HttpContext httpContext, ParameterInfo parameter)
        {
            return await httpContext.BindFromForm<SignInRequest>();
        }
    }
}
