using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace Mock.API.Service.WAPE.Extensions
{
    public static class BindingExtensions
    {
        public static async Task<T?> BindFromForm<T>(this HttpContext httpContext)
        {
            var serviceProvider = httpContext.RequestServices;
            var factory = serviceProvider.GetRequiredService<IModelBinderFactory>();
            var metadataProvider = serviceProvider.GetRequiredService<IModelMetadataProvider>();

            var metadata = metadataProvider.GetMetadataForType(typeof(T));
            var modelBinder = factory.CreateBinder(new()
            {
                Metadata = metadata
            });

            var context = new DefaultModelBindingContext
            {
                ModelMetadata = metadata,
                ModelName = string.Empty,
                ValueProvider = new FormValueProvider(
                    BindingSource.Form,
                    httpContext.Request.Form,
                    CultureInfo.InvariantCulture
                ),
                ActionContext = new ActionContext(
                    httpContext,
                    new RouteData(),
                    new ActionDescriptor()),
                ModelState = new ModelStateDictionary()
            };
            await modelBinder.BindModelAsync(context);
            return (T?)context.Result.Model;
        }
    }
}
