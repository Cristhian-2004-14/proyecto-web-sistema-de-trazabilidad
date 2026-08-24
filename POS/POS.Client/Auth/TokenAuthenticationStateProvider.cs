using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Security.Claims;
using System.Text.Json;

namespace POS.Client.Auth;

public class TokenAuthenticationStateProvider(IJSRuntime js) : AuthenticationStateProvider
{
    public const string TokenKey = "palmaVerdeToken";
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await js.InvokeAsync<string?>("localStorage.getItem", TokenKey);
        return new AuthenticationState(new ClaimsPrincipal(string.IsNullOrWhiteSpace(token) ? new ClaimsIdentity() : new ClaimsIdentity(ParseClaims(token), "jwt")));
    }
    public async Task SetTokenAsync(string? token)
    {
        if (string.IsNullOrWhiteSpace(token)) await js.InvokeVoidAsync("localStorage.removeItem", TokenKey);
        else await js.InvokeVoidAsync("localStorage.setItem", TokenKey, token);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
    private static IEnumerable<Claim> ParseClaims(string token)
    {
        var payload = token.Split('.')[1].Replace('-', '+').Replace('_', '/');
        payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
        var values = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(Convert.FromBase64String(payload)) ?? [];
        foreach (var value in values)
        {
            var type = value.Key switch { "role" => ClaimTypes.Role, "unique_name" => ClaimTypes.Name, "nameid" => ClaimTypes.NameIdentifier, _ => value.Key };
            yield return new Claim(type, value.Value.ToString());
        }
    }
}
