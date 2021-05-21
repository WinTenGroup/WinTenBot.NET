using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace WinTenDev.Zizi.Utils
{
    public static class EfUtil
    {
        public static void EnsureMigrationOfContext<T>(this IApplicationBuilder app) where T : DbContext
        {
            var context = app.GetServiceProvider().GetRequiredService<T>();
            var isCreated = context.Database.EnsureCreated();
            Log.Information($"Is {nameof(context)} creatd? {isCreated}");
        }
    }
}