using HamnetDbRest.Controllers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RestService.Database;
using RestService.DataFetchingService;

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
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            var querySection = this.Configuration.GetSection(DataAquisitionService.AquisitionServiceSectionKey);
            if ((querySection == null) || querySection.GetValue<bool>("Enabled"))
            {
                services.AddHostedService<DataAquisitionService>();
            }

            var maintenanceSection = this.Configuration.GetSection(MaintenanceService.MaintenanceServiceSectionKey);
            if ((maintenanceSection == null) || maintenanceSection.GetValue<bool>("Enabled"))
            {
                services.AddHostedService<MaintenanceService>();
            }

            services.AddTransient<VwRestRssiController>();

            services.AddDbContext<QueryResultDatabaseContext>(opt => opt.UseSqlite(this.Configuration.GetConnectionString("ResultDbConnection")));
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <param name="env">The hosting environment.</param>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetRequiredService<QueryResultDatabaseContext>();
                context.Database.Migrate();
            }

            //app.UseHttpsRedirection();
            
            app.UseMvc();
        }
    }
}
