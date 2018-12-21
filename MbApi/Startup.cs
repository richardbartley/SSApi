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
using System.IdentityModel.Tokens.Jwt;
using ServiceStack;
using ServiceStack.Auth;

namespace SSApi
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public Startup(IConfiguration configuration) => Configuration = configuration;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
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

            //IdentityServer will redirect too SSApi upon successful login.
            app.UseServiceStack(new AppHost("http://127.0.0.1:6000")
            {
                AppSettings = new NetCoreAppSettings(Configuration)
            });

            app.UseMvc();
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
            var isDebug = AppSettings.Get(nameof(HostConfig.DebugMode), false);
            SetConfig(new HostConfig
            {
                // the url:port that IdentityServer will know to redirect to upon succesful login
                WebHostUrl = serviceUrl,
                DebugMode = isDebug
            });

            //Lets just use IdentityServer for authentication
            //See https://stackoverflow.com/questions/53777907/servicestack-trying-to-create-my-own-openidoauthprovider-but-vs-2017-says-assemb/53780449?noredirect=1#comment94484236_53780449
            Plugins.Add(new AuthFeature(() => new AuthUserSession(),
                new IAuthProvider[] {
                     new NetCoreIdentityAuthProvider(AppSettings), 
              }));

            //to register your auth providers
            Plugins.Add(new RegistrationFeature());

            //enable CORS
            Plugins.Add(new CorsFeature());

            //Get database settings for context options
            var configuration = ((NetCoreAppSettings)AppSettings).Configuration;
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
