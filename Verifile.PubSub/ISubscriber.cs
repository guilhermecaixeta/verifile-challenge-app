namespace Verifile.PubSub
{
    public interface ISubscriber<TMessage>
        where TMessage : class
    {
        Task<TMessage> SubscribeAsync(Func<TMessage, Task> handleMessage);
    }
}
