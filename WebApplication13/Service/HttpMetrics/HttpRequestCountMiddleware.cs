using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace Prometheus.HttpMetrics
{
    internal sealed class HttpRequestCountMiddleware : HttpRequestMiddlewareBase<ICollector<ICounter>, ICounter>
    {
        private readonly RequestDelegate _next;

        public HttpRequestCountMiddleware(RequestDelegate next, HttpRequestCountOptions options)
            : base(options, options?.Counter)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            finally
            {
                CreateChild(context).Inc();
            }
        }

        protected override string[] BaselineLabels => HttpRequestLabelNames.Default;

        protected override ICollector<ICounter> CreateMetricInstance(string[] labelNames) => MetricFactory.CreateCounter(
            "http_requests_received_total",
            "Provides the count of HTTP requests that have been processed by the ASP.NET Core pipeline.",
            new CounterConfiguration
            {
                LabelNames = labelNames
            });
    }
}