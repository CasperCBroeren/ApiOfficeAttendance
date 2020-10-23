using System.Threading.Tasks;
using ApiOfficeAttendance.Repository;
using ApiOfficeAttendance.Repository.AzureTableStorage;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Prometheus;

namespace ApiOfficeAttendance
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            var domain = Configuration["Auth0:Domain"];

            services.AddCors(options =>
            {
                options.AddPolicy(name: "default",
                    builder => builder.WithOrigins("http://localhost:5000")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials());
            });

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.Authority = $"https://{domain}/";
                options.Audience = Configuration["Auth0:ApiIdentifier"];
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = OnTokenValidated
                };
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateLifetime = true,
                    ValidateAudience = true,
                    ValidAudience = Configuration["Auth0:ApiIdentifier"]

                };
            });
            services.AddAuthorization(options =>
            {
                options.AddPolicy(PolicyPermissions.ReadAttendance, policy => policy.RequireClaim("permissions", PolicyPermissions.ReadAttendance));
                options.AddPolicy(PolicyPermissions.WriteOwnAttendance, policy => policy.RequireClaim("permissions", PolicyPermissions.WriteOwnAttendance));
            });

            services.AddTransient<IAvailabilityRepository, AvailabilityRepository>();

            services.AddMvc();

        }

        private static readonly Counter TokenValidated = Metrics.CreateCounter("officeattendance_token_validation_total", "Number of JWT tokens validated", "app");

        private Task OnTokenValidated(TokenValidatedContext arg)
        {
            TokenValidated.WithLabels("api").Inc();
            return Task.CompletedTask;
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();


            app.UseCors("default");

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapMetrics();
            });
        }
    }
}
