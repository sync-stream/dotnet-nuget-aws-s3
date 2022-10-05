using SyncStream.Aws.S3.Client.Configuration;
using SyncStream.Serializer;

// Define our namespace
namespace SyncStream.Aws.S3.Client;

/// <summary>
///     This class maintains the structure of our AWS S3 service provider that forces JSON serialization and deserialization
/// </summary>
public class JsonAwsSimpleStorageService : AwsSimpleStorageService, IAwsSimpleStorageService
{
    /// <summary>
    ///     This method instantiates an AWS S3 service that forces JSON serialization and deserialization
    /// </summary>
    /// <param name="configuration">The configuration values to use</param>
    public JsonAwsSimpleStorageService(AwsSimpleStorageServiceClientConfiguration configuration) : base(new()
    {
        // Set the authentication access key ID into the configuration
        AccessKeyId = configuration.AccessKeyId,

        // Set the KMS key ID into the configuration
        KeyManagementServiceKeyId = configuration.KeyManagementServiceKeyId,

        // Set the region into the configuration
        Region = configuration.Region,

        // Set the authentication secret access key into the configuration
        SecretAccessKey = configuration.SecretAccessKey,

        // Set the serialization format into the configuration
        SerializationFormat = SerializerFormat.Json
    })
    {
    }

    /// <summary>
    ///     This method instantiates an AWS S3 service that forces JSON serialization and deserialization
    /// </summary>
    /// <param name="accessKeyId">The AWS authentication access_key_id</param>
    /// <param name="secretAccessKey">The AWS authentication secret_access_key</param>
    /// <param name="region">Optional, AWS region</param>
    /// <param name="kmsKeyId">Optional, AWS Key Management Service key ID for encrypting objects</param>
    public JsonAwsSimpleStorageService(string accessKeyId, string secretAccessKey, string region = null,
        string kmsKeyId = null) : this(new(accessKeyId, secretAccessKey, region, kmsKeyId))
    {
    }
}
