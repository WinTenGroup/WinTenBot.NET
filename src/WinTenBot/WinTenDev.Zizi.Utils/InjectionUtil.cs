using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace WinTenDev.Zizi.Utils
{
    public static class InjectionUtil
    {
        public static IServiceProvider GetServiceProvider(this IApplicationBuilder app)
        {
            var serviceScope = app.ApplicationServices.CreateScope();
            return serviceScope.ServiceProvider;
        }
    }
}