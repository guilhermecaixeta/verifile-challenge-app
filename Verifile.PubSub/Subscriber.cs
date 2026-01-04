namespace Verifile.PubSub
{
    internal class Subscriber<TMessage> : ISubscriber<TMessage>
        where TMessage : class, new()
    {
        private const int MaxValue = 5_000;
        private const int MinValue = 1_000;

        public async Task<TMessage> SubscribeAsync(Func<TMessage, Task> handleMessage)
        {
            var random = new Random();

            await Task.Delay(TimeSpan.FromMilliseconds(random.Next(MinValue, MaxValue)));
            return new TMessage();
        }
    }
}
