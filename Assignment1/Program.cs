using Assignment1.Controllers;
using Assignment1.Data;
using Assignment1.Data.Migrations;
using Assignment1.Models;
using Azure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SQLitePCL;
using System.Buffers.Text;
using System.Text;

namespace Assignment1
{
    // I, Ben Gozdowski, student number 000906539, certify that this material is my
    // original work. No other person's work has been used without due
    // acknowledgement and I have not made my work available to anyone else.
    public class Program
    {
        public static AppSecrets appSecrets { get; set; }
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.WebHost.ConfigureKestrel(serverOptions =>
            {
                serverOptions.AddServerHeader = false;
            });

            // Add services to the container.
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = true;

                // LOGIN RETRY COUNT / ACCOUNT LOCKOUT
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;
            })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders()
                .AddDefaultUI();

            // SECURE COOKIES + SESSION TIMEOUT
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // Session timeout
                options.SlidingExpiration = true;
            });

            builder.Services.AddControllersWithViews();
            
            
            var kvUri = new Uri(builder.Configuration.GetSection("KVURI").Value);
            var azCred = new DefaultAzureCredential();

            builder.Configuration.AddAzureKeyVault(kvUri, azCred);
            DbInitializer.secretEmployeePassword=builder.Configuration.GetSection("EmployeePassword").Value;
            DbInitializer.secretSupervisorPassword = builder.Configuration.GetSection("SupervisorPassword").Value;

            // GET KEY VAULT VALUES AND  REMOVE COMMA'S
            string keys = builder.Configuration["AESKey"];
           
           
            string iv = builder.Configuration["AESIV"];

            byte[] keystoByte = keys.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(k => byte.Parse(k.Trim()))
                    .ToArray();

            byte[] ivstoByte = iv.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(i => byte.Parse(i.Trim()))
                    .ToArray();


            byte[] aesKey = keystoByte;
            byte[] aesIV = ivstoByte;

            //ASSIGN THE VALUES OF THE CONTROLLER KEYS AND IVS TO KEY VAULT VALUES


            MessagesController.AESKEYS = aesKey;
            MessagesController.AESIV = aesIV;

            EmployeesController.AESKEYS = aesKey;
            EmployeesController.AESIV = aesIV;
         
          






            var app = builder.Build();

            // LOAD SECRETS FOR DBINITIAILIZER (from professor's code)
            var configuration = app.Services.GetService<IConfiguration>();
            var hosting = app.Services.GetService<IWebHostEnvironment>();

            if (hosting.IsDevelopment())
            {
                var secrets = configuration.GetSection("Secrets").Get<AppSecrets>();
                DbInitializer.appSecrets = secrets;
            }

            using (var scope = app.Services.CreateScope())
            {
                DbInitializer.SeedUsersAndRoles(scope.ServiceProvider).Wait();
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            // SECURITY HEADERS - ANTI-CLICKJACKING + CSP
            app.Use(async (context, next) =>
            {
                // Anti-clickjacking
                context.Response.Headers.Append("X-Frame-Options", "DENY");

                // Content Security Policy (CSP) - with fallback
                context.Response.Headers.Append("Content-Security-Policy",
                    "default-src 'self'; " +
                    "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
                    "style-src 'self' 'unsafe-inline'; " +
                    "img-src 'self' data:; " +
                    "font-src 'self'; " +
                    "frame-ancestors 'none'; " +
                    "form-action 'self'; " +
                    "base-uri 'self';");

                // Extra security headers
                context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
                context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
                context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");

                // Remove server info
                context.Response.Headers.Remove("Server");
                context.Response.Headers.Remove("X-Powered-By");

                await next();
            });

            // HSTS - Strict Transport Security
            app.UseHsts();

            app.UseHttpsRedirection();
            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = ctx =>
                {
                    ctx.Context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
                }
            });

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            app.MapRazorPages();

            app.Run();
        }
    }
}