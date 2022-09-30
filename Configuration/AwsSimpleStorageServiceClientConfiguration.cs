using System.Text.Json.Serialization;
using System.Xml.Serialization;

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
    [JsonPropertyName("accessKeyId")]
    [XmlAttribute("accessKeyId")]
    public string AccessKeyId { get; set; }

    /// <summary>
    ///     This property contains the AWS KMS Key ID for encrypting and decrypting objects
    /// </summary>
    [JsonPropertyName("keyManagementServiceKeyId")]
    [XmlAttribute("keyManagementServiceKeyId")]
    public string KeyManagementServiceKeyId { get; set; }

    /// <summary>
    ///     This property contains the AWS secret access key
    /// </summary>
    [JsonPropertyName("secretAccessKey")]
    [XmlAttribute("secretAccessKey")]
    public string SecretAccessKey { get; set; }

    /// <summary>
    ///     This property contains the AWS region
    /// </summary>
    [JsonPropertyName("region")]
    [XmlAttribute("region")]
    public string Region { get; set; }
}
