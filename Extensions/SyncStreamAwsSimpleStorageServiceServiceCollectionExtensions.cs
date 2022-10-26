using Microsoft.Extensions.DependencyInjection;
using SyncStream.Aws.S3.Client.Configuration;
using SyncStream.Serializer;

// Define our namespace
namespace SyncStream.Aws.S3.Client.Extensions;

/// <summary>
///
/// </summary>
public static class SyncStreamAwsSimpleStorageServiceServiceCollectionExtensions
{
    /// <summary>
    ///     This method globally configures the SyncStream AWS S3 service and client with <paramref name="configuration" />
    /// </summary>
    /// <param name="instance">The current IServiceCollection instance</param>
    /// <param name="configuration">The configuration values to use</param>
    /// <returns><paramref name="instance" /></returns>
    public static IServiceCollection UseGlobalSyncStreamAwsSimpleStorageService(this IServiceCollection instance,
        AwsSimpleStorageServiceClientConfiguration configuration)
    {
        // Configure the AWS S3 service globally
        AwsSimpleStorageServiceClient.WithConfiguration(configuration);

        // We're done, return the instance
        return instance;
    }

    /// <summary>
    ///     This method globally configures the SyncStream AWS S3 service and client
    /// </summary>
    /// <param name="instance">The current IServiceCollection</param>
    /// <param name="accessKeyId">The AWS authentication aws_access_key_id</param>
    /// <param name="secretAccessKey">The AWS authentication secret_access_key</param>
    /// <param name="region">Optional, AWS region</param>
    /// <param name="kmsKeyId">Optional, AWS Key Management Service key ID for encrypting objects</param>
    /// <param name="format">Optional, preferred serialization format</param>
    /// <returns><paramref name="instance" /></returns>
    public static IServiceCollection UseGlobalSyncStreamAwsSimpleStorageService(this IServiceCollection instance,
        string accessKeyId, string secretAccessKey, string region = null, string kmsKeyId = null,
        SerializerFormat format = SerializerFormat.Json) => UseGlobalSyncStreamAwsSimpleStorageService(instance,
        new(accessKeyId, secretAccessKey, region ?? "us-east-1", kmsKeyId, format));

    /// <summary>
    ///     This method configures and registers a SyncStream AWS S3 singleton service with <paramref name="configuration" />
    /// </summary>
    /// <param name="instance">The current IServiceCollection instance</param>
    /// <param name="configuration">The configuration values to use</param>
    /// <returns><paramref name="instance" /></returns>
    public static IServiceCollection UseSyncStreamAwsSimpleStorageService(this IServiceCollection instance,
        AwsSimpleStorageServiceClientConfiguration configuration)
    {
        // Configure and register our singleton service
        instance.AddSingleton<IAwsSimpleStorageService, AwsSimpleStorageService>(_ => new(configuration));

        // We're done, return the instance
        return instance;
    }

    /// <summary>
    ///     This method configures and registers a SyncStream AWS S3 singleton service
    /// </summary>
    /// <param name="instance">The current IServiceCollection instance</param>
    /// <param name="accessKeyId">The AWS authentication access_key_id</param>
    /// <param name="secretAccessKey">The AWS authentication secret_access_key</param>
    /// <param name="region">Optional, AWS region</param>
    /// <param name="kmsKeyId">Optional, AWS Key Management Service key ID for encrypting objects</param>
    /// <param name="format">Optional, preferred serialization format</param>
    /// <returns><paramref name="instance" /></returns>
    public static IServiceCollection UseSyncStreamAwsSimpleStorageService(this IServiceCollection instance,
        string accessKeyId, string secretAccessKey, string region = null, string kmsKeyId = null,
        SerializerFormat format = SerializerFormat.Json) =>
        UseSyncStreamAwsSimpleStorageService(instance, new(accessKeyId, secretAccessKey, region, kmsKeyId, format));

    /// <summary>
    ///     This method configures and registers a SyncStream AWS S3 singleton service with <paramref name="configuration" /> with forced JSON serialization
    /// </summary>
    /// <param name="instance">The current IServiceCollection instance</param>
    /// <param name="configuration">The configuration values to use</param>
    /// <returns><paramref name="instance" /></returns>
    public static IServiceCollection UseSyncStreamJsonAwsSimpleStorageService(this IServiceCollection instance,
        AwsSimpleStorageServiceClientConfiguration configuration)
    {
        // Configure and register our singleton service
        instance.AddSingleton<IAwsSimpleStorageService, JsonAwsSimpleStorageService>(_ => new(configuration));

        // We're done, return the instance
        return instance;
    }

    /// <summary>
    ///     This method configures and registers a SyncStream AWS S3 singleton service with forced JSON serialization
    /// </summary>
    /// <param name="instance">The current IServiceCollection instance</param>
    /// <param name="accessKeyId">The AWS authentication access_key_id</param>
    /// <param name="secretAccessKey">The AWS authentication secret_access_key</param>
    /// <param name="region">Optional, AWS region</param>
    /// <param name="kmsKeyId">Optional, AWS Key Management Service key ID for encrypting objects</param>
    /// <returns><paramref name="instance" /></returns>
    public static IServiceCollection UseSyncStreamJsonAwsSimpleStorageService(this IServiceCollection instance,
        string accessKeyId, string secretAccessKey, string region = null, string kmsKeyId = null) =>
        UseSyncStreamJsonAwsSimpleStorageService(instance,
            new(accessKeyId, secretAccessKey, region, kmsKeyId, SerializerFormat.Json));

    /// <summary>
    ///     This method configures and registers a SyncStream AWS S3 singleton service with <paramref name="configuration" /> with forced JSON serialization
    /// </summary>
    /// <param name="instance">The current IServiceCollection instance</param>
    /// <param name="configuration">The configuration values to use</param>
    /// <returns><paramref name="instance" /></returns>
    public static IServiceCollection UseSyncStreamXmlAwsSimpleStorageService(this IServiceCollection instance,
        AwsSimpleStorageServiceClientConfiguration configuration)
    {
        // Configure and register our singleton service
        instance.AddSingleton<IAwsSimpleStorageService, JsonAwsSimpleStorageService>(_ => new(configuration));

        // We're done, return the instance
        return instance;
    }

    /// <summary>
    ///     This method configures and registers a SyncStream AWS S3 singleton service with forced JSON serialization
    /// </summary>
    /// <param name="instance">The current IServiceCollection instance</param>
    /// <param name="accessKeyId">The AWS authentication access_key_id</param>
    /// <param name="secretAccessKey">The AWS authentication secret_access_key</param>
    /// <param name="region">Optional, AWS region</param>
    /// <param name="kmsKeyId">Optional, AWS Key Management Service key ID for encrypting objects</param>
    /// <returns><paramref name="instance" /></returns>
    public static IServiceCollection UseSyncStreamXmlAwsSimpleStorageService(this IServiceCollection instance,
        string accessKeyId, string secretAccessKey, string region = null, string kmsKeyId = null) =>
        UseSyncStreamXmlAwsSimpleStorageService(instance,
            new(accessKeyId, secretAccessKey, region, kmsKeyId, SerializerFormat.Xml));
}
