using Microsoft.Extensions.DependencyInjection;

namespace eCommerceAutomation.Framework
{
    public static class Runtime
    {
        public static void Build(IServiceCollection services)
        {
            services.AddSingleton<CommonHelper>();
        }
    }
}
