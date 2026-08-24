using Microsoft.JSInterop;
using System.Net.Http.Headers;

namespace POS.Client.Auth;

public class AuthorizationMessageHandler(IJSRuntime js) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await js.InvokeAsync<string?>("localStorage.getItem", TokenAuthenticationStateProvider.TokenKey);
        if (!string.IsNullOrWhiteSpace(token)) request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await base.SendAsync(request, cancellationToken);
    }
}
