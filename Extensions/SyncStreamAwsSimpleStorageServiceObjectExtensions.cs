using Amazon.S3;
using SyncStream.Aws.S3.Client.Configuration;

// Define our namespace
namespace SyncStream.Aws.S3.Client.Extensions;

/// <summary>
///     This class maintains the SyncStream AWS S3 Client extensions for all objects
/// </summary>
public static class SyncStreamAwsSimpleStorageServiceObjectExtensions
{
    /// <summary>
    ///     This method provides a fluid extension for serializing and uploading objects directly to AWS S3
    /// </summary>
    /// <param name="instance">The current instance of the object to serialize and upload to S3</param>
    /// <param name="objectPath">The path to store the object's serialization</param>
    /// <param name="configuration">Optional, client configuration override</param>
    /// <param name="metadata">Optional, object metadata</param>
    /// <param name="acl">Optional, object access-control-list</param>
    /// <typeparam name="TSource">The expected type of the current object <paramref name="instance" /></typeparam>
    /// <returns>An awaitable task containing a void result</returns>
    public static Task ToAwsSimpleStorageServiceAsync<TSource>(this TSource instance, string objectPath,
        IAwsSimpleStorageServiceClientConfiguration configuration = null, Dictionary<string, object> metadata = null,
        S3CannedACL acl = null) =>
        AwsSimpleStorageServiceClient.UploadAsync(objectPath, instance, configuration, metadata, acl);

    /// <summary>
    ///     This method provides a fluid extension for serializing and uploading objects directly to AWS S3
    /// </summary>
    /// <param name="instance">The current instance of the object to serialize and upload to S3</param>
    /// <param name="objectPath">The path to store the object's serialization</param>
    /// <param name="serviceProvider">An existing IAwsSimpleStorageService service provider instance</param>
    /// <param name="metadata">Optional, object metadata</param>
    /// <param name="acl">Optional, object access-control-list</param>
    /// <typeparam name="TSource">The expected type of the current object <paramref name="instance" /></typeparam>
    /// <returns>An awaitable task containing a void result</returns>
    public static Task ToAwsSimpleStorageServiceAsync<TSource>(this TSource instance, string objectPath,
        IAwsSimpleStorageService serviceProvider, Dictionary<string, object> metadata = null, S3CannedACL acl = null) =>
        serviceProvider.UploadAsync(objectPath, instance, metadata, acl);
}
