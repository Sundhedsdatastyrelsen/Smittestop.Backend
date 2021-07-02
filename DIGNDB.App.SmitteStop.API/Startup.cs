using AutoMapper;
using DIGNDB.App.SmitteStop.API.Attributes;
using DIGNDB.App.SmitteStop.API.Exceptions;
using DIGNDB.App.SmitteStop.API.HealthCheckAuthorization;
using DIGNDB.App.SmitteStop.API.HealthChecks;
using DIGNDB.App.SmitteStop.API.Mappers;
using DIGNDB.App.SmitteStop.API.Services;
using DIGNDB.App.SmitteStop.Core;
using DIGNDB.App.SmitteStop.Core.Contracts;
using DIGNDB.App.SmitteStop.Core.DependencyInjection;
using DIGNDB.App.SmitteStop.Core.Helpers;
using DIGNDB.App.SmitteStop.Core.Services;
using DIGNDB.App.SmitteStop.DAL.Context;
using DIGNDB.App.SmitteStop.DAL.Repositories;
using DIGNDB.App.SmitteStop.Domain.Configuration;
using DIGNDB.APP.SmitteStop.API.Config;
using FederationGatewayApi.Mappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DIGNDB.App.SmitteStop.API
{
    /// <summary>
    /// Startup
    /// </summary>
    [System.Runtime.InteropServices.Guid("69F5557E-D0D6-4A70-B06F-7796FF895325")]
    public class Startup
    {
        private IConfiguration Configuration { get; }
        private readonly IWebHostEnvironment _env;
        private ApiConfig _apiConfig { get; set; }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="env"></param>
        public Startup(IWebHostEnvironment env)
        {
            _env = env;
            var combinedEnvironmentName = env.EnvironmentName.Split('.');

            var environmentName = combinedEnvironmentName.FirstOrDefault();
            var serverName = combinedEnvironmentName.LastOrDefault();

            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile($"appsettings.{environmentName}.json", optional: true)
                .AddJsonFile($"appsettings.{environmentName}.{serverName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            _apiConfig = Configuration.Get<ApiConfig>();
            services.Configure<ApiConfig>(Configuration);
            ModelValidator.ValidateContract(_apiConfig);
            services.AddControllers().AddControllersAsServices();
            services.AddControllers(options => options.Filters.Add(new HttpResponseExceptionFilter()));

            services.AddAuthentication(HealthCheckBasicAuthenticationHandler.HealthCheckBasicAuthenticationScheme).AddNoOperationAuthentication();
            // Configure jwt authentication and adding policy for health check endpoints
            services.AddAuthorization(options =>
            {
                // Adding policy for health check access
                options.AddPolicy(HealthCheckAccessPolicyName, policy =>
                    policy.AddAuthenticationSchemes(HealthCheckBasicAuthenticationHandler.HealthCheckBasicAuthenticationScheme)
                        .Requirements.Add(new HealthCheckAuthorizationRequirement()));
            });

            services.AddSingleton<IAuthorizationHandler, HealthCheckAuthorizationHandler>();

            services.AddMvc(options =>
            {
                options.Filters.Add(typeof(DeprecatedCheckAttribute));
            }).SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
            
            // Add API versioning to as service to your project
            services.AddApiVersioning(config =>
            {
                // Specify the default API Version
                config.DefaultApiVersion = new ApiVersion(1, 0);
                // If the client hasn't specified the API version in the request, use the default API version number
                config.AssumeDefaultVersionWhenUnspecified = true;
                // Advertise the API versions supported for the particular endpoint
                config.ReportApiVersions = true;
                config.ApiVersionReader = ApiVersionReader.Combine(
                        new UrlSegmentApiVersionReader(),
                        new HeaderApiVersionReader()
                        {
                            HeaderNames = { "api-version" }
                        },
                        new QueryStringApiVersionReader("v")
                );
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "API version 1", Version = "v1" });
                c.SwaggerDoc("v2", new OpenApiInfo { Title = "API version 2", Version = "v2" });
                c.OperationFilter<RemoveVersionParameterAttribute>();
                c.DocumentFilter<FilterRoutesDocumentFilter>();
                c.DocumentFilter<ReplaceVersionWithExactValueInPathAttribute>();
                c.EnableAnnotations();
                c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.XML";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });

            services.AddHttpContextAccessor();
            services.AddAutoMapper(typeof(CountryMapper));
            services.AddAutoMapper(typeof(ApplicationStatisticsMapper));
            services.AddAutoMapper(typeof(SSIStatisticsMapper));
            services.AddAutoMapper(typeof(SSIStatisticsVaccinationMapper));
            services.AddAutoMapper(typeof(CovidStatisticsMapper));

            // Disable header token check in dev mode
            services.AddSingleton(new AuthOptions(_env.IsDevelopment()));

            services.AddSingleton<IAppSettingsConfig, Core.AppSettingsConfig>();
            services.AddSingleton(Configuration);
            services.AddSingleton(_apiConfig);
            services.AddSingleton(_apiConfig.TemporaryExposureKeyZipFilesSettings);
            services.AddScoped<MobileAuthorizationAttribute>();
            services.AddScoped<TriforkAuthorizationAttribute>();
            services.AddScoped<DeprecatedCheckAttribute>();
            services.AddScoped<AuthorizationAttribute>();

            services.AddHsts(options =>
            {
                options.MaxAge = DateTime.Now.AddYears(1) - DateTime.Now;
                options.IncludeSubDomains = true;
            });

            services.AddScoped<ICacheOperations, CacheOperations>();
            services.AddScoped<IDatabaseKeysToBinaryStreamMapperService, DatabaseKeysToBinaryStreamMapperService>();
            services.AddScoped(typeof(ITemporaryExposureKeyRepository), typeof(TemporaryExposureKeyRepository));
            services.AddScoped<IExposureKeyMapper, ExposureKeyMapper>();
            services.AddScoped<IExposureKeyValidator, ExposureKeyValidator>();
            services.AddScoped<ILogMessageValidator, LogMessageValidator>();
            services.AddScoped<IAppleService, AppleService>();
            services.AddScoped<IKeysListToMemoryStreamConverter, KeysListToMemoryStreamConverter>();
            services.AddScoped<ICountryService, CountryService>();
            services.AddScoped<IPackageBuilderService, PackageBuilderService>();
            services.AddScoped<IAddTemporaryExposureKeyService, AddTemporaryExposureKeyService>();
            services.AddScoped<IZipFileInfoService, ZipFileInfoService>();
            services.AddScoped<IFileSystem, FileSystem>();
            services.AddScoped<IUploadFileValidationService, UploadFileValidationService>();
            services.AddScoped<IHealthCheckHangFireService, HealthCheckHangFireService>();


            // Health checks
            services.AddHealthChecks()
                .AddDbContextCheck<DigNDB_SmittestopContext>("DB Smittestop", HealthStatus.Unhealthy, new[] {DatabaseTag})
                .AddCheck<HangFireHealthCheck>("HangFire", HealthStatus.Unhealthy, new[] {HangFireTag})
                .AddCheck<LogFilesHealthCheck>("Log files", HealthStatus.Unhealthy, new[] { LogFilesTag })
                .AddCheck<ZipFilesHealthCheck>("Zip files", HealthStatus.Unhealthy, new[] { ZipFilesTag })
                .AddCheck<NumbersTodayHealthCheck>("Numbers today", HealthStatus.Unhealthy, new[] {NumbersTodayTag})
                .AddCheck<RollingStartNumberHealthCheck>("Rolling start number", HealthStatus.Unhealthy, new[] {RollingStartNumberTag});

            // Context
            var connectionString = Configuration["SQLConnectionString"];
            services.AddDbContext<DigNDB_SmittestopContext>(opts =>
                opts.UseSqlServer(connectionString, x => x.MigrationsAssembly("DIGNDB.App.SmitteStop.DAL")));
            services.AddScoped<DigNDB_SmittestopContext>();
            services.AddScoped<ICacheOperationsV2, CacheOperationsV2>();
            
            // Repositories
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped(typeof(IDataAccessLoggingService<>), typeof(DataAccessLoggingService<>));
            services.AddScoped<ICountryRepository, CountryRepository>();
            services.AddScoped<ISSIStatisticsRepository, SSIStatisticsRepository>();
            services.AddScoped<ISSIStatisticsVaccinationRepository, SSIStatisticsVaccinationRepository>();
            services.AddScoped<IApplicationStatisticsRepository, ApplicationStatisticsRepository>();
            services.AddScoped<ILoginInformationRepository, LoginInformationRepository>();

            services.AddSingleton<IExposureConfigurationService, ExposureConfigurationService>();
            services.AddSingleton<IExportKeyConfigurationService, ExportKeyConfigurationService>();
            services.AddSingleton<IKeyValidationConfigurationService, KeyValidationConfigurationService>();

            services.AddScoped<IKeyValidator, KeyValidator>();
            services.AddScoped<IEpochConverter, EpochConverter>();
            services.AddScoped<IRiskCalculator, RiskCalculator>();

            services.AddMemoryCache();

            var controllers = Assembly.GetExecutingAssembly().GetTypes()
                .Where(type => typeof(ControllerBase).IsAssignableFrom(type))
                .ToList();
            InjectionChecker.CheckIfAreAnyDependenciesAreMissing(services, controllers);
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        /// <param name="loggerFactory"></param>
        /// <param name="logger"></param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory, ILogger<Startup> logger)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            loggerFactory.AddFile(@"" + Configuration.GetSection(Core.AppSettingsConfig.AppSettingsSectionName)
                .GetValue<string>("logsApiPath"));

            app.UseHttpsRedirection();

            app.UseHsts();

            app.Use(async (context, next) =>
            {
                context.Response.Headers.Add("X-Frame-Options", "SAMEORIGIN");
                await next();
            });

            app.UseRouting();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("v1/swagger.json", "API Version 1");
                c.SwaggerEndpoint("v2/swagger.json", "API Version 2");
            });

            // Who are you...
            app.UseAuthentication();
            // ...what you may access
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();

                // Health checks mapped to different endpoints
                MapHealthChecks(endpoints);
            });
            
            var exposureConfigurationService = app.ApplicationServices.GetService<IExposureConfigurationService>();
            exposureConfigurationService.SetConfiguration(Configuration);

            var exportKeyConfigurationService = app.ApplicationServices.GetService<IExportKeyConfigurationService>();
            exportKeyConfigurationService.SetConfiguration(Configuration);
            var keyValidationConfigurationService = app.ApplicationServices.GetService<IKeyValidationConfigurationService>();
            keyValidationConfigurationService.SetConfiguration(Configuration);
        }

        #region Health check mapping and response write

        /// <summary>
        /// Name of policy used for health check authorization
        /// </summary>
        public const string HealthCheckAccessPolicyName = "HealtCheckAccess";

        private const string DatabaseTag = "database";
        private const string DatabasePattern = "/health/database";
        private const string HangFireTag = "hangfire";
        private const string HangFirePattern = "/health/hangfire";
        private const string LogFilesTag = "logfiles";
        private const string LogFilesPattern = "/health/logfiles";
        
        private const string ZipFilesTag = "zipfiles";
        /// <summary>
        /// Path to health check endpoint for checking zip files
        /// </summary>
        public const string ZipFilesPattern = "/health/zipfiles";
        
        private const string NumbersTodayTag = "numberstoday";
        /// <summary>
        /// Path to health check endpoint for checking today's numbers
        /// </summary>
        public const string NumbersTodayPattern = "/health/numberstoday";

        private const string RollingStartNumberTag = "rollingstartnumber";
        /// <summary>
        /// Path to health check endpoint for checking rolllingStartNumber
        /// </summary>
        public const string RollingStartNumberPattern = "/health/rollingstartnumber";
        
        private static void MapHealthChecks(IEndpointRouteBuilder endpoints)
        {
            endpoints.MapHealthChecks(DatabasePattern, new HealthCheckOptions()
            {
                Predicate = check => check.Tags.Contains(DatabaseTag),
                ResponseWriter = WriteHealthCheckResponse
            }).RequireAuthorization(HealthCheckAccessPolicyName);

            endpoints.MapHealthChecks(HangFirePattern, new HealthCheckOptions()
            {
                Predicate = check => check.Tags.Contains(HangFireTag),
                ResponseWriter = WriteHealthCheckResponse
            }).RequireAuthorization(HealthCheckAccessPolicyName);

            endpoints.MapHealthChecks(LogFilesPattern, new HealthCheckOptions()
            {
                Predicate = check => check.Tags.Contains(LogFilesTag),
                ResponseWriter = WriteHealthCheckResponse
            }).RequireAuthorization(HealthCheckAccessPolicyName);

            endpoints.MapHealthChecks(ZipFilesPattern, new HealthCheckOptions()
            {
                Predicate = check => check.Tags.Contains(ZipFilesTag),
                ResponseWriter = WriteHealthCheckResponse
            }).RequireAuthorization(HealthCheckAccessPolicyName);

            endpoints.MapHealthChecks(NumbersTodayPattern, new HealthCheckOptions()
            {
                Predicate = check => check.Tags.Contains(NumbersTodayTag),
                ResponseWriter = WriteHealthCheckResponse
            }).RequireAuthorization(HealthCheckAccessPolicyName);

            endpoints.MapHealthChecks(RollingStartNumberPattern, new HealthCheckOptions()
            {
                Predicate = check => check.Tags.Contains(RollingStartNumberTag),
                ResponseWriter = WriteHealthCheckResponse
            }).RequireAuthorization(HealthCheckAccessPolicyName);
        }

        private static Task WriteHealthCheckResponse(HttpContext context, HealthReport result)
        {
            context.Response.ContentType = "application/json; charset=utf-8";

            var options = new JsonWriterOptions
            {
                Indented = true
            };

            using var stream = new MemoryStream();
            using (var writer = new Utf8JsonWriter(stream, options))
            {
                writer.WriteStartObject();
                writer.WriteString("status", result.Status.ToString());
                writer.WriteStartObject("results");
                foreach (var entry in result.Entries)
                {
                    // Status, description, and exception (if any)
                    writer.WriteStartObject(entry.Key);
                    writer.WriteString("status", entry.Value.Status.ToString());
                    writer.WriteString("description", entry.Value.Description);
                    
                    // Exception
                    if (entry.Value.Exception != null)
                    {
                        writer.WriteStartObject("exception");
                        writer.WriteString("message", entry.Value.Exception.Message);
                        writer.WriteString("stackTrace", entry.Value.Exception.StackTrace);
                        writer.WriteEndObject();
                    }
                    writer.WriteEndObject();

                    // Write data from health checks to response
                    writer.WriteStartObject("data");
                    try
                    {
                        foreach (var (key, value) in entry.Value.Data)
                        {
                            if (value is string)
                            {
                                var val = value.ToString();
                                writer.WriteString(key, val);
                            }
                            else
                            {
                                var itemValue = JsonSerializer.Serialize(value, value?.GetType() ?? typeof(object));
                                writer.WriteString(key, itemValue);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        var errorMessage = $"Error in writing to health check response: {e.Message} - {e.StackTrace}";
                        writer.WriteString("Error in writing health check response", errorMessage);
                    }
                    
                    writer.WriteEndObject();
                }
                writer.WriteEndObject();
                writer.WriteEndObject();
            }

            var json = Encoding.UTF8.GetString(stream.ToArray());

            return context.Response.WriteAsync(json);
        }

        #endregion
    }
}
