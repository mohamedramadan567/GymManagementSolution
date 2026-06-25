using GymManagement.BLL;
using GymManagement.BLL.Services.Attachment;
using GymManagement.BLL.Services.Classes;
using GymManagement.BLL.Services.Interfaces;
using GymManagement.DAL.Data.DataSeeding;
using GymManagement.DAL.Data.DbContexts;
using GymManagement.DAL.Repositories.Classes;
using GymManagement.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace GymManagement
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            //builder.Services.AddScoped<IPlanRepository, PlanRepository>();
            builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            builder.Services.AddScoped<IMemberService, MemberService>();
            builder.Services.AddScoped<IPlanService, PlanService>();
            builder.Services.AddScoped<ITrainerService, TrainerService>();
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<ISessionRepository, SessionRepository>();
            builder.Services.AddScoped<ISessionService, SessionService>();
            builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
            builder.Services.AddScoped<IMembershipRepository, MembershipRepository>();
            builder.Services.AddScoped<IMembershipService, MembershipService>();
            builder.Services.AddScoped<IAttachmentService, AttachmentService>();
            builder.Services.AddAutoMapper(m => m.AddProfile(new MappingProfile()));
            builder.Services.AddDbContext<GymDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            });

            var app = builder.Build();

            var scope = app.Services.CreateScope();
            var _context = scope.ServiceProvider.GetRequiredService<GymDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

            var folderPath = Path.Combine(app.Environment.ContentRootPath, "wwwroot", "Files");
            var penddingMigrations = await _context.Database.GetPendingMigrationsAsync();
            if(penddingMigrations.Any())
            {
                await _context.Database.MigrateAsync();
            }

            await GymDataSeeding.SeedAsync(_context, folderPath, logger);

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}
