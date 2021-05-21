using Localtunnel;
using Localtunnel.Connections;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Threading;

namespace WinTenDev.Zizi.Utils.Extensions
{
    public static class TunnelingServiceExtension
    {
        public static IServiceCollection AddLocalTunnelClient(this IServiceCollection services)
        {
            services.AddScoped(service =>
            {
                var clientTunnel = new LocaltunnelClient();
                return clientTunnel;
            });
            return services;
        }

        public static IApplicationBuilder UseLocalTunnel(this IApplicationBuilder app, string subdomain)
        {
            // var clientTunnel = new LocaltunnelClient();
            var services = app.GetServiceProvider();
            var tunnelClient = services.GetRequiredService<LocaltunnelClient>();

            var tunnel = tunnelClient.OpenAsync(handle =>
            {
                var options = new ProxiedHttpTunnelOptions()
                {
                    Host = "localhost",
                    Port = 5010,
                    ReceiveBufferSize = 10
                };
                return new ProxiedHttpTunnelConnection(handle, options);
            }, subdomain: subdomain, CancellationToken.None).Result;
            tunnel.StartAsync();

            var tunnelUrl = tunnel.Information.Url;
            Log.Information($"Tunnel URL: {tunnelUrl}");
            Log.Information($"Tunnel URL: {tunnel.Information.Id}");

            return app;
        }
    }
}