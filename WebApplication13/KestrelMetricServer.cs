using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Prometheus;
using System;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace Prometheus
{
    public sealed class KestrelMetricServer : MetricHandler
    {
        public KestrelMetricServer(int port, string url = "/metrics", CollectorRegistry? registry = null, X509Certificate2? certificate = null) : this("+", port, url, registry, certificate)
        {
        }

        public KestrelMetricServer(string hostname, int port, string url = "/metrics", CollectorRegistry? registry = null, X509Certificate2? certificate = null) : base(registry)
        {
            _hostname = hostname;
            _port = port;
            _url = url;

            _certificate = certificate;
        }

        private readonly string _hostname;
        private readonly int _port;
        private readonly string _url;

        private readonly X509Certificate2? _certificate;

        protected override Task StartServer(CancellationToken cancel)
        {
            var s = _certificate != null ? "s" : "";
            var hostAddress = $"http{s}://{_hostname}:{_port}";

            var builder = new WebHostBuilder()
                .UseKestrel()
                .UseIISIntegration();
                

            if (_certificate != null)
            {
                builder = builder.ConfigureServices(services =>
                    {
                        Action<ListenOptions> configureEndpoint = options =>
                        {
                            options.UseHttps(_certificate);
                        };

                        services.Configure<KestrelServerOptions>(options =>
                        {
                            options.Listen(IPAddress.Any, _port, configureEndpoint);
                        });
                    });
            }
            else
            {
                builder = builder.UseUrls(hostAddress);
            }

            var webHost = builder.Build();
            webHost.Start();

            return webHost.WaitForShutdownAsync(cancel);
        }
    }
}
