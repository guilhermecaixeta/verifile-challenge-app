namespace Verifile.PubSub
{
    public interface IPublisher<TMessage>
        where TMessage : class
    {
        Task<bool> PublishAsync(TMessage message);
    }
}
