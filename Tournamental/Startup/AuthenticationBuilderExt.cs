using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.Extensions.Options;
using Tournamental.Infrastructure.Options;

namespace Tournamental.Startup;

public static class AuthenticationBuilderExt
{
    public static void AddDiscordAuth(this AuthenticationBuilder builder)
    {
        builder.Services.AddOptions<OAuthOptions>("Discord").Configure<IOptions<DiscordAuthOptions>>((options, discordAuthOptions) => 
        {
            var discordOptions = discordAuthOptions.Value;
            
            options.ClientId = discordOptions.ClientId;
            options.ClientSecret = discordOptions.ClientSecret;
            options.CallbackPath = new PathString("/signin-discord");

            options.AuthorizationEndpoint = "https://discord.com/api/oauth2/authorize";
            options.TokenEndpoint = "https://discord.com/api/oauth2/token";
            options.UserInformationEndpoint = "https://discord.com/api/users/@me";

            options.Scope.Add("identify");
            options.Scope.Add("connections");
        });
        
        builder.AddOAuth("Discord", options =>
        {
            options.Events.OnCreatingTicket = async context =>
            {
                var userRequest = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
                userRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);
                var userResponse = await context.Backchannel.SendAsync(userRequest,
                    HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted);
                userResponse.EnsureSuccessStatusCode();

                var connectionsRequest = new HttpRequestMessage(HttpMethod.Get,
                    context.Options.UserInformationEndpoint + "/connections");
                connectionsRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);
                var connectionsResponse = await context.Backchannel.SendAsync(connectionsRequest,
                    HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted);
                connectionsResponse.EnsureSuccessStatusCode();

                var user = JsonDocument.Parse(await userResponse.Content.ReadAsStringAsync());
                context.Identity.AddClaim(new Claim(ClaimTypes.NameIdentifier,
                    user.RootElement.GetProperty("id").GetString()));
                context.Identity.AddClaim(new Claim(ClaimTypes.Name,
                    user.RootElement.GetProperty("username").GetString()));

                var connections = JsonDocument.Parse(await connectionsResponse.Content.ReadAsStringAsync());
                Console.Write(connections);
            };
        });
    }
}