using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Wigo4it.MultiTenant.MultiTenantHttpClient;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IHttpClientBuilder AddMultiTenantHttpClient(string name) =>
            services.AddHttpClient(name).AddMultiTenantHeadersHandler();

        public IHttpClientBuilder AddMultiTenantHttpClient(string name, Action<HttpClient> configureClient) =>
            services.AddHttpClient(name, configureClient).AddMultiTenantHeadersHandler();

        public IHttpClientBuilder AddMultiTenantHttpClient(string name, Action<IServiceProvider, HttpClient> configureClient) =>
            services.AddHttpClient(name, configureClient).AddMultiTenantHeadersHandler();

        public IHttpClientBuilder AddMultiTenantHttpClient<TClient>()
            where TClient : class => services.AddHttpClient<TClient>().AddMultiTenantHeadersHandler();

        public IHttpClientBuilder AddMultiTenantHttpClient<TClient>(Action<HttpClient> configureClient)
            where TClient : class => services.AddHttpClient<TClient>(configureClient).AddMultiTenantHeadersHandler();

        public IHttpClientBuilder AddMultiTenantHttpClient<TClient>(Action<IServiceProvider, HttpClient> configureClient)
            where TClient : class => services.AddHttpClient<TClient>(configureClient).AddMultiTenantHeadersHandler();

        public IHttpClientBuilder AddMultiTenantHttpClient<TClient>(string name)
            where TClient : class => services.AddHttpClient<TClient>(name).AddMultiTenantHeadersHandler();

        public IHttpClientBuilder AddMultiTenantHttpClient<TClient>(string name, Action<HttpClient> configureClient)
            where TClient : class => services.AddHttpClient<TClient>(name, configureClient).AddMultiTenantHeadersHandler();

        public IHttpClientBuilder AddMultiTenantHttpClient<TClient>(
            string name,
            Action<IServiceProvider, HttpClient> configureClient
        )
            where TClient : class => services.AddHttpClient<TClient>(name, configureClient).AddMultiTenantHeadersHandler();

        public IHttpClientBuilder AddMultiTenantHttpClient<TClient, TImplementation>()
            where TClient : class
            where TImplementation : class, TClient =>
            services.AddHttpClient<TClient, TImplementation>().AddMultiTenantHeadersHandler();

        public IHttpClientBuilder AddMultiTenantHttpClient<TClient, TImplementation>(Action<HttpClient> configureClient)
            where TClient : class
            where TImplementation : class, TClient =>
            services.AddHttpClient<TClient, TImplementation>(configureClient).AddMultiTenantHeadersHandler();

        public IHttpClientBuilder AddMultiTenantHttpClient<TClient, TImplementation>(
            Action<IServiceProvider, HttpClient> configureClient
        )
            where TClient : class
            where TImplementation : class, TClient =>
            services.AddHttpClient<TClient, TImplementation>(configureClient).AddMultiTenantHeadersHandler();

        public IHttpClientBuilder AddMultiTenantHttpClient<TClient, TImplementation>(Func<HttpClient, TImplementation> factory)
            where TClient : class
            where TImplementation : class, TClient =>
            services.AddHttpClient<TClient, TImplementation>(factory).AddMultiTenantHeadersHandler();

        public IHttpClientBuilder AddMultiTenantHttpClient<TClient, TImplementation>(
            Func<HttpClient, IServiceProvider, TImplementation> factory
        )
            where TClient : class
            where TImplementation : class, TClient =>
            services.AddHttpClient<TClient, TImplementation>(factory).AddMultiTenantHeadersHandler();

        public IHttpClientBuilder AddMultiTenantHttpClient<TClient, TImplementation>(string name)
            where TClient : class
            where TImplementation : class, TClient =>
            services.AddHttpClient<TClient, TImplementation>(name).AddMultiTenantHeadersHandler();

        public IHttpClientBuilder AddMultiTenantHttpClient<TClient, TImplementation>(
            string name,
            Action<HttpClient> configureClient
        )
            where TClient : class
            where TImplementation : class, TClient =>
            services.AddHttpClient<TClient, TImplementation>(name, configureClient).AddMultiTenantHeadersHandler();

        public IHttpClientBuilder AddMultiTenantHttpClient<TClient, TImplementation>(
            string name,
            Action<IServiceProvider, HttpClient> configureClient
        )
            where TClient : class
            where TImplementation : class, TClient =>
            services.AddHttpClient<TClient, TImplementation>(name, configureClient).AddMultiTenantHeadersHandler();

        public IHttpClientBuilder AddMultiTenantHttpClient<TClient, TImplementation>(
            string name,
            Func<HttpClient, TImplementation> factory
        )
            where TClient : class
            where TImplementation : class, TClient =>
            services.AddHttpClient<TClient, TImplementation>(name, factory).AddMultiTenantHeadersHandler();

        public IHttpClientBuilder AddMultiTenantHttpClient<TClient, TImplementation>(
            string name,
            Func<HttpClient, IServiceProvider, TImplementation> factory
        )
            where TClient : class
            where TImplementation : class, TClient =>
            services.AddHttpClient<TClient, TImplementation>(name, factory).AddMultiTenantHeadersHandler();
    }

    private static IHttpClientBuilder AddMultiTenantHeadersHandler(this IHttpClientBuilder builder)
    {
        builder.Services.TryAddTransient<MultiTenantHeadersDelegatingHandler>();
        return builder.AddHttpMessageHandler<MultiTenantHeadersDelegatingHandler>();
    }
}
