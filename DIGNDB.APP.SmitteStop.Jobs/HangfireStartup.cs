using AutoMapper;
using DIGNDB.App.SmitteStop.API.Services;
using DIGNDB.App.SmitteStop.Core.Adapters;
using DIGNDB.App.SmitteStop.Core.Contracts;
using DIGNDB.App.SmitteStop.Core.DependencyInjection;
using DIGNDB.App.SmitteStop.Core.Helpers;
using DIGNDB.App.SmitteStop.Core.Services;
using DIGNDB.App.SmitteStop.DAL.Context;
using DIGNDB.App.SmitteStop.DAL.Repositories;
using DIGNDB.App.SmitteStop.Jobs.Services;
using DIGNDB.App.SmitteStop.Jobs.Services.Interfaces;
using DIGNDB.APP.SmitteStop.Jobs.Config;
using DIGNDB.APP.SmitteStop.Jobs.Services;
using FederationGatewayApi.Contracts;
using FederationGatewayApi.Mappers;
using FederationGatewayApi.Services;
using FederationGatewayApi.Services.Settings;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.EventLog;
using System.Linq;
using System.Reflection;
using ZipFileService = DIGNDB.APP.SmitteStop.Jobs.Services.ZipFileService;

namespace DIGNDB.APP.SmitteStop.Jobs
{
    public class HangfireStartup
    {
        private HangfireConfig _hangfireConfig { get; set; }

        private IConfiguration _configuration { get; }

        public HangfireStartup(IWebHostEnvironment env)
        {
            var combinedEnvironmentName = env.EnvironmentName.Split('.');

            var environmentName = combinedEnvironmentName.FirstOrDefault();
            var serverName = combinedEnvironmentName.LastOrDefault();

            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile($"appsettings.json", optional: false)
                .AddJsonFile($"appsettings.{environmentName}.json", optional: true)
                .AddJsonFile($"appsettings.{environmentName}.{serverName}.json", optional: true)
                .AddEnvironmentVariables();
            _configuration = builder.Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            _hangfireConfig = _configuration.Get<HangfireConfig>();
            services.Configure<HangfireConfig>(_configuration);
            ModelValidator.ValidateContract(_hangfireConfig);
            var gateWayConfig = _hangfireConfig.EuGateway;

            var _eventLogConfig = _hangfireConfig.Logging.EventLog;
            services.Configure<EventLogSettings>(config =>
            {
                config.SourceName = _eventLogConfig.SourceName;
                config.LogName = _eventLogConfig.LogName;
                config.MachineName = _eventLogConfig.MachineName;
            });

            services.AddControllers().AddControllersAsServices();
            services.AddLogging();

            services.AddAutoMapper(Assembly.GetExecutingAssembly());
            services.AddAutoMapper(typeof(CountryMapper));

            services.AddHangfire(x => x.UseSqlServerStorage(_hangfireConfig.HangFireConnectionString));
            services.AddDbContext<DigNDB_SmittestopContext>(opts =>
                opts.UseSqlServer(_hangfireConfig.SmittestopConnectionString));
            services.AddScoped<ISSIZipFolderHandlingService, SSIZipFolderHandlingService>();
            services.AddScoped<ITemporaryExposureKeyRepository, TemporaryExposureKeyRepository>();
            services.AddScoped<IDatabaseKeysValidationService, DatabaseKeysValidationService>();
            services.AddScoped<IKeyValidator, KeyValidator>();
            services.AddScoped<IEpochConverter, EpochConverter>();
            services.AddScoped<IRiskCalculator, RiskCalculator>();
            services.AddScoped<IKeyFilter, KeyFilter>();
            services.AddScoped<IGatewayWebContextReader, GatewayWebContextReader>();
            services.AddScoped<IEuGatewayService, EuGatewayService>();
            services.AddScoped<IZipFileInfoService, ZipFileInfoService>();
            services.AddScoped<IZipFileService, ZipFileService>();
            services.AddScoped<ISSIZipFileReaderService, SSIZipFileReaderService>();
            services.AddScoped<ISSIStatisticsRepository, SSIStatisticsRepository>();
            services.AddScoped<ISSIStatisticsVaccinationRepository, SSIStatisticsVaccinationRepository>();
            services.AddScoped<IApplicationStatisticsRepository, ApplicationStatisticsRepository>();
            services.AddScoped<IPackageBuilderService, PackageBuilderService>();
            services.AddScoped<IDatabaseKeysToBinaryStreamMapperService, DatabaseKeysToBinaryStreamMapperService>();
            services.AddScoped<IKeysListToMemoryStreamConverter, KeysListToMemoryStreamConverter>();
            services.AddScoped<ISettingRepository, SettingRepository>();
            services.AddScoped<IExposureConfigurationService, ExposureConfigurationService>();
            services.AddScoped<IDateTimeNowWrapper, DateTimeNowWrapper>();
            services.AddScoped<IExposureKeyMapper, ExposureKeyMapper>();
            services.AddScoped<IFileSystem, FileSystem>();
            services.AddSingleton(gateWayConfig);
            services.AddSingleton(_hangfireConfig.Jobs.UploadKeysToTheGateway);
            services.AddSingleton(_hangfireConfig.TemporaryExposureKeyZipFilesSettings);
            services.AddSingleton(_hangfireConfig.Jobs.ProcessSSIDataInFolder.ZipFolderProcessingConfig);
            services.AddSingleton(_hangfireConfig.Jobs.ProcessSSIDataInFolder.ExcelParsingConfig);

            services.AddScoped<IEncodingService, EncodingService>();
            services.AddScoped<ISignatureService, SignatureService>();
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<ICountryRepository, CountryRepository>();

            services.AddScoped<IDaysSinceOnsetOfSymptomsDecoder, DaysSinceOnsetOfSymptomsDecoder>();

            services.AddScoped<ISettingRepository, SettingRepository>();
            services.AddScoped<IGatewaySyncStateSettingsDao, GatewaySyncStateSettingsDao>();
            services.AddScoped<ISettingsService, SettingsService>();
            services.AddScoped<IEFGSKeyStoreService, EFGSKeyStoreService>();
            services.AddScoped<IGatewayHttpClient, GatewayHttpClient>();
            services.AddSingleton<IKeyValidationConfigurationService, KeyValidationConfigurationService>();
            services.AddSingleton(typeof(ILoggerAdapter<>), typeof(LoggerAdapter<>));

            services.AddScoped<IAddTemporaryExposureKeyService, AddTemporaryExposureKeyService>();
            services.AddSingleton<IGatewayKeyProvider>(
               new GatewayKeyProvider(gateWayConfig.AuthenticationCertificateFingerprint, gateWayConfig.SigningCertificateFingerprint));

            var controllers = Assembly.GetExecutingAssembly().GetTypes()
                .Where(type => typeof(ControllerBase).IsAssignableFrom(type))
                .ToList();
            InjectionChecker.CheckIfAreAnyDependenciesAreMissing(services, controllers);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddFile(_hangfireConfig.LogsPath);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseHangfireDashboard();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseHangfireServer(options: new BackgroundJobServerOptions { WorkerCount = 1 });

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            ScheduledJobsConfiguration.ConfigureScheduledJobs(_hangfireConfig);
        }
    }
}
