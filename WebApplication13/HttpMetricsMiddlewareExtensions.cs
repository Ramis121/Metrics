using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Prometheus.HttpMetrics;
using System;

namespace Prometheus
{
    public static class HttpMetricsMiddlewareExtensions
    {
        public static IApplicationBuilder UseHttpMetrics(this IApplicationBuilder app,
            Action<HttpMiddlewareExporterOptions> configure)
        {
            var options = new HttpMiddlewareExporterOptions();

            configure?.Invoke(options);

            app.UseHttpMetrics(options);

            return app;
        }

        public static IApplicationBuilder UseHttpMetrics(this IApplicationBuilder app,
            HttpMiddlewareExporterOptions? options = null)
        {
            options = options ?? new HttpMiddlewareExporterOptions();

            if (app.ApplicationServices.GetService<PageLoader>() != null)
            {
                options.InProgress.IncludePageLabelInDefaultsInternal = true;
                options.RequestCount.IncludePageLabelInDefaultsInternal = true;
                options.RequestDuration.IncludePageLabelInDefaultsInternal = true;
            }

            void ApplyConfiguration(IApplicationBuilder builder)
            {
                builder.UseMiddleware<CaptureRouteDataMiddleware>();

                if (options.InProgress.Enabled)
                    builder.UseMiddleware<HttpInProgressMiddleware>(options.InProgress);
                if (options.RequestCount.Enabled)
                    builder.UseMiddleware<HttpRequestCountMiddleware>(options.RequestCount);
                if (options.RequestDuration.Enabled)
                    builder.UseMiddleware<HttpRequestDurationMiddleware>(options.RequestDuration);
            }

            if (options.CaptureMetricsUrl)
                ApplyConfiguration(app);
            else
                app.UseWhen(context => context.Request.Path != "/metrics", ApplyConfiguration);

            return app;
        }
    }
}