﻿using System;
using System.Linq;
using HamnetDbAbstraction;
using HamnetDbRest.Controllers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RestService.Database;
using RestService.DataFetchingService;
using SnmpAbstraction;

namespace HamnetDbRest
{
    /// <summary>
    /// The class handling the initialization of the service.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Constructs the class taking a configuration to start.
        /// </summary>
        /// <param name="configuration">The configuraiton to start.</param>
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        /// <summary>
        /// Gets the service configuration.
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services">The service collection to receive more services.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().AddNewtonsoftJson();

            var rssiSection = this.Configuration.GetSection(Program.RssiAquisitionServiceSectionKey);
            if ((rssiSection == null) || rssiSection.GetValue<bool>("Enabled"))
            {
                services.AddHostedService<RssiAquisitionService>();
            }

            var bpgSection = this.Configuration.GetSection(Program.BgpAquisitionServiceSectionKey);
            if ((bpgSection == null) || bpgSection.GetValue<bool>("Enabled"))
            {
                services.AddHostedService<BgpAquisitionService>();
            }

            var maintenanceSection = this.Configuration.GetSection(MaintenanceService.MaintenanceServiceSectionKey);
            if ((maintenanceSection == null) || maintenanceSection.GetValue<bool>("Enabled"))
            {
                services.AddHostedService<MaintenanceService>();
            }

            services.AddTransient<VwRestRssiController>();
            services.AddTransient<RestController>();
            services.AddTransient<LinkTestController>();
            services.AddTransient<StatusController>();
            services.AddTransient<CacheInfoApiController>();
            services.AddTransient<BgpController>();
            services.AddTransient<ToolController>();

            var hamnetDbAccess = HamnetDbProvider.Instance.GetHamnetDbFromConfiguration(this.Configuration.GetSection(HamnetDbProvider.HamnetDbSectionName));
            services.AddSingleton(hamnetDbAccess);

            IFailureRetryFilteringDataHandler retryFeasibleHandler = new FailureRetryFilteringDataHandler(this.Configuration);
            services.AddSingleton<IFailureRetryFilteringDataHandler>(retryFeasibleHandler);

            QueryResultDatabaseProvider.Instance.SetConfiguration(this.Configuration);
            CacheMaintenance.SetDatabaseConfiguration(this.Configuration);

            var databaseType = this.Configuration.GetSection(QueryResultDatabaseProvider.ResultDatabaseSectionName).GetValue<string>(QueryResultDatabaseProvider.DatabaseTypeKey)?.ToUpperInvariant();

            switch(databaseType)
            {
                case "SQLITE":
                    services.AddDbContext<QueryResultDatabaseContext>(opt => opt.UseSqlite(this.Configuration.GetSection(QueryResultDatabaseProvider.ResultDatabaseSectionName).GetValue<string>(QueryResultDatabaseProvider.ConnectionStringKey)));
                    break;

                case "MYSQL":
                    var connectionString = this.Configuration.GetSection(QueryResultDatabaseProvider.ResultDatabaseSectionName).GetValue<string>(QueryResultDatabaseProvider.ConnectionStringKey);
                    services.AddDbContext<QueryResultDatabaseContext>(opt => opt.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
                    break;

                default:
                    throw new ArgumentOutOfRangeException($"The configured database type '{databaseType}' is not supported for the query result database");
            }


        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <param name="env">The hosting environment.</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseStatusCodePages();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");

                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                //app.UseHsts();
            }

            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetRequiredService<QueryResultDatabaseContext>();
                context.Database.Migrate();

                var retryHandler = serviceScope.ServiceProvider.GetRequiredService<IFailureRetryFilteringDataHandler>();
                retryHandler.InitializeData(QueryType.BgpQuery, context.BgpFailingQueries.AsQueryable().Where(bfq => bfq.PenaltyInfo != null).AsEnumerable().Select(bfq => new SingleFailureInfoWithEntity(EntityType.Host, bfq.Host, bfq.PenaltyInfo)));
                retryHandler.InitializeData(QueryType.RssiQuery, context.RssiFailingQueries.AsQueryable().Where(rfq => rfq.PenaltyInfo != null).AsEnumerable().Select(rfq => new SingleFailureInfoWithEntity(EntityType.Subnet, rfq.Subnet, rfq.PenaltyInfo)));
                retryHandler.InitializeData(QueryType.RssiQuery, context.RssiFailingQueries.AsQueryable().Where(rfq => rfq.PenaltyInfo != null).AsEnumerable().SelectMany(rfq => rfq.AffectedHosts.Select(afh => new SingleFailureInfoWithEntity(EntityType.Host, afh, rfq.PenaltyInfo))));
            }

            //app.UseHttpsRedirection();

            app.UseRouting();
            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
            });
        }
    }
}
