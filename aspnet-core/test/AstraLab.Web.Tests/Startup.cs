using System;
using Abp.AspNetCore;
using Abp.AspNetCore.Mvc.Antiforgery;
using Abp.AspNetCore.SignalR.Hubs;
using Abp.AspNetCore.TestBase;
using Abp.Dependency;
using AstraLab.Configuration;
using AstraLab.EntityFrameworkCore;
using AstraLab.Identity;
using AstraLab.Web.Host.Startup;
using Castle.MicroKernel.Registration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AstraLab.Web.Tests
{
    public class Startup
    {
        private readonly IConfigurationRoot _appConfiguration;

        public Startup(IWebHostEnvironment env)
        {
            _appConfiguration = env.GetAppConfiguration();
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddEntityFrameworkInMemoryDatabase();

            services.AddControllersWithViews(options =>
            {
                options.Filters.Add(new AbpAutoValidateAntiforgeryTokenAttribute());
            });

            IdentityRegistrar.Register(services);
            AuthConfigurer.Configure(services, _appConfiguration);
            services.AddSignalR();
            services.AddAstraLabHostAntiforgery();
            services.AddAstraLabHostCors(_appConfiguration);

            return services.AddAbp<AstraLabWebTestModule>(options =>
            {
                options.SetupTest();
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            UseInMemoryDb(app.ApplicationServices);

            app.UseAbp(options => { options.UseAbpRequestLocalization = false; });
            app.UseStaticFiles();
            app.UseRouting();
            app.UseCors(AstraLabHostHttpSecurity.DefaultCorsPolicyName);
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseAbpRequestLocalization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<AbpCommonHub>("/signalr");
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapControllerRoute("defaultWithArea", "{area}/{controller=Home}/{action=Index}/{id?}");
            });
        }

        private void UseInMemoryDb(IServiceProvider serviceProvider)
        {
            var builder = new DbContextOptionsBuilder<AstraLabDbContext>();
            builder.UseInMemoryDatabase(Guid.NewGuid().ToString()).UseInternalServiceProvider(serviceProvider);
            var options = builder.Options;

            var iocManager = serviceProvider.GetRequiredService<IIocManager>();
            iocManager.IocContainer.Register(
                Component.For<DbContextOptions<AstraLabDbContext>>()
                    .Instance(options)
                    .LifestyleSingleton());
        }
    }
}
