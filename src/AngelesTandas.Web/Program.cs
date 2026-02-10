using AngelesTandas.Application;
using AngelesTandas.Infrastructure.Data;
using AngelesTandas.Infrastructure.Identity;
using AngelesTandas.Infrastructure.Services;
using AngelesTandas.Web.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;
using System.IO;

namespace AngelesTandas.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var config = builder.Configuration;
            var connStr = config.GetConnectionString("DefaultConnection") ?? 
                "Server=(localdb)\\mssqllocaldb;Database=AngelesTandas;Trusted_Connection=true;MultipleActiveResultSets=true";

            // Database
            builder.Services.AddDbContext<ApplicationDbContext>(opt => opt.UseSqlServer(connStr));

            // Identity with strong password policy
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                // Password policy
                options.Password.RequiredLength = 12;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredUniqueChars = 4;

                // Lockout policy
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // User policy
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = false;

                // Session timeout
                options.Tokens.ProviderMap.Add("Default", new TokenProviderDescriptor(typeof(IUserTwoFactorTokenProvider<ApplicationUser>)));
            })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // Blazor + Pages
            builder.Services.AddRazorPages();
            builder.Services.AddServerSideBlazor();
            builder.Services.AddControllers();

            // Services
            builder.Services.AddScoped<ITandaService, AngelesTandas.Infrastructure.Services.TandaService>();
            builder.Services.AddScoped<IPaymentService, AngelesTandas.Infrastructure.Services.PaymentService>();
            builder.Services.AddScoped<IAuditService, AngelesTandas.Infrastructure.Services.AuditService>();
            builder.Services.AddScoped<ISecurityService, AngelesTandas.Infrastructure.Services.SecurityService>();
            builder.Services.AddHttpContextAccessor();

            // Auth with security headers
            builder.Services.ConfigureApplicationCookie(opt =>
            {
                opt.SlidingExpiration = true;
                opt.ExpireTimeSpan = TimeSpan.FromHours(4); // Reduced from 8
                opt.Cookie.HttpOnly = true;
                opt.Cookie.SecurePolicy = CookieSecurePolicy.Always; // HTTPS only
                opt.Cookie.SameSite = SameSiteMode.Strict;
            });

            // HSTS
            builder.Services.AddHsts(options =>
            {
                options.MaxAge = TimeSpan.FromDays(365);
                options.IncludeSubDomains = true;
                options.Preload = true;
            });

            var app = builder.Build();

            // Seed database
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var auditService = scope.ServiceProvider.GetRequiredService<IAuditService>();

                db.Database.EnsureCreated();

                // Seed roles
                if (!roleManager.RoleExistsAsync("Admin").Result)
                {
                    roleManager.CreateAsync(new IdentityRole("Admin")).Wait();
                }
                if (!roleManager.RoleExistsAsync("User").Result)
                {
                    roleManager.CreateAsync(new IdentityRole("User")).Wait();
                }

                // Seed admin user with secure password from config or generate one
                var adminEmail = config["Seed:AdminEmail"] ?? "admin@local";
                var adminUser = userManager.FindByEmailAsync(adminEmail).Result;
                if (adminUser == null)
                {
                    // Generate secure password: at least 12 chars, mixed case, numbers, special chars
                    var pwd = config["Seed:AdminPassword"] ?? GenerateSecurePassword();
                    
                    adminUser = new ApplicationUser 
                    { 
                        UserName = adminEmail, 
                        Email = adminEmail, 
                        EmailConfirmed = true,
                        TwoFactorEnabled = true
                    };
                    
                    var result = userManager.CreateAsync(adminUser, pwd).Result;
                    if (result.Succeeded)
                    {
                        userManager.AddToRoleAsync(adminUser, "Admin").Wait();
                        
                        // Log admin account creation
                        auditService.LogActionAsync("system", "AdminUserCreated", "ApplicationUser", adminUser.Id, AngelesTandas.Domain.AuditSeverity.Warning).Wait();
                        
                        System.Console.WriteLine($"\n⚠️  ADMIN USER CREATED - SAVE THIS PASSWORD SECURELY:");
                        System.Console.WriteLine($"   Email: {adminEmail}");
                        System.Console.WriteLine($"   Temporary Password: {pwd}");
                        System.Console.WriteLine($"   (Password must be changed on first login)\n");
                    }
                    else
                    {
                        System.Console.WriteLine($"❌ Failed to create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }
            }

            // Pipeline
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseMiddleware<CurrentUserMiddleware>();

            // Upload endpoint with security
            app.MapPost("/api/payments/{paymentId:guid}/receipt", 
                async (Guid paymentId, HttpRequest request, IPaymentService paymentService, IHttpContextAccessor ctx) =>
                {
                    if (!request.HasFormContentType) return Results.BadRequest("multipart required");
                    var form = await request.ReadFormAsync();
                    var file = form.Files.FirstOrDefault();
                    if (file == null) return Results.BadRequest("no file");

                    // Validate file
                    var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf" };
                    var ext = Path.GetExtension(file.FileName).ToLower();
                    if (!validExtensions.Contains(ext)) 
                        return Results.BadRequest("Invalid file type");

                    if (file.Length > 10 * 1024 * 1024) // 10 MB max
                        return Results.BadRequest("File too large");

                    var userId = ctx.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system";
                    await using var stream = file.OpenReadStream();
                    var fileName = $"receipts/{paymentId}_{Path.GetFileName(file.FileName)}";

                    try
                    {
                        var blobUri = await paymentService.UploadStreamToBlobAsync(stream, fileName);
                        await paymentService.UploadReceiptAsync(paymentId, userId, blobUri);
                        return Results.Ok(new { Uri = blobUri });
                    }
                    catch (Exception ex)
                    {
                        return Results.BadRequest(ex.Message);
                    }
                }).RequireAuthorization().WithName("UploadReceipt");

            app.MapControllers();
            app.MapBlazorHub();
            app.MapFallbackToPage("/_Host");

            app.Run();
        }

        private static string GenerateSecurePassword()
        {
            const string lowerCase = "abcdefghijklmnopqrstuvwxyz";
            const string upperCase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string digits = "0123456789";
            const string special = "!@#$%^&*";

            var random = new Random();
            var password = new System.Text.StringBuilder();

            // Ensure at least one of each required type
            password.Append(lowerCase[random.Next(lowerCase.Length)]);
            password.Append(upperCase[random.Next(upperCase.Length)]);
            password.Append(digits[random.Next(digits.Length)]);
            password.Append(special[random.Next(special.Length)]);

            // Fill rest with random characters
            const string allChars = lowerCase + upperCase + digits + special;
            for (int i = 0; i < 8; i++)
            {
                password.Append(allChars[random.Next(allChars.Length)]);
            }

            // Shuffle
            var chars = password.ToString().ToCharArray();
            for (int i = chars.Length - 1; i > 0; i--)
            {
                int randomIndex = random.Next(i + 1);
                (chars[i], chars[randomIndex]) = (chars[randomIndex], chars[i]);
            }

            return new string(chars);
        }
    }
}