using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using SyncStream.Aws.S3.Client.Configuration;

// Define our namespace
namespace SyncStream.Aws.S3.Client;

/// <summary>
///     This interface maintains the service provider structure for our AWS S3 client
/// </summary>
public interface IAwsSimpleStorageService
{
    /// <summary>
    ///     This method determines the bucket and name of an object
    /// </summary>
    /// <param name="objectName">The object name to parse</param>
    /// <returns>A tuple containing the bucket name and the object name</returns>
    public Tuple<string, string> BucketAndObjectName(string objectName);

    /// <summary>
    ///     This method asynchronously determines whether or not a bucket <paramref name="objectName" /> exists
    /// </summary>
    /// <param name="objectName">The object to query</param>
    /// <returns>An awaitable task containing a boolean denoting the existence of the bucket</returns>
    public Task<bool> BucketExistsAsync(string objectName);

    /// <summary>
    ///     This method asynchronously copies <paramref name="sourceObjectName" /> to <paramref name="targetObjectName" /> in Amazon S3
    /// </summary>
    /// <param name="sourceObjectName">The source object to copy data from</param>
    /// <param name="targetObjectName">The target object to copy data to</param>
    /// <returns>An awaitable task containing the AWS response from the copy</returns>
    public Task<CopyObjectResponse> CopyFileAsync(string sourceObjectName, string targetObjectName);

    /// <summary>
    ///     This method asynchronously deletes <paramref name="objectName" /> from S3 if it exists
    /// </summary>
    /// <param name="objectName">The object to delete if it exists</param>
    /// <returns>An awaitable task containing the AWS SDK object-delete response</returns>
    public Task<DeleteObjectResponse> DeleteFileIfExistsAsync(string objectName);

    /// <summary>
    ///     This method asynchronously downloads object <paramref name="objectName" /> from S3
    /// </summary>
    /// <param name="objectName">The object to download</param>
    /// <returns>An awaitable task containing a stream of the object's contents</returns>
    public Task<Stream> DownloadObjectAsync(string objectName);

    /// <summary>
    ///     This method asynchronously downloads object <paramref name="objectName" /> into a <typeparamref name="TTarget" /> object
    /// </summary>
    /// <param name="objectName">The object to download</param>
    /// <typeparam name="TTarget">The target deserialization type</typeparam>
    /// <returns>An awaitable task contain the deserialized object</returns>
    public Task<TTarget> DownloadObjectAsync<TTarget>(string objectName);

    /// <summary>
    ///     This method asynchronously finds an object in S3 that matches the <paramref name="searchPattern" />
    ///     with the prefix path <paramref name="objectPrefix" />
    /// </summary>
    /// <param name="objectPrefix">The prefix path where the result object can be found</param>
    /// <param name="searchPattern">The pattern the object key must match</param>
    /// <returns>An awaitable task containing the object in <paramref name="objectPrefix" /> that matches the <paramref name="searchPattern" /></returns>
    public Task<S3Object> FindObjectAsync(string objectPrefix, string searchPattern);

    /// <summary>
    ///     This method asynchronously finds an object in S3 that contain the <paramref name="metadata" />
    ///     with the prefix path <paramref name="objectPrefix" />
    /// </summary>
    /// <param name="objectPrefix">The prefix path where the result object can be found</param>
    /// <param name="metadata">The metadata the object must contain</param>
    /// <returns>An awaitable task containing the object in <paramref name="objectPrefix" /> that contains the <paramref name="metadata" /></returns>
    public Task<S3Object> FindObjectAsync(string objectPrefix, Dictionary<string, string> metadata);

    /// <summary>
    ///     This method asynchronously finds an object in S3 that matches the <paramref name="searchPattern" /> with the prefix path
    ///     <paramref name="objectPrefix" /> and returns it as a deserialized <typeparamref name="TTarget" /> object instance
    /// </summary>
    /// <param name="objectPrefix">The prefix path where the result object can be found</param>
    /// <param name="searchPattern">The pattern the object key must match</param>
    /// <typeparam name="TTarget">The expected type of the deserialized object instance</typeparam>
    /// <returns>An awaitable task containing the object in <paramref name="objectPrefix" /> that matches the <paramref name="searchPattern" /></returns>
    public Task<TTarget> FindObjectAsync<TTarget>(string objectPrefix, string searchPattern);

    /// <summary>
    ///     This method asynchronously finds an object in S3 that contains the <paramref name="metadata" /> with the prefix path
    ///     <paramref name="objectPrefix" /> and returns it as a deserialized <typeparamref name="TTarget" /> object instance
    /// </summary>
    /// <param name="objectPrefix">The prefix path where the result object can be found</param>
    /// <param name="metadata">The metadata the object must contain</param>
    /// <typeparam name="TTarget">The expected type of the deserialized object instance</typeparam>
    /// <returns>An awaitable task containing the object in <paramref name="objectPrefix" /> that contain the <paramref name="metadata" /></returns>
    public Task<TTarget> FindObjectAsync<TTarget>(string objectPrefix, Dictionary<string, string> metadata);

    /// <summary>
    ///     This method asynchronously finds objects in S3 that match the <paramref name="searchPattern" />
    ///     with the prefix path <paramref name="objectPrefix" />
    /// </summary>
    /// <param name="objectPrefix">The prefix path where the result objects can be found</param>
    /// <param name="searchPattern">The pattern the object keys must match</param>
    /// <returns>An awaitable task containing the objects in <paramref name="objectPrefix" /> that match the <paramref name="searchPattern" /></returns>
    public Task<List<S3Object>> FindObjectsAsync(string objectPrefix, string searchPattern);

    /// <summary>
    ///     This method asynchronously finds objects in S3 that contain the <paramref name="metadata" />
    ///     with the prefix path <paramref name="objectPrefix" />
    /// </summary>
    /// <param name="objectPrefix">The prefix path where the result objects can be found</param>
    /// <param name="metadata">The metadata the object must contain</param>
    /// <param name="single">Optional, flag that denotes whether to return after a single match or not</param>
    /// <returns>An awaitable task containing the objects in <paramref name="objectPrefix" /> that contain the <paramref name="metadata" /></returns>
    public Task<List<S3Object>> FindObjectsAsync(string objectPrefix, Dictionary<string, string> metadata,
        bool single = false);

    /// <summary>
    ///     This method asynchronously finds objects in S3 that match the <paramref name="searchPattern" /> with the prefix path
    ///     <paramref name="objectPrefix" /> and returns them as deserialized <typeparamref name="TTarget" /> object instances
    /// </summary>
    /// <param name="objectPrefix">The prefix path where the result objects can be found</param>
    /// <param name="searchPattern">The pattern the object keys must match</param>
    /// <typeparam name="TTarget">The expected type of the deserialized object instances</typeparam>
    /// <returns>An awaitable task containing the objects in <paramref name="objectPrefix" /> that match the <paramref name="searchPattern" /></returns>
    public Task<List<TTarget>> FindObjectsAsync<TTarget>(string objectPrefix, string searchPattern);

    /// <summary>
    ///     This method asynchronously finds objects in S3 that contain the <paramref name="metadata" /> with the prefix path
    ///     <paramref name="objectPrefix" /> and returns them as deserialized <typeparamref name="TTarget" /> object instances
    /// </summary>
    /// <param name="objectPrefix">The prefix path where the result objects can be found</param>
    /// <param name="metadata">The metadata the object must contain</param>
    /// <param name="single">Optional, flag that denotes whether to return after a single match or not</param>
    /// <typeparam name="TTarget">The expected type of the deserialized object instances</typeparam>
    /// <returns>An awaitable task containing the objects in <paramref name="objectPrefix" /> that contain the <paramref name="metadata" /></returns>
    public Task<List<TTarget>> FindObjectsAsync<TTarget>(string objectPrefix, Dictionary<string, string> metadata,
        bool single = false);

    /// <summary>
    ///     This method generates an authenticated AWS client
    /// </summary>
    /// <returns>An authenticated AWS S3 Client</returns>
    public AmazonS3Client GetClient();

    /// <summary>
    ///     This method generates an authenticated transfer utility
    /// </summary>
    /// <returns>An authenticated AWS S3 Transfer Utility</returns>
    public TransferUtility GetTransferUtility();

    /// <summary>
    ///     This method asynchronously lists all objects with prefix <paramref name="objectPrefix" />
    /// </summary>
    /// <param name="objectPrefix">The prefix of the objects to list</param>
    /// <returns>An awaitable task containing the list of objects with the prefix <paramref name="objectPrefix" /></returns>
    public Task<List<S3Object>> ListAllObjectsAsync(string objectPrefix);

    /// <summary>
    ///     This method asynchronously lists all objects with prefix <paramref name="objectPrefix" />,
    ///     downloads them then deserializes them into <typeparamref name="TTarget" />
    /// </summary>
    /// <param name="objectPrefix">The prefix of the objects to list</param>
    /// <typeparam name="TTarget">The expected type of the the deserialized objects</typeparam>
    /// <returns>An awaitable task containing the list of <typeparamref name="TTarget" /> typed objects with the prefix <paramref name="objectPrefix" /></returns>
    public Task<List<TTarget>> ListAllObjectsAsync<TTarget>(string objectPrefix);

    /// <summary>
    /// This method asynchronously and recursively lists the objects matching <paramref name="objectPrefix" />
    /// </summary>
    /// <param name="objectPrefix">The prefix pattern to query S3 with</param>
    /// <param name="delimiter">Optional, directory or object delimiter</param>
    /// <param name="recursive">Optional, flag to denote recursive subdirectory listings</param>
    /// <returns>An awaitable task containing a list of objects</returns>
    public Task<List<S3Object>> ListObjectsAsync(string objectPrefix, string delimiter = null, bool recursive = true);

    /// <summary>
    ///     This method asynchronously and recursively lists the objects matching <paramref name="objectPrefix" /> and maps them to documents <typeparamref name="TTarget" />
    /// </summary>
    /// <param name="objectPrefix">The prefix pattern to query S3 with</param>
    /// <param name="delimiter">Optional, directory or object delimiter</param>
    /// <param name="recursive">Optional, flag to denote recursive subdirectory listings</param>
    /// <typeparam name="TTarget">The output document type</typeparam>
    /// <returns>An awaitable task containing a list of <typeparamref name="TTarget" /> objects</returns>
    public Task<List<TTarget>> ListObjectsAsync<TTarget>(string objectPrefix, string delimiter = null,
        bool recursive = true);

    /// <summary>
    ///     This method asynchronously determines whether object <paramref name="objectName" /> exists or not
    /// </summary>
    /// <param name="objectName">The object to query</param>
    /// <returns>An awaitable task containing a boolean denoting whether the object exists or not</returns>
    public Task<bool> ObjectExistsAsync(string objectName);

    /// <summary>
    ///     This method returns a pre-signed URL to <paramref name="objectName" /> object
    /// </summary>
    /// <param name="objectName">The object to get a pre-signed URL for</param>
    /// <returns>The pre-signed URL to <paramref name="objectName" /> on Amazon S3</returns>
    public string ObjectUrl(string objectName);

    /// <summary>
    ///     This method asynchronously uploads <paramref name="data" /> to <paramref name="objectName" />
    /// </summary>
    /// <param name="objectName">The target object to save <paramref name="data" /> to</param>
    /// <param name="data">The content of the object</param>
    /// <param name="metadata">Optional metadata for the object</param>
    /// <param name="acl">Optional access control for the object</param>
    /// <returns>An awaitable task with no result</returns>
    public Task UploadAsync(string objectName, Stream data, Dictionary<string, object> metadata = null,
        S3CannedACL acl = null);

    /// <summary>
    ///     This method asynchronously uploads <paramref name="binary" /> to <paramref name="objectName" />
    /// </summary>
    /// <param name="objectName">The target object to save <paramref name="binary" /> to</param>
    /// <param name="binary">The binary content of the object</param>
    /// <param name="metadata">Optional metadata for the object</param>
    /// <param name="acl">Optional access control for the object</param>
    /// <returns>An awaitable task with no result</returns>
    public Task UploadAsync(string objectName, byte[] binary, Dictionary<string, object> metadata = null,
        S3CannedACL acl = null);

    /// <summary>
    ///     This method asynchronously uploads <paramref name="localPathOrContent" /> to <paramref name="objectName" />
    /// </summary>
    /// <param name="objectName">The target object to save <paramref name="localPathOrContent" /> to</param>
    /// <param name="localPathOrContent">The local path to the file, directory or the content to be uploaded</param>
    /// <param name="metadata">Optional metadata for the object</param>
    /// <param name="acl">Optional access control for the object</param>
    /// <returns>An awaitable task with no result</returns>
    public Task UploadAsync(string objectName, string localPathOrContent, Dictionary<string, object> metadata = null,
        S3CannedACL acl = null);

    /// <summary>
    ///     This method asynchronously serializes <paramref name="content" /> then uploads it to <paramref name="objectName" /> on S3
    /// </summary>
    /// <param name="objectName">The target object to save <paramref name="content" /> to</param>
    /// <param name="content">The object to serialize and upload</param>
    /// <param name="metadata">Optional metadata for the object</param>
    /// <param name="acl">Optional access control for the object</param>
    /// <typeparam name="TSource">The expected source object type</typeparam>
    /// <returns>An awaitable task containing a void result</returns>
    public Task UploadAsync<TSource>(string objectName, TSource content, Dictionary<string, object> metadata = null,
        S3CannedACL acl = null);

    /// <summary>
    ///     This method asynchronously uploads a local file or directory, or a serialized string of data to S3 as <typeparamref name="TSource" />
    /// </summary>
    /// <param name="objectName">The object path to upload <paramref name="localDirectoryFileOrContent" /> to</param>
    /// <param name="localDirectoryFileOrContent">The local file or directory path, or a serialized string</param>
    /// <param name="metadata">Optional metadata for the object</param>
    /// <param name="acl">Optional access control for the object</param>
    /// <typeparam name="TSource">The expected source object type</typeparam>
    /// <returns>An awaitable task containing a void result</returns>
    public Task UploadAsync<TSource>(string objectName, string localDirectoryFileOrContent,
        Dictionary<string, object> metadata = null, S3CannedACL acl = null);

    /// <summary>
    ///     This method fluidly resets the <paramref name="configuration" /> into the instance
    /// </summary>
    /// <param name="configuration">The new configuration</param>
    /// <returns>The current instance</returns>
    public IAwsSimpleStorageService WithConfiguration(AwsSimpleStorageServiceClientConfiguration configuration);
}
