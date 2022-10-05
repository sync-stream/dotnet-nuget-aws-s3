using System.Text.Json.Serialization;
using System.Xml.Serialization;
using Microsoft.Extensions.Configuration;

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
    ///     This property contains the AWS secret access key
    /// </summary>
    [ConfigurationKeyName("secretAccessKey")]
    [JsonPropertyName("secretAccessKey")]
    [XmlAttribute("secretAccessKey")]
    public string SecretAccessKey { get; set; }

    /// <summary>
    ///     This property contains the AWS region
    /// </summary>
    [ConfigurationKeyName("region")]
    [JsonPropertyName("region")]
    [XmlAttribute("region")]
    public string Region { get; set; }
}
