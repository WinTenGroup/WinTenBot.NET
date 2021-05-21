using EasyCaching.Core;
using EasyCaching.Disk;
using EasyCaching.SQLite;
using Microsoft.AspNetCore.Builder;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;

namespace WinTenDev.Zizi.Utils.Extensions
{
    public static class EasyCachingServiceExtension
    {
        public static IServiceCollection AddEasyCachingSqlite(this IServiceCollection services)
        {
            services.AddEasyCaching(options =>
            {
                options.UseSQLite(sqLiteOptions =>
                {
                    sqLiteOptions.EnableLogging = true;
                    sqLiteOptions.DBConfig = new SQLiteDBOptions()
                    {
                        CacheMode = SqliteCacheMode.Shared,
                        FilePath = "Storage/EasyCaching/",
                        FileName = "LocalCache.db",
                        OpenMode = SqliteOpenMode.ReadWriteCreate,
                    };
                });
            });

            return services;
        }

        public static IServiceCollection AddEasyCachingDisk(this IServiceCollection services)
        {
            services.AddEasyCaching(options =>
            {
                options.UseDisk(diskOptions =>
                {
                    diskOptions.EnableLogging = true;
                    diskOptions.DBConfig = new DiskDbOptions()
                    {
                        BasePath = "Storage/EasyCaching/Disk/",
                    };
                });
            });

            return services;
        }

        public static IApplicationBuilder UseEasyCaching(this IApplicationBuilder app)
        {
            var services = app.ApplicationServices;
            var cachingProvider = services.GetRequiredService<IEasyCachingProvider>();


            return app;
        }
    }
}