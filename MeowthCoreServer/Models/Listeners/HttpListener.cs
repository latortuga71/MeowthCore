using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


namespace MeowthCoreServer.Models
{
    public class HttpListener : Listener
    {
        public override string Name { get; }
        public int BindPort { get; }
        private CancellationTokenSource _tokenSource;
        public HttpListener(string name, int bindPort)
        {
            Name = name;
            BindPort = bindPort;
        }

        public override async Task Start()
        {
            var hostBuilder = new HostBuilder()
                .ConfigureWebHostDefaults(host =>
                {
                    host.UseUrls($"https://0.0.0.0:{BindPort}");
                    host.Configure(ConfigureApp);
                    host.ConfigureServices(ConfigureServices);
                });
            var host = hostBuilder.Build();
            _tokenSource = new CancellationTokenSource();
            host.RunAsync(_tokenSource.Token);
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSingleton(AgentService);
        }

        private void ConfigureApp(IApplicationBuilder app)
        {
            app.UseRouting();
            //app.UseHttpsRedirection();
            app.UseEndpoints(e =>
            {
                e.MapControllerRoute("/", "/", new { controller = "HttpListener", action = "HandleImplant" });
            });
        }

        public override void Stop()
        {
            _tokenSource.Cancel();
        }
    }
}
