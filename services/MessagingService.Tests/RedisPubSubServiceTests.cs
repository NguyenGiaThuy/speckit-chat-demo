using MessagingService.Services;
using Moq;
using StackExchange.Redis;

namespace MessagingService.Tests;

public class RedisPubSubServiceTests
{
    [Fact]
    public async Task PublishAsync_SendsMessageToCorrectChannel()
    {
        var channelId = Guid.NewGuid();
        var capturedChannel = string.Empty;
        var capturedMessage = string.Empty;

        var subscriberMock = new Mock<ISubscriber>();
        subscriberMock
            .Setup(s => s.PublishAsync(
                It.IsAny<RedisChannel>(),
                It.IsAny<RedisValue>(),
                It.IsAny<CommandFlags>()))
            .Callback<RedisChannel, RedisValue, CommandFlags>((ch, msg, _) =>
            {
                capturedChannel = ch.ToString();
                capturedMessage = msg.ToString();
            })
            .ReturnsAsync(0);

        var redisMock = new Mock<IConnectionMultiplexer>();
        redisMock.Setup(r => r.GetSubscriber(It.IsAny<object>())).Returns(subscriberMock.Object);

        // We need to provide a ConnectionManager too
        var connManager = new ConnectionManager();
        var pubSub = new RedisPubSubService(redisMock.Object, connManager);

        var evt = new WsEvent("message.created", new { id = Guid.NewGuid() });
        await pubSub.PublishAsync(channelId, evt);

        Assert.Equal($"channel:{channelId}", capturedChannel);
        Assert.Contains("message.created", capturedMessage);
    }

    [Fact]
    public async Task PublishAsync_MessageContainsEventType()
    {
        var subscriberMock = new Mock<ISubscriber>();
        var messages = new List<string>();

        subscriberMock
            .Setup(s => s.PublishAsync(
                It.IsAny<RedisChannel>(),
                It.IsAny<RedisValue>(),
                It.IsAny<CommandFlags>()))
            .Callback<RedisChannel, RedisValue, CommandFlags>((_, msg, _) =>
                messages.Add(msg.ToString()))
            .ReturnsAsync(0);

        var redisMock = new Mock<IConnectionMultiplexer>();
        redisMock.Setup(r => r.GetSubscriber(It.IsAny<object>())).Returns(subscriberMock.Object);

        var pubSub = new RedisPubSubService(redisMock.Object, new ConnectionManager());

        await pubSub.PublishAsync(Guid.NewGuid(), new WsEvent("message.updated", new { id = Guid.NewGuid() }));
        await pubSub.PublishAsync(Guid.NewGuid(), new WsEvent("message.deleted", new { id = Guid.NewGuid() }));

        Assert.Contains(messages, m => m.Contains("message.updated"));
        Assert.Contains(messages, m => m.Contains("message.deleted"));
    }
}
