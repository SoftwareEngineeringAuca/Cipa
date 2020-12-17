using Cipa.Interfaces;
using Cipa.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Cipa
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddTransient<ICountryRepository, CountryRepository>();
            services.AddTransient<IDuplicationRepository, DuplicationRepository>();
            services.AddTransient<IExamsRepository, ExamsRepository>();
            services.AddTransient<IMessageDeliveryRepository, MessageDeliveryRepository>(); 
            services.AddTransient<IWorkCodeRepository, WorkCodeRepository>(); 
            services.AddTransient<ICipaSystemRepository, CipaSystemRepository>(); 
            services.AddTransient<IExamsSessionsRepository, ExamsSessionsRepository>(); 
            services.AddTransient<IUsersSessionRepository, UsersSessionRepository>(); 
            services.AddTransient<IGradingRepository, GradingRepository>(); 
            services.AddTransient<IScoreRepository, ScoreRepository>(); 
            services.AddTransient<ITaxonomyRepository, TaxonomyRepository>(); 
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapRazorPages(); });
        }
    }
}