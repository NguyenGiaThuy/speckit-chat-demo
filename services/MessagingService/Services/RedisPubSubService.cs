using System.Text.Json;
using StackExchange.Redis;

namespace MessagingService.Services;

public class RedisPubSubService : IDisposable
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ISubscriber _subscriber;
    private readonly ConnectionManager _wsManager;

    public RedisPubSubService(IConnectionMultiplexer redis, ConnectionManager wsManager)
    {
        _redis = redis;
        _subscriber = redis.GetSubscriber();
        _wsManager = wsManager;
    }

    public async Task PublishAsync(Guid channelId, WsEvent evt)
    {
        var payload = JsonSerializer.Serialize(evt, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        });
        await _subscriber.PublishAsync(RedisChannel.Literal($"channel:{channelId}"), payload);
    }

    /// <summary>Called once at startup to begin listening for all channel events.</summary>
    public async Task StartSubscribingAsync()
    {
        await _subscriber.SubscribeAsync(RedisChannel.Pattern("channel:*"), (channel, message) =>
        {
            if (message.IsNull) return;

            // channel name is "channel:<guid>"
            var parts = ((string)channel!).Split(':', 2);
            if (parts.Length != 2 || !Guid.TryParse(parts[1], out var channelId)) return;

            _wsManager.BroadcastToChannel(channelId, message!);
        });
    }

    public void Dispose() => _redis.Dispose();
}
