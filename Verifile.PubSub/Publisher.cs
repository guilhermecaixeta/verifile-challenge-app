namespace Verifile.PubSub
{
    internal class Publisher<TMessage> : IPublisher<TMessage>
        where TMessage : class
    {
        private const int MaxValue = 5_000;
        private const int MinValue = 1_000;

        public async Task<bool> PublishAsync(TMessage message)
        {
            var random = new Random();

            var value = random.Next(MinValue, MaxValue);
            await Task.Delay(TimeSpan.FromMilliseconds(value));
            return value % 2 == 0;
        }
    }
}
