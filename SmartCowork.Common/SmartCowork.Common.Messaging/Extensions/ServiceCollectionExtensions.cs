// Extensions/ServiceCollectionExtensions.cs
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartCowork.Common.Messaging.RabbitMQ;

namespace SmartCowork.Common.Messaging.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRabbitMQProducer(this IServiceCollection services)
        {
            services.AddSingleton<IRabbitMQProducer, RabbitMQProducer>();
            return services;
        }
    }
}