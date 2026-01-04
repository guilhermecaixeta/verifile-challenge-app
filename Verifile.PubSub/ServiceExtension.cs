using Microsoft.Extensions.DependencyInjection;

namespace Verifile.PubSub
{
    public static class ServiceExtension
    {
        public static IServiceCollection AddPubSub(this IServiceCollection services)
        {
            services.AddScoped(typeof(IPublisher<>), typeof(Publisher<>));
            services.AddScoped(typeof(ISubscriber<>), typeof(Subscriber<>));
            return services;
        }
    }
}
