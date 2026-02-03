using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RackingSystem.Data;
using RackingSystem.Data.Maintenances;
using RackingSystem.Services;
using RackingSystem.Services.AccountServices;
using RackingSystem.Services.BOMServices;
using RackingSystem.Services.GRNServices;
using RackingSystem.Services.ItemServices;
using RackingSystem.Services.JOServices;
using RackingSystem.Services.LoaderServices;
using RackingSystem.Services.RackJobQueueServices;
using RackingSystem.Services.RackServices;
using RackingSystem.Services.ReelServices;
using RackingSystem.Services.SettingServices;
using RackingSystem.Services.SlotServices;
using RackingSystem.Services.TrolleyServices;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(option => 
option.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddDbContextFactory<AppDbContext>(options =>
     options.UseSqlServer(
         builder.Configuration.GetConnectionString("DefaultConnection"))
         , ServiceLifetime.Scoped
     );

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "MyAuthCookie";
    options.DefaultSignInScheme = "MyAuthCookie";
    options.DefaultChallengeScheme = "MyAuthCookie";
})
.AddCookie("MyAuthCookie", options =>
{
    options.Cookie.Name = "MyAuthCookie";
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";

    options.ExpireTimeSpan = TimeSpan.FromMinutes(1);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = SameSiteMode.Strict;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
    };
});

builder.Services.AddAuthorization();

builder.Services.AddRazorPages();
builder.Services.AddScoped<ISettingService, SettingService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<ISlotService, SlotService>();
builder.Services.AddScoped<ITrolleyService, TrolleyService>();
builder.Services.AddScoped<IItemService, ItemService>();
builder.Services.AddScoped<IReelService, ReelService>();
builder.Services.AddScoped<ILoaderService, LoaderService>();
builder.Services.AddScoped<IGRNService, GRNService>();
builder.Services.AddScoped<IBOMService, BOMService>();
builder.Services.AddScoped<IJOService, JOService>();
builder.Services.AddScoped<IRackJobQueueService, RackJobQueueService>();
builder.Services.AddScoped<IRackService, RackService>();

builder.Services.AddIdentityApiEndpoints<User>(options =>
{
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddAutoMapper(cfg => cfg.AddProfile<AutoMapperProfile>(), AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(3600); 
    options.Cookie.HttpOnly = true;
    //options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

var app = builder.Build();

await SeedService.SeedDatabase(app.Services);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.MapIdentityApi<User>();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
