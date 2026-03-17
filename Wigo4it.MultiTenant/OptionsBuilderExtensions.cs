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
    /// Registreert de dependency injection container om <typeparamref name="TOptions"/> te binden tegen
    /// de tenant-specifieke <see cref="IConfiguration"/>.
    /// </summary>
    /// <typeparam name="TOptions">Het type van de opties dat moet worden geconfigureerd.</typeparam>
    /// <param name="optionsBuilder">De opties builder waaraan de services moeten worden toegevoegd.</param>
    /// <param name="configSectionPath">De naam van de configuratiesectie waarvan moet worden gebonden.</param>
    /// <param name="configureBinder">Optioneel. Wordt gebruikt om de <see cref="BinderOptions"/> in te stellen.</param>
    /// <returns>De <see cref="OptionsBuilder{TOptions}"/> zodat aanvullende aanroepen kunnen worden geketend.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="optionsBuilder"/> of <paramref name="configSectionPath"/> is <see langword="null"/>.
    /// </exception>
    public static OptionsBuilder<TOptions> BindConfigurationPerTenant<TOptions>(
        this OptionsBuilder<TOptions> optionsBuilder,
        string configSectionPath,
        Action<BinderOptions>? configureBinder = null
    )
        where TOptions : class
    {
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
