using chat;
using chat.Context;
using chat.Hubs;
using chat.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddSignalR();

builder.Services.AddDbContext<UserDbContext>(config =>
{
    config.UseInMemoryDatabase("UserDataBase");
});

builder.Services.AddIdentity<AppUser, IdentityRole>(config =>
{
    config.Password.RequiredLength = 3;
    config.Password.RequireDigit = false;
    config.Password.RequireNonAlphanumeric = false;
    config.Password.RequireUppercase = false;
}).AddEntityFrameworkStores<UserDbContext>()
  .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(config =>
{
    config.Cookie.Name = "ChatCookie";
    config.LoginPath = "/Chat/Login";
    config.ExpireTimeSpan = TimeSpan.FromDays(1);
});

builder.Services.AddControllersWithViews();

builder.WebHost.ConfigureServices((hostContext, services) => {
    services.AddHostedService<ChatWorker>();
});

var mvc = builder.Services.AddRazorPages().AddRazorRuntimeCompilation();

var app = builder.Build();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");

    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(name: "default", pattern: "{controller=Chat}/{action=Index}/{id?}");
});

app.MapHub<ChatHub>("/chatHub");

app.Run();