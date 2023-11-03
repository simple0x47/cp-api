using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Cuplan;

public class DevOnly : Attribute, IFilterFactory
{
    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
    {
        return new DevOnlyAttributeImpl(serviceProvider.GetService<IWebHostEnvironment>());
    }

    public bool IsReusable { get; } = true;

    private class DevOnlyAttributeImpl : Attribute, IAuthorizationFilter
    {
        private const string DevelopmentEnvironment = "Development";
        private const string ActionsEnvironment = "Actions";

        private readonly IWebHostEnvironment _environment;

        public DevOnlyAttributeImpl(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (!_environment.IsEnvironment(DevelopmentEnvironment) && !_environment.IsEnvironment(ActionsEnvironment))
                context.Result = new NotFoundResult();
        }
    }
}