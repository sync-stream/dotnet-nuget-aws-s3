using System.Text.Json.Serialization;
using System.Xml.Serialization;
using Microsoft.Extensions.Configuration;
using SyncStream.Serializer;

// Define the namespace
namespace SyncStream.Aws.S3.Client.Configuration;

/// <summary>
///     This class maintains the structure of our AWS S3 configuration
/// </summary>
[XmlRoot("simpleStorageServiceClientConfiguration")]
public class AwsSimpleStorageServiceClientConfiguration : IAwsSimpleStorageServiceClientConfiguration
{
    /// <summary>
    ///     This property contains the AWS access key
    /// </summary>
    [ConfigurationKeyName("accessKeyId")]
    [JsonPropertyName("accessKeyId")]
    [XmlAttribute("accessKeyId")]
    public string AccessKeyId { get; set; }

    /// <summary>
    ///     This property contains the AWS KMS Key ID for encrypting and decrypting objects
    /// </summary>
    [ConfigurationKeyName("keyManagementServiceKeyId")]
    [JsonPropertyName("keyManagementServiceKeyId")]
    [XmlAttribute("keyManagementServiceKeyId")]
    public string KeyManagementServiceKeyId { get; set; }

    /// <summary>
    ///     This property contains the AWS region
    /// </summary>
    [ConfigurationKeyName("region")]
    [JsonPropertyName("region")]
    [XmlAttribute("region")]
    public string Region { get; set; }

    /// <summary>
    ///     This property contains the AWS secret access key
    /// </summary>
    [ConfigurationKeyName("secretAccessKey")]
    [JsonPropertyName("secretAccessKey")]
    [XmlAttribute("secretAccessKey")]
    public string SecretAccessKey { get; set; }

    /// <summary>
    ///     This property contains our serialization format for complex objects
    /// </summary>

    public SerializerFormat SerializationFormat { get; set; } = SerializerFormat.Json;

    /// <summary>
    ///     This method instantiates our configuration model
    /// </summary>
    public AwsSimpleStorageServiceClientConfiguration()
    {
    }

    /// <summary>
    ///     This method instantiates our configuration model with values
    /// </summary>
    /// <param name="accessKeyId">The AWS authentication access_key_id</param>
    /// <param name="secretAccessKey">The AWS authentication secret_access_key</param>
    /// <param name="region">Optional, AWS region</param>
    /// <param name="kmsKeyId">Optional, AWS Key Management Service key ID used for encrypting objects</param>
    /// <param name="format">Optional, serialization format for complex objects</param>
    public AwsSimpleStorageServiceClientConfiguration(string accessKeyId, string secretAccessKey, string region = null,
        string kmsKeyId = null, SerializerFormat format = SerializerFormat.Json)
    {
        // Set the access key ID into the instance
        AccessKeyId = accessKeyId;

        // Set the KMS key ID into the instance
        KeyManagementServiceKeyId = kmsKeyId;

        // Set the region into the instance
        Region = region ?? "us-east-1";

        // Set the secret access key into the instance
        SecretAccessKey = secretAccessKey;

        // Set the serializer format into the instance
        SerializationFormat = format;
    }
}
