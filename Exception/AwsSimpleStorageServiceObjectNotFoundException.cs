// Define our namespace
namespace SyncStream.Aws.S3.Client.Exception;

/// <summary>
///     This exception class maintains the structure of an S3 object not-found
/// </summary>
public class AwsSimpleStorageServiceObjectNotFoundException : System.Exception
{
    /// <summary>
    ///     This method instantiates our exception with an optional message and optional inner exception
    /// </summary>
    /// <param name="message">Optional message describing the exception</param>
    /// <param name="innerException">Optional inner exception that occurred prior to this exception</param>
    public AwsSimpleStorageServiceObjectNotFoundException(string message = null, System.Exception innerException = null)
        : base(message, innerException)
    {
    }
}
