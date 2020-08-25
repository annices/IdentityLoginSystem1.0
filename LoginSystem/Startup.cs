using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using LoginSystem.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity.UI.Services;
using LoginSystem.Services;

namespace LoginSystem
{
    /// <summary>
    /// This class registers the base settings for the application that will apply on startup.
    /// </summary>
    public class Startup
    {
        // Inject dependencies into the Startup constructor to reach the settings specified in appsettings.json:
        public Startup(IConfiguration configuration) => Configuration = configuration;
        public IConfiguration Configuration { get; }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        public void ConfigureServices(IServiceCollection services)
        {
            // Add the ASP.NET Identity service and configure password, DB, token provider etc:
            services.AddIdentity<AppUser, IdentityRole>(options =>
            {
                options.User.RequireUniqueEmail = true;
            }).AddEntityFrameworkStores<LoginSystemDbContext>().AddDefaultTokenProviders();

            // Register the EmailSender service with the settings specified in appsettings.json:
            services.AddTransient<IEmailSender, EmailSender>(i =>
                new EmailSender(
                    Configuration["EmailSender:Host"],
                    Configuration.GetValue<int>("EmailSender:Port"),
                    Configuration.GetValue<bool>("EmailSender:EnableSSL"),
                    Configuration["EmailSender:UserName"],
                    Configuration["EmailSender:Password"]
                )
            );

            // Register the application database context and call the connection string from appsettings.json:
            services.AddDbContext<LoginSystemDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("LoginSystemConnection")));

            // Register authorization policies to enable the ability to use e.g. claim based authorization:
            services.AddAuthorization(options =>
            {
                // Create policy properties such as "SA" and apply values based on settings specified in appsettings.json.
                // (Note! If you change the policy property names here, they also have to reflect the ones used in the controllers.)
                options.AddPolicy("SA", policy => { policy.RequireRole(Configuration[new Role().SA]); });
                options.AddPolicy("SA&A", policy => { policy.RequireRole(Configuration[new Role().SA], Configuration[new Role().A]); });
                options.AddPolicy("SA&A&LA", policy => { policy.RequireRole(Configuration[new Role().SA], Configuration[new Role().A], Configuration[new Role().LA]); });
                options.AddPolicy("A", policy => { policy.RequireRole(Configuration[new Role().A]); });
                options.AddPolicy("LA", policy => { policy.RequireRole(Configuration[new Role().LA]); });
            });

            services.AddMvc(option => option.EnableEndpointRouting = false);

            // Register support for sessions:
            services.AddSession(options =>
            {
                // Set session cookie timeout:
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                // Make the session cookie essential:
                options.Cookie.IsEssential = true;
            });

            services.ConfigureApplicationCookie(options =>
            {
                // Set cookie timeout:
                options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                options.LoginPath = "/Account/Login";
            });
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        public void Configure(IApplicationBuilder app)
        {
            app.UseHttpsRedirection();
            app.UseStatusCodePages();
            app.UseDeveloperExceptionPage();
            app.UseStaticFiles();
            app.UseSession();
            // The UseAuthentication adds the user's credentials in a cookie on every HTTP Request:
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseMvcWithDefaultRoute();
        }

    } // End class.
} // End namespace.
