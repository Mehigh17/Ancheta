using System;
using System.IO;
using System.Reflection;
using Ancheta.Model.MappingProfiles;
using Ancheta.Model.Repositories;
using Ancheta.Model.Services;
using Ancheta.WebApi.Contexts;
using Ancheta.WebApi.Hubs;
using Ancheta.WebApi.Repositories;
using Ancheta.WebApi.Services;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using SharpCatch.Services;

namespace Ancheta.WebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var captchaKey = Configuration["Recaptcha:Key"] ?? throw new ArgumentNullException("The captcha key cannot be null.");
            var connectionString = Configuration["Database:ConnectionString"] ?? throw new ArgumentException("The connection string cannot be null.");

            services.AddDbContext<ApplicationDbContext>(o => o.UseNpgsql(connectionString));

            services.AddScoped<IPollRepository, PollRepository>();
            services.AddScoped<IPollService, PollService>();
            services.AddScoped<IVoteRepository, VoteRepository>();
            services.AddSingleton<ITinyMessengerHub, TinyMessengerHub>();

            services.AddTransient<IRecaptchaService, RecaptchaService>(_ => new RecaptchaService(captchaKey));
            services.AddHostedService<NotificationsService>();

            services.AddAutoMapper(typeof(ViewModelProfile));
            services.AddControllers();
            services.AddRouting(o =>
            {
                o.LowercaseUrls = true;
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Ancheta",
                    Version = "v1",
                    License = new OpenApiLicense
                    {
                        Name = "MIT",
                        Url = new Uri("https://www.mit.edu/~amini/LICENSE.md"),
                    },
                    Description = "Ancheta is a piece of software allowing users to create, manage and vote on polls.",
                    Contact = new OpenApiContact
                    {
                        Name = "Github Repository",
                        Url = new Uri("https://github.com/Mehigh17/Ancheta"),
                    },
                });

                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "v1 API");
                });
            }

            // Setup database
            using (var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();
                dbContext.Database.Migrate();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<NotificationsHub>("/notificationshub");
            });
        }
    }
}
