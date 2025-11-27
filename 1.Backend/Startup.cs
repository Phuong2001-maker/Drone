using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.ResponseCaching;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PT.Base;
using PT.Domain.Model;
using PT.Infrastructure;
using PT.Infrastructure.Interfaces;
using PT.Infrastructure.Repositories;
using PT.Shared;
using PT.UI.SignalR;
using Serilog;

namespace PT.UI
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;
            Serilog.Log.Logger = new LoggerConfiguration()
               .MinimumLevel.Warning()
               .WriteTo.RollingFile(Path.Combine(env.ContentRootPath, "logs/log-{Date}.txt"))
               .CreateLogger();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDataProtection().SetApplicationName("rosedentalclinic");

            services.AddRouting();
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => false;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });
            services.AddDbContext<ApplicationContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")), ServiceLifetime.Scoped);
            services.AddIdentity<ApplicationUser, ApplicationRole>().AddEntityFrameworkStores<ApplicationContext>().AddDefaultTokenProviders();
            // Add Custom Claims processor
            services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, CustomClaimsPrincipalFactory>();
            services.Configure<BaseSettings>(Configuration.GetSection("BaseSettings"));
            services.Configure<LogSettings>(Configuration.GetSection("LogSettings"));
            services.Configure<EmailSettings>(Configuration.GetSection("EmailSettings"));
            services.Configure<SocketSettings>(Configuration.GetSection("SocketSettings"));
            services.Configure<List<SeoSettings>>(Configuration.GetSection("SeoSettings"));
            services.Configure<List<WebsiteInfoSettings>>(Configuration.GetSection("WebsiteInfoSettings"));
            services.Configure<ExchangeRateSettings>(Configuration.GetSection("ExchangeRateSettings"));
            services.Configure<BindContentSettings>(Configuration.GetSection("BindContentSettings"));
            services.Configure<List<AdvertisingHomepageSettings>>(Configuration.GetSection("AdvertisingHomepageSettings"));
            services.Configure<AuthorizeSettings>(Configuration.GetSection("AuthorizeSettings"));
            services.Configure<List<RedirectLinkSetting>>(Configuration.GetSection("RedirectLinkSettings"));
            services.AddMemoryCache();

            services.Configure<IdentityOptions>(options =>
            {
                // Password settings
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = false;
                options.Password.RequiredUniqueChars = 6;

                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(120);
                options.Lockout.MaxFailedAccessAttempts = 10;
                options.Lockout.AllowedForNewUsers = true;

                // User settings
                options.User.RequireUniqueEmail = true;
            });

            services.ConfigureApplicationCookie(options =>
            {
                // Cookie settings
                options.Cookie.HttpOnly = true;
                options.Cookie.Expiration = TimeSpan.FromHours(2);
                options.LoginPath = "/Login"; // If the LoginPath is not set here, ASP.NET Core will default to /Account/Login
                options.LogoutPath = "/Logout"; // If the LogoutPath is not set here, ASP.NET Core will default to /Account/Logout
                options.AccessDeniedPath = "/Admin/AccessDenied"; // If the AccessDeniedPath is not set here, ASP.NET Core will default to /Account/AccessDenied
                options.SlidingExpiration = true;

            });

            //services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            //      .AddCookie(options =>
            //      {
            //          options.Cookie.HttpOnly = true;
            //          options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            //          options.Cookie.SameSite = SameSiteMode.Lax;
            //      });

            var supportedCultures = ListData.ListLanguage.Select(x => new CultureInfo(x.Id)).ToArray();


           

            services.AddLocalization(options => options.ResourcesPath = "Resources");
            services.Configure<RequestLocalizationOptions>(options =>
            {
                options.DefaultRequestCulture = new RequestCulture("en","en-US");
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
                //options.RequestCultureProviders = new List<IRequestCultureProvider>
                //{
                //    new QueryStringRequestCultureProvider(),
                //    new CookieRequestCultureProvider()
                //};

                options.RequestCultureProviders.Insert(0, new UrlRequestCultureProvider
                {
                    Options = options
                });
            });

            // Adding our UrlRequestCultureProvider as first object in the list
            
            // Ngôn ngữ End
            // Add application services.
            services.AddScoped<IEmailSenderRepository, EmailSenderRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRoleAreaRepository, RoleAreaRepository>();
            services.AddScoped<IRoleGroupRepository, RoleGroupRepository>();
            services.AddScoped<IRoleControllerRepository, RoleControllerRepository>();
            services.AddScoped<IRoleControllerRepository, RoleControllerRepository>();
            services.AddScoped<IRoleActionRepository, RoleActionRepository>();
            services.AddScoped<IRoleDetailRepository, RoleDetailRepository>();
            services.AddScoped<ILogRepository, LogRepository>();
            services.AddScoped<IEmailSenderRepository, EmailSenderRepository>();
            services.AddScoped<IFileRepository, FileRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();

            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<ILinkRepository, LinkRepository>();

            services.AddScoped<IContentPageCategoryRepository, ContentPageCategoryRepository>();
            services.AddScoped<IContentPageRelatedRepository, ContentPageRelatedRepository>();
            services.AddScoped<IContentPageRepository, ContentPageRepository>();
            services.AddScoped<IContentPageTagRepository, ContentPageTagRepository>();
            services.AddScoped<ITagRepository, TagRepository>();

            services.AddScoped<IEmployeeRepository, EmployeeRepository>();
            services.AddScoped<IContactRepository, ContactRepository>();
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<IStaticInformationRepository, StaticInformationRepository>();

            services.AddScoped<IMenuItemRepository, MenuItemRepository>();
            services.AddScoped<IMenuRepository, MenuRepository>();
            services.AddScoped<IBannerRepository, BannerRepository>();
            services.AddScoped<IBannerItemRepository, BannerItemRepository>();
            services.AddScoped<IServicePriceRepository, ServicePriceRepository>();
            services.AddScoped<IImageGalleryRepository, ImageGalleryRepository>();
            services.AddScoped<IImageRepository, ImageRepository>();
            services.AddScoped<ILinkReferenceRepository, LinkReferenceRepository>();
            services.AddScoped<IContentPageReferenceRepository, ContentPageReferenceRepository>();
            services.AddScoped<ICountryRepository, CountryRepository>();
            services.AddScoped<IContactLogRepository, ContactLogRepository>();

            //Gzip
            services.Configure<GzipCompressionProviderOptions>(options => options.Level = System.IO.Compression.CompressionLevel.Optimal);

            services.AddResponseCompression(options =>
            {
                //options.EnableForHttps = true;
                options.Providers.Add<GzipCompressionProvider>();
                options.MimeTypes = new string[]{
                        "text/plain",
                        "text/css",
                        "application/javascript",
                        "text/html",
                        "application/xml",
                        "text/xml",
                        "application/json",
                        "text/json",
                        "image/svg+xml",
                        "application/atom+xml"
                    };
            });
       
            //Content/Admin/plugins/signalr
            services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 209_715_200;
            });

            services.AddDistributedMemoryCache();

            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.Name = ".PhamTrong.Session";
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            });

            services.AddLocalization(options => options.ResourcesPath = "Resources");

            //services.AddHttpsRedirection(options =>
            //{
            //    options.HttpsPort = 443;
            //    options.RedirectStatusCode = 301;
            //});

            services.AddMvc(options =>
            {
                options.EnableEndpointRouting = false;
                //options.Filters.Add(new RequireHttpsAttribute
                //{
                //    Permanent = true
                //});
                options.Filters.Add(new RequireWwwAttribute
                {
                    IgnoreLocalhost = true,
                    Permanent = true
                });
            })
                .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix, opts => { opts.ResourcesPath = "Resources"; })
                .AddDataAnnotationsLocalization()
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                }).SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

           

            // services.AddSignalR();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {

            //var supportedCultures = new[] { new CultureInfo("en"), new CultureInfo(HistoryBuy) };
            //app.UseRequestLocalization(new RequestLocalizationOptions()
            //{
            //    DefaultRequestCulture = new RequestCulture(new CultureInfo("vi")),
            //    SupportedCultures = supportedCultures,
            //    SupportedUICultures = supportedCultures
            //});

            

            loggerFactory.AddSerilog();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseStatusCodePagesWithReExecute("/Home/Error/{0}");
                app.UseHsts();
            }

            AppHttpContext.Services = app.ApplicationServices;

            //Gzip
            app.UseResponseCompression();
            //app.UseHttpsRedirection();

            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = ctx =>
                {
                    // Requires the following import:
                    // using Microsoft.AspNetCore.Http;
                    ctx.Context.Response.Headers.Append("Cache-Control", $"public, max-age={604800* 58}");
                }
            });

            var localizationOption = app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>();
            app.UseRequestLocalization(localizationOption.Value);

            app.UseCookiePolicy();
            app.UseSession();
            app.UseAuthentication();
            app.UseMvc(routes =>
            {
                routes.Routes.Add(new CustomRouter(routes.DefaultHandler));
                routes.MapRoute(
                name: "areas",
                template: "Admin/{area:exists}/{controller=Home}/{action=Index}/{id?}");

             routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });


            //app.UseSignalR(routes =>
            //{
            //    routes.MapHub<WebsiteHub>("/WebsiteHub/Message");
            //});

        }
    }
}
