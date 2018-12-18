using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Funq;
using SSApi.ServiceInterface;
using SSApi.Dal.Vehicle;
using SSApi.EFCore.Vehicle;
using SSApi.EFCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.IO;
using System.IdentityModel.Tokens.Jwt;
using System.Collections.Generic;
using System;
using ServiceStack;
using ServiceStack.Configuration;
using ServiceStack.Auth;
using ServiceStack.Authentication.IdentityServer;
using ServiceStack.Authentication.IdentityServer.Enums;

namespace SSApi
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public Startup(IConfiguration configuration) => Configuration = configuration;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddMvc();

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            services.AddMvcCore()
                .AddAuthorization()
                .AddJsonFormatters();

            services.AddDbContext<DataContext>(x => x
                .UseSqlServer(Configuration.GetConnectionString("DefaultConnection"))
                .ConfigureWarnings(warnings => warnings.Ignore(CoreEventId.IncludeIgnoredWarning))
            );

            //repository DI
            services.AddScoped<IVehicleRepository, VehicleRepository>();

            services.AddAuthentication("Bearer")
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = "http://127.0.0.1:65048";   //points to authentication token service.
                    options.RequireHttpsMetadata = false;

                    options.ApiName = "apiMB";                      //name of "this" application and api resources we want to authorise for.
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseAuthentication();

            // if (env.IsDevelopment())
            // {
            //     app.UseDeveloperExceptionPage();
            // }
            // else
            // {
            //     app.UseExceptionHandler("/Home/Error");
            // }

            //seed the database
            //seeder.SeedStaticDataAsync();

            //app.UseStaticFiles();

            //IdentityServer will redirect too upon successful login.
            app.UseServiceStack(new AppHost("http://127.0.0.1:6000")
            {
                AppSettings = new NetCoreAppSettings(Configuration)
            });

            app.UseMvc();

            // app.UseMvc(routes =>
            // {
            //     routes.MapRoute(
            //         name: "default",
            //         template: "{controller=Home}/{action=Index}/{id?}");
            // });
        }
    }


    /// <summary>
    /// ServiceStack setup.
    /// </summary>
    public class AppHost : AppHostBase
    {
        private readonly string serviceUrl;
        public AppHost(string serviceUrl) : base("SSApi", typeof(MyServices).Assembly) { this.serviceUrl = serviceUrl; }

        // Configure your AppHost with the necessary configuration and dependencies your App needs
        public override void Configure(Container container)
        {
            var appSettings = new AppSettings(); //Access Web.Config AppSettings
            var isDebug = appSettings.Get(nameof(HostConfig.DebugMode), false);
            SetConfig(new HostConfig
            {
                // the url:port that IdentityServer will know to redirect to upon succesful login
                WebHostUrl = serviceUrl,
                DebugMode = isDebug
            });

            // WHAT PLUGIN CONFIG DO WE NEED TO GET SERVICESTACK TO PLAY WITH IDENTITYSERVER????????
            Plugins.Add(new AuthFeature(() => new AuthUserSession(),
                new IAuthProvider[] {
                    new BasicAuthProvider(), //Sign-in with HTTP Basic Auth
                    new CredentialsAuthProvider(), //HTML Form post of UserName/Password credentials
                    new JwtAuthProvider(appSettings)
              }));

            // TRYING THIS COMMUNITY PLUGIN
            // see https://github.com/wwwlicious/servicestack-authentication-identityserver
            this.Plugins.Add(new IdentityServerAuthFeature
            {
                AuthProviderType = IdentityServerAuthProviderType.UserAuthProvider,
                AuthRealm = "http://127.0.0.1:65048",         // The URL of the IdentityServer instance
                ClientId = "roclient",            // The Client Identifier so that IdentityServer can identify the service
                ClientSecret = "secret",        // The Client Secret so that IdentityServer can authorize the service
                Scopes = "apiMB openid"         // See {{urlIdvSvr}}/.well-known/openid-configuration for available scopes
            });

            //to register your auth providers
            Plugins.Add(new RegistrationFeature());

            //enable CORS
            Plugins.Add(new CorsFeature());

            //TODO: Basic logging but can implement your own.
            var logger = new CsvRequestLogger();
            logger.EnableRequestBodyTracking = true;
            logger.EnableResponseTracking = true;
            logger.EnableErrorTracking = true;
            logger.EnableSessionTracking = true;
            //TODO: Exclude logging sensitive security stuff here.                       
            //logger.ExcludeRequestDtoTypes = new System.Type[] { typeof(UserPassword) };
            Plugins.Add(new RequestLogsFeature
            {
                RequestLogger = logger
            });

            //Get database settings for context options
            IConfigurationRoot configuration = new ConfigurationBuilder()
                   .SetBasePath(Directory.GetCurrentDirectory())
                   .AddJsonFile("appsettings.json")
                   .Build();

            var optionsBuilder = new DbContextOptionsBuilder<DataContext>();
            optionsBuilder
                .UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
                    providerOptions => providerOptions.CommandTimeout(60))
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

            container.Register<DataContext>(i => new DataContext(optionsBuilder.Options)).ReusedWithin(ReuseScope.Request);

            //See http://docs.servicestack.net/ioc
            //use scoped for multi-threaded use.
            container.AddScoped<IVehicleRepository, VehicleRepository>();
            //is the same as
            //container.RegisterAutoWiredAs<VehicleRepository, IVehicleRepository>().ReusedWithin(ReuseScope.Request);
        }
    }
}
