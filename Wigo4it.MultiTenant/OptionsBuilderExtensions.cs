using Finbuckle.MultiTenant.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Wigo4it.MultiTenant;

public static class OptionsBuilderExtensions
{
    /// <summary>
    /// Multi-tenant versie van <see cref="OptionsBuilderConfigurationExtensions.BindConfiguration{TOptions}(OptionsBuilder{TOptions}, IConfiguration, Action{BinderOptions})"/>.
    ///
    /// Registeerd de dependency injection container om <typeparamref name="TOptions"/> te binden tegen
    /// de tenant-specifieke <see cref="IConfiguration"/>
    /// </summary>
    /// <typeparam name="TOptions">The options type to be configured.</typeparam>
    /// <param name="optionsBuilder">The options builder to add the services to.</param>
    /// <param name="configSectionPath">The name of the configuration section to bind from.</param>
    /// <param name="configureBinder">Optional. Used to configure the <see cref="BinderOptions"/>.</param>
    /// <returns>The <see cref="OptionsBuilder{TOptions}"/> so that additional calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="optionsBuilder"/> or <paramref name="configSectionPath" /> is <see langword="null"/>.
    /// </exception>
    /// <seealso cref="Bind{TOptions}(OptionsBuilder{TOptions}, IConfiguration, Action{BinderOptions})"/>
    public static OptionsBuilder<TOptions> BindConfigurationPerTenant<TOptions>(
        this OptionsBuilder<TOptions> optionsBuilder,
        string configSectionPath,
        Action<BinderOptions>? configureBinder = null
    )
        where TOptions : class
    {
        // Implementatie Gebaseerd op OptionsBuilderConfigurationExtensions.BindConfiguration(), maar dan met ConfigurePerTenant ipv Configure.

        ArgumentNullException.ThrowIfNull(optionsBuilder);
        ArgumentNullException.ThrowIfNull(configSectionPath);

        optionsBuilder.ConfigurePerTenant<TOptions, Wigo4itTenantInfo>(
            (opts, info) =>
            {
                IConfiguration section = string.Equals("", configSectionPath, StringComparison.OrdinalIgnoreCase)
                    ? info.Configuration
                    : info.Configuration.GetSection(configSectionPath);
                section.Bind(opts, configureBinder);
            }
        );

        optionsBuilder.Services.AddSingleton<IOptionsChangeTokenSource<TOptions>, ConfigurationChangeTokenSource<TOptions>>(sp =>
        {
            return new ConfigurationChangeTokenSource<TOptions>(optionsBuilder.Name, sp.GetRequiredService<IConfiguration>());
        });

        return optionsBuilder;
    }
}
