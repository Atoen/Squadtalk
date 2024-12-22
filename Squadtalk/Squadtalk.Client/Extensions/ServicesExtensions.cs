using Refit;
using Squadtalk.Client.Network;

namespace Squadtalk.Client.Extensions;

public static class ServicesExtensions
{
    public static IServiceCollection AddNetworking(this IServiceCollection serviceCollection, string baseAddress)
    {
        serviceCollection.AddSingleton(_ => new HttpClient
        {
            BaseAddress = new Uri(baseAddress)
        });

        serviceCollection
            .AddApi<IAccountApi>()
            .AddApi<IChatApi>();

        return serviceCollection;
    }

    private static IServiceCollection AddApi<T>(this IServiceCollection serviceCollection) where T : class
    {
        serviceCollection.AddSingleton(provider =>
        {
            var client = provider.GetRequiredService<HttpClient>();
            return RestService.For<T>(client, new RefitSettings
            {
                ExceptionFactory = _ => Task.FromResult<Exception?>(null)
            });
        });

        return serviceCollection;
    }
}
