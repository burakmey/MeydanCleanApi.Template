using System.Globalization;
using Microsoft.AspNetCore.Localization;

namespace MeydanCleanApi.Template.WebApi.Configurations.Localization;

/// <summary>
/// Extension methods for configuring ASP.NET Core Request Localization middleware.
/// Reads active supported cultures from the database/provider at startup and handles fallbacks.
/// </summary>
public static class LocalizationExtensions
{
    private static readonly string[] FallbackCultures = [CultureConstants.TurkishCode, CultureConstants.EnglishUsCode];

    /// <summary>
    /// Reads active supported culture codes from <see cref="ISupportedCultureProvider"/> at application startup and configures request localization.
    /// </summary>
    public static async Task<IApplicationBuilder> UseApplicationRequestLocalizationAsync(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var cultureCodes = await ResolveCultureCodesAsync(app);

        var supportedCultures = cultureCodes
            .Select(code => new CultureInfo(code))
            .ToArray();

        var options = new RequestLocalizationOptions
        {
            DefaultRequestCulture = new RequestCulture(CultureConstants.DefaultCulture),
            SupportedCultures = supportedCultures,
            SupportedUICultures = supportedCultures,
            RequestCultureProviders =
            [
                new QueryStringRequestCultureProvider(),
                new CookieRequestCultureProvider(),
                new AcceptLanguageHeaderRequestCultureProvider()
            ]
        };

        return app.UseRequestLocalization(options);
    }

    private static async Task<IReadOnlyList<string>> ResolveCultureCodesAsync(WebApplication app)
    {
        var logger = app.Services.GetRequiredService<ILoggerFactory>()
            .CreateLogger(typeof(LocalizationExtensions));

        try
        {
            // The provider is scoped because it reads through DbContext, so it cannot be taken from
            // the root provider. Startup code has no request scope of its own, so we open one here.
            using var scope = app.Services.CreateScope();
            var provider = scope.ServiceProvider.GetRequiredService<ISupportedCultureProvider>();
            var codes = await provider.GetActiveCodesAsync();

            if (codes == null || codes.Count == 0)
            {
                logger.LogWarning(
                    "No active cultures found in SupportedCultures; falling back to {Cultures}.",
                    string.Join(", ", FallbackCultures));
                return FallbackCultures;
            }

            var valid = new List<string>();
            foreach (var code in codes)
            {
                try
                {
                    _ = CultureInfo.GetCultureInfo(code);
                    valid.Add(code);
                }
                catch (CultureNotFoundException)
                {
                    logger.LogWarning("Ignoring unknown culture code '{Code}' from SupportedCultures.", code);
                }
            }

            if (valid.Count > 0)
            {
                logger.LogInformation("Request localization enabled for cultures: {Cultures}.", string.Join(", ", valid));
                return valid;
            }

            logger.LogWarning("No usable culture codes found; falling back to {Cultures}.", string.Join(", ", FallbackCultures));
            return FallbackCultures;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Could not read SupportedCultures from database; falling back to {Cultures}.",
                string.Join(", ", FallbackCultures));
            return FallbackCultures;
        }
    }
}
