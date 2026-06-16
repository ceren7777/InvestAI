using InvestAI.Models;
using InvestAI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient<WorldBankService>();
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// Google OAuth
//builder.Services.AddAuthentication()
 //   .AddGoogle(options =>
   // {
     //   options.ClientId = builder.Configuration["Google:ClientId"]!;
       // options.ClientSecret = builder.Configuration["Google:ClientSecret"]!;
        //options.CallbackPath = "/signin-google";
   // });

// Cookie yönlendirmesi
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/Login";
});

builder.Services.AddHttpClient();
builder.Services.AddScoped<WikidataService>();
builder.Services.AddScoped<IInvestmentScoringService, InvestmentScoringService>();
builder.Services.AddHttpClient<IWikidataService, WikidataService>();
builder.Services.AddHostedService<RegionDataUpdateService>();
builder.Services.AddHttpClient<OverpassService>();
builder.Services.AddHttpClient<TcmbService>();
builder.Services.AddHttpClient<WeatherService>();
builder.Services.AddHttpClient<GoogleMapsService>();
builder.Services.AddHttpClient<SentimentService>();
builder.Services.AddScoped<InvestAnalysisService>();
builder.Services.AddHttpClient<OllamaAgentService>(client =>
{
    client.Timeout = TimeSpan.FromMinutes(5);
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication(); // Bu satýr eklendi
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}"); // Login sayfasýna yönlendir

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();
    var env = services.GetRequiredService<IWebHostEnvironment>();
    SeedData.LoadDistrictPopulations(context, env);
}

app.Run();