using System;
using System.Linq;
using Abp.Extensions;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AstraLab.Web.Host.Startup
{
    public static class AstraLabHostHttpSecurity
    {
        public const string DefaultCorsPolicyName = "Frontend";
        public const string AntiForgeryCookieName = "XSRF-TOKEN";
        public const string AntiForgeryHeaderName = "X-XSRF-TOKEN";

        public static readonly string[] AllowedCorsHeaders =
        {
            "Authorization",
            "Content-Type",
            AntiForgeryHeaderName,
            "Abp.TenantId"
        };

        public static readonly string[] AllowedCorsMethods =
        {
            "GET",
            "POST",
            "PUT",
            "PATCH",
            "DELETE",
            "OPTIONS"
        };

        public static IServiceCollection AddAstraLabHostCors(this IServiceCollection services, IConfiguration configuration)
        {
            var origins = GetConfiguredCorsOrigins(configuration);

            services.AddCors(options =>
            {
                options.AddPolicy(DefaultCorsPolicyName, builder =>
                {
                    if (origins.Length > 0)
                    {
                        builder.WithOrigins(origins);
                    }
                    else
                    {
                        builder.SetIsOriginAllowed(_ => false);
                    }

                    builder
                        .WithHeaders(AllowedCorsHeaders)
                        .WithMethods(AllowedCorsMethods)
                        .WithExposedHeaders(AntiForgeryHeaderName)
                        .AllowCredentials();
                });
            });

            return services;
        }

        public static IServiceCollection AddAstraLabHostAntiforgery(this IServiceCollection services)
        {
            services.AddAntiforgery(options =>
            {
                options.HeaderName = AntiForgeryHeaderName;
                options.Cookie.Name = AntiForgeryCookieName;
                options.Cookie.HttpOnly = false;
                options.Cookie.IsEssential = true;
                options.Cookie.SameSite = SameSiteMode.None;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            });

            services.Configure<CookiePolicyOptions>(options =>
            {
                options.MinimumSameSitePolicy = SameSiteMode.Unspecified;
                options.HttpOnly = HttpOnlyPolicy.None;
                options.Secure = CookieSecurePolicy.Always;
            });

            return services;
        }

        public static string[] GetConfiguredCorsOrigins(IConfiguration configuration)
        {
            return (configuration["App:CorsOrigins"] ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(origin => origin.Trim().RemovePostFix("/"))
                .Where(origin => !origin.IsNullOrWhiteSpace())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }
    }
}
