using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.Cookies;
using Tournamental;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultAuthenticateScheme = "Discord";
        options.DefaultChallengeScheme = "Discord";
    })
    .AddCookie()
    .AddOAuth("Discord", options =>
    {
        options.ClientId = "";
        options.ClientSecret = "";
        options.CallbackPath = new PathString("/signin-discord");

        options.AuthorizationEndpoint = "https://discord.com/api/oauth2/authorize";
        options.TokenEndpoint = "https://discord.com/api/oauth2/token";
        options.UserInformationEndpoint = "https://discord.com/api/users/@me";

        options.Scope.Add("identify");
        options.Scope.Add("connections");

        options.Events.OnCreatingTicket = async context =>
        {
            var userRequest = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
            userRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);
            var userResponse = await context.Backchannel.SendAsync(userRequest, HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted);
            userResponse.EnsureSuccessStatusCode();
            
            var connectionsRequest = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint + "/connections");
            connectionsRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);
            var connectionsResponse = await context.Backchannel.SendAsync(connectionsRequest, HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted);
            connectionsResponse.EnsureSuccessStatusCode();

            var user = JsonDocument.Parse(await userResponse.Content.ReadAsStringAsync());
            context.Identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.RootElement.GetProperty("id").GetString()));
            context.Identity.AddClaim(new Claim(ClaimTypes.Name, user.RootElement.GetProperty("username").GetString()));
            
            var connections = JsonDocument.Parse(await connectionsResponse.Content.ReadAsStringAsync());
            Console.Write(connections);
        };
    });

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