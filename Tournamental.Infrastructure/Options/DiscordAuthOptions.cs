namespace Tournamental.Infrastructure.Options;

public class DiscordAuthOptions
{
    public required string ClientId { get; init; }
    public required string ClientSecret { get; init; }
}