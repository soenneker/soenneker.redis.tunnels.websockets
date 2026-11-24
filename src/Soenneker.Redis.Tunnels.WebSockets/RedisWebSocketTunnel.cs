using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using StackExchange.Redis;
using StackExchange.Redis.Configuration;

namespace Soenneker.Redis.Tunnels.WebSockets;

/// <summary>Connects StackExchange.Redis to a binary WebSocket relay over TLS.</summary>
public sealed class RedisWebSocketTunnel : Tunnel
{
    private readonly Uri _endpoint;
    private readonly string? _authorizationHeader;

    /// <param name="endpoint">The absolute wss URI of the relay.</param>
    /// <param name="bearerToken">An optional bearer token for the WebSocket handshake.</param>
    public RedisWebSocketTunnel(Uri endpoint, string? bearerToken = null)
    {
        ArgumentNullException.ThrowIfNull(endpoint);
        if (!endpoint.IsAbsoluteUri || endpoint.Scheme != "wss")
            throw new ArgumentException("The relay endpoint must be an absolute wss URI.", nameof(endpoint));

        _endpoint = endpoint;
        _authorizationHeader = bearerToken is null ? null : $"Bearer {bearerToken}";
    }

    public override ValueTask<EndPoint?> GetSocketConnectEndpointAsync(EndPoint endpoint, CancellationToken cancellationToken) => new((EndPoint?)null);

    public override async ValueTask<Stream?> BeforeAuthenticateAsync(EndPoint endpoint, ConnectionType connectionType, Socket? socket, CancellationToken cancellationToken)
    {
        var client = new ClientWebSocket();
        try
        {
            if (_authorizationHeader is not null)
                client.Options.SetRequestHeader("Authorization", _authorizationHeader);

            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(30));
            await client.ConnectAsync(_endpoint, timeout.Token).ConfigureAwait(false);
            return WebSocketStream.Create(client, WebSocketMessageType.Binary, ownsWebSocket: true);
        }
        catch
        {
            client.Dispose();
            throw;
        }
    }
}
