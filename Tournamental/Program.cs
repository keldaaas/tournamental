using Microsoft.AspNetCore.Authentication.Cookies;
using Tournamental;
using Tournamental.Infrastructure.Options;
using Tournamental.Startup;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Options
builder.Services.Configure<DiscordAuthOptions>(builder.Configuration.GetSection("DiscordAuth"));

// Services
builder.Services.AddAuthentication(configureOptions: options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultAuthenticateScheme = "Discord";
        options.DefaultChallengeScheme = "Discord";
    })
    .AddCookie()
    .AddDiscordAuth();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();