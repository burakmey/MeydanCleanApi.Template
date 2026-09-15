using MeydanCleanApi.Template.WebApi.Configurations.StartupChecks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace MeydanCleanApi.Template.Tests.WebApi;

/// <summary>
/// Covers the guard that stops the API starting with a signing key published in this repository.
/// </summary>
public sealed class ProductionSecretGuardTests
{
    private const string PublishedJwtKey = "Development_JwtSecurityKey_Minimum_32_Chars_Long_Secret_Key!";
    private const string PublishedStorageKey = "Docker_Local_StorageSigningKey_Minimum_32_Chars_Long_Key!";
    private const string RealKey = "3PnJ8xQ2vW5zB7cL1mR4tY6uI9oP0aS2dF4gH6jK8lZ";

    private static IHost BuildHost(string environmentName, Dictionary<string, string?> settings)
    {
        return new HostBuilder()
            .UseEnvironment(environmentName)
            .ConfigureAppConfiguration(builder => builder.AddInMemoryCollection(settings))
            .Build();
    }

    private static Dictionary<string, string?> Settings(string jwtKey, string refreshKey, string storageKey) => new()
    {
        ["Tokens:Jwt:JwtSecurityKey"] = jwtKey,
        ["Tokens:Jwt:RefreshSecurityKey"] = refreshKey,
        ["Storage:Local:SigningKey"] = storageKey,
    };

    [Fact]
    public void InDevelopment_ThePublishedKeysAreFine()
    {
        // The whole point of shipping them is that a clone runs unchanged.
        using var host = BuildHost(Environments.Development, Settings(PublishedJwtKey, PublishedJwtKey, PublishedStorageKey));

        host.EnsureProductionSecretsAreReal();
    }

    [Fact]
    public void InProduction_APublishedSigningKeyStopsStartup()
    {
        using var host = BuildHost(Environments.Production, Settings(PublishedJwtKey, RealKey, RealKey));

        var exception = Assert.Throws<InvalidOperationException>(host.EnsureProductionSecretsAreReal);

        // The message has to name the key, because the operator's next move is to replace it.
        Assert.Contains("Tokens:Jwt:JwtSecurityKey", exception.Message, StringComparison.Ordinal);
        Assert.Contains("Production", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void InProduction_APublishedStorageKeyStopsStartupToo()
    {
        using var host = BuildHost(Environments.Production, Settings(RealKey, RealKey, PublishedStorageKey));

        var exception = Assert.Throws<InvalidOperationException>(host.EnsureProductionSecretsAreReal);

        Assert.Contains("Storage:Local:SigningKey", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void InProduction_EveryOffendingKeyIsNamedAtOnce()
    {
        // Naming one at a time would mean restarting to discover the next.
        using var host = BuildHost(Environments.Production, Settings(PublishedJwtKey, PublishedJwtKey, PublishedStorageKey));

        var exception = Assert.Throws<InvalidOperationException>(host.EnsureProductionSecretsAreReal);

        Assert.Contains("Tokens:Jwt:JwtSecurityKey", exception.Message, StringComparison.Ordinal);
        Assert.Contains("Tokens:Jwt:RefreshSecurityKey", exception.Message, StringComparison.Ordinal);
        Assert.Contains("Storage:Local:SigningKey", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void InProduction_RealKeysPass()
    {
        using var host = BuildHost(Environments.Production, Settings(RealKey, RealKey, RealKey));

        host.EnsureProductionSecretsAreReal();
    }

    [Fact]
    public void InStaging_ThePublishedKeysAreStillRefused()
    {
        // Anything that is not Development is somewhere other people can reach.
        using var host = BuildHost("Staging", Settings(PublishedJwtKey, RealKey, RealKey));

        Assert.Throws<InvalidOperationException>(host.EnsureProductionSecretsAreReal);
    }

    [Fact]
    public void AnUnsetKey_IsNotTreatedAsPublished()
    {
        // Missing configuration is a different failure, reported by the options validation at startup.
        using var host = BuildHost(Environments.Production, new Dictionary<string, string?>());

        host.EnsureProductionSecretsAreReal();
    }
}
