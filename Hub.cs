using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace signalr_test
{
    //    System.InvalidOperationException: No authentication handler is registered for the scheme 'Cookies'.
    //    The registered schemes are: Identity.Application, Identity.External, Identity.TwoFactorRememberMe, Identity.TwoFactorUserId, idsrv, idsrv.external, IdentityServerJwt, IdentityServerJwtBearer.
    //    Did you forget to call AddAuthentication().Add[SomeAuthHandler] ("Cookies",...)?

    //[Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    //[Authorize(AuthenticationSchemes = "IdentityServerJwt")]
    //[Authorize]
    public class NewsHub : Hub
    {
        public static readonly SortedDictionary<string, HubAuthItem> Connected = new SortedDictionary<string, HubAuthItem>();

        public override Task OnConnectedAsync()
        {
            Debug.WriteLine($"NewsHub OnConnected: {Context.ConnectionId}");

            NewsHub.Connected.Add(Context.ConnectionId, new HubAuthItem { ConnectionId = Context.ConnectionId, LastConnect = DateTime.Now });

            return base.OnConnectedAsync();
        }
        public override Task OnDisconnectedAsync(Exception exception)
        {
            Debug.WriteLine($"NewsHub OnDisconnected: {Context.ConnectionId}");

            Connected.Remove(Context.ConnectionId);

            return base.OnDisconnectedAsync(exception);
        }
    }

    public class HubAuthItem
    {
        public string UserId { get; set; }
        public string ConnectionId { get; set; }
        public DateTime LastConnect { get; set; }
    }
}
