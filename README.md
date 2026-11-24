[![](https://img.shields.io/nuget/v/soenneker.redis.tunnels.websockets.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.redis.tunnels.websockets/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.redis.tunnels.websockets/build-and-test.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.redis.tunnels.websockets/actions/workflows/build-and-test.yml)
[![](https://img.shields.io/nuget/dt/soenneker.redis.tunnels.websockets.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.redis.tunnels.websockets/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.redis.tunnels.websockets/codeql.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.redis.tunnels.websockets/actions/workflows/codeql.yml)

# ![](https://user-images.githubusercontent.com/4441470/224455560-91ed3ee7-f510-4041-a8d2-3fc093025112.png) Soenneker.Redis.Tunnels.WebSockets
### A WebSocket tunnel for StackExchange.Redis connections.

## Installation

```
dotnet add package Soenneker.Redis.Tunnels.WebSockets
```

## Usage

Use this tunnel when a relay accepts binary WebSocket connections and forwards their bytes to a Redis-compatible server. Start with your usual StackExchange.Redis connection string, then assign a `RedisWebSocketTunnel` to `ConfigurationOptions.Tunnel` before connecting.

For example, supply these settings through your application's configuration or environment:

| Environment variable | Example or purpose |
| --- | --- |
| `REDIS_CONNECTION_STRING` | `redis.internal.example:6379,ssl=true,sslHost=redis.internal.example,password=<redis-password>`. Use the backend's certificate name for `sslHost`. |
| `REDIS_RELAY_URL` | Full WebSocket endpoint, such as `wss://relay.example.com/garnet`. |
| `REDIS_RELAY_TOKEN` | Optional bearer token for the relay handshake. Supply the raw token and the tunnel adds the `Bearer` prefix. |

```csharp
using System;
using Soenneker.Redis.Tunnels.WebSockets;
using StackExchange.Redis;

var options = ConfigurationOptions.Parse(Required("REDIS_CONNECTION_STRING"));
options.Tunnel = new RedisWebSocketTunnel(
    new Uri(Required("REDIS_RELAY_URL")),
    Environment.GetEnvironmentVariable("REDIS_RELAY_TOKEN"));

using var redis = await ConnectionMultiplexer.ConnectAsync(options);
var latency = await redis.GetDatabase().PingAsync();
Console.WriteLine($"Redis replied in {latency.TotalMilliseconds:F1} ms");

static string Required(string name) =>
    Environment.GetEnvironmentVariable(name)
    ?? throw new InvalidOperationException($"Set {name} before connecting.");
```

The `wss://` connection secures the hop to the relay. In this example, `ssl=true` also establishes TLS with Redis through the tunnel, so the client must trust the Redis certificate. The Redis password authenticates to the backend. In a long-running application, create the multiplexer once and reuse it across requests.
