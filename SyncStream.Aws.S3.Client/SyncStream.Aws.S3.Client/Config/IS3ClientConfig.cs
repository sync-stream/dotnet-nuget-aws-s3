// Define our namespace
namespace SyncStream.Aws.S3.Client.Config;

/// <summary>
/// This interface maintains the structure of our AWS configuration
/// </summary>
public interface IS3ClientConfig
{
    /// <summary>
    /// This property contains the AWS access key
    /// </summary>
    public string AccessKeyId { get; set; }
    
    /// <summary>
    /// This property contains the AWS KMS Key ID for encrypting and decrypting objects
    /// </summary>
    public string KeyManagementServiceKeyId { get; set; }
    
    /// <summary>
    /// This property contains the AWS secret access key
    /// </summary>
    public string SecretAccessKey { get; set; }
    
    /// <summary>
    /// This property contains the AWS region
    /// </summary>
    public string Region { get; set; }
}
