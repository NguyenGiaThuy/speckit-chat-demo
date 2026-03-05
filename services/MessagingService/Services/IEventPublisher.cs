namespace MessagingService.Services;

public interface IEventPublisher
{
    Task PublishAsync(Guid channelId, WsEvent evt);
}
