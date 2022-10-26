using System.Text;
using System.Text.RegularExpressions;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using SyncStream.Aws.S3.Client.Configuration;
using SyncStream.Aws.S3.Client.Exception;
using SyncStream.Serializer;

// Define our namespace
namespace SyncStream.Aws.S3.Client;

/// <summary>
///     This class maintains the structure of our S3 client
/// </summary>
public class AwsSimpleStorageServiceClient
{
    /// <summary>
    ///     This property contains the instance of our configuration
    /// </summary>
    public static IAwsSimpleStorageServiceClientConfiguration Configuration { get; private set; }

    /// <summary>
    ///     This method configures the encryption for an upload
    /// </summary>
    /// <param name="request">The upload request being sent to S3</param>
    /// <param name="configuration">Optional override configuration instance</param>
    protected static void ConfigureEncryption(ref TransferUtilityUploadRequest request,
        IAwsSimpleStorageServiceClientConfiguration configuration = null)
    {
        // Localize the KMS key ID from the configuration
        string keyManagementServiceKeyId = (configuration ?? Configuration)?.KeyManagementServiceKeyId;

        // Check for a KMS key ID in the configuration
        if (!string.IsNullOrEmpty(keyManagementServiceKeyId) && !string.IsNullOrWhiteSpace(keyManagementServiceKeyId))
        {

            // Set the KMS key ID into the request
            request.ServerSideEncryptionKeyManagementServiceKeyId = keyManagementServiceKeyId;

            // Set the server-side encryption method into the request
            request.ServerSideEncryptionMethod = ServerSideEncryptionMethod.AWSKMS;
        }
    }

    /// <summary>
    ///     This method determines the bucket and name of an object
    /// </summary>
    /// <param name="objectName">The object name to parse</param>
    /// <returns>A tuple containing the bucket name and the object name</returns>
    public static Tuple<string, string> BucketAndObjectName(string objectName)
    {
        // Check the name for a directory
        if (objectName.EndsWith("/")) objectName = objectName.TrimEnd('/');

        // Check for an absolute path
        if (objectName.StartsWith("/")) objectName = objectName.Substring(1);

        // Check for a directory separator
        if (!objectName.Contains('/')) return new(objectName, string.Empty);

        // Split the parts
        List<string> parts = objectName.Split('/', StringSplitOptions.TrimEntries).ToList();

        // Check for parts
        if (!parts.Any()) return new(objectName, string.Empty);

        // We're done, return the bucket and objet name
        return new(parts.FirstOrDefault(), string.Join("/", parts.Skip(1)));
    }

    /// <summary>
    ///     This method asynchronously determines whether or not a bucket <paramref name="objectName" /> exists
    /// </summary>
    /// <param name="objectName">The object to query</param>
    /// <param name="configuration">Optional configuration override instance</param>
    /// <returns>An awaitable task containing a boolean denoting the existence of the bucket</returns>
    public static async Task<bool> BucketExistsAsync(string objectName,
        IAwsSimpleStorageServiceClientConfiguration configuration = null)
    {
        // Grab the bucket file name
        Tuple<string, string> bucketAndObjectName = BucketAndObjectName(objectName);

        // Check for a bucket name
        if (string.IsNullOrEmpty(bucketAndObjectName.Item1) ||
            string.IsNullOrWhiteSpace(bucketAndObjectName.Item1)) return false;

        // Check for an object name
        if (string.IsNullOrEmpty(bucketAndObjectName.Item2) ||
            string.IsNullOrWhiteSpace(bucketAndObjectName.Item2)) return false;

        // Localize our client into a disposable scope
        using AmazonS3Client client = GetClient(configuration);

        // Try to determine whether the bucket exists or not
        try
        {
            // Localize the bucket's location
            GetBucketLocationResponse response = await client.GetBucketLocationAsync(bucketAndObjectName.Item1);

            // We're done, return the existence
            return !string.IsNullOrEmpty(response.Location?.Value) &&
                   !string.IsNullOrWhiteSpace(response.Location?.Value);
        }
        catch (System.Exception)
        {
            // We're done, return the non-existence
            return false;
        }
    }

    /// <summary>
    ///     This method asynchronously copies <paramref name="sourceObjectName" /> to <paramref name="targetObjectName" /> in Amazon S3
    /// </summary>
    /// <param name="sourceObjectName">The source object to copy data from</param>
    /// <param name="targetObjectName">The target object to copy data to</param>
    /// <param name="configuration">Optional configuration override instance</param>
    /// <returns>An awaitable task containing the AWS response from the copy</returns>
    public static async Task<CopyObjectResponse> CopyFileAsync(string sourceObjectName, string targetObjectName,
        IAwsSimpleStorageServiceClientConfiguration configuration = null)
    {
        // Localize the source bucket and object name
        Tuple<string, string> sourceBucketAndObjectName = BucketAndObjectName(sourceObjectName);
        // Localize the target bucket and object name
        Tuple<string, string> targetBucketAndObjectName = BucketAndObjectName(targetObjectName);

        // Check to see if we are missing a source bucket and object name, then return
        if (string.IsNullOrEmpty(sourceBucketAndObjectName.Item1) ||
            string.IsNullOrWhiteSpace(sourceBucketAndObjectName.Item1) ||
            string.IsNullOrEmpty(sourceBucketAndObjectName.Item2) ||
            string.IsNullOrWhiteSpace(sourceBucketAndObjectName.Item2)) return null;

        // Check to see if we are missing a target bucket and object name, then return
        if (string.IsNullOrEmpty(targetBucketAndObjectName.Item1) ||
            string.IsNullOrWhiteSpace(targetBucketAndObjectName.Item1) ||
            string.IsNullOrEmpty(targetBucketAndObjectName.Item2) ||
            string.IsNullOrWhiteSpace(targetBucketAndObjectName.Item2)) return null;

        // Instantiate our request
        CopyObjectRequest request = new()
        {
            // Set the target bucket into the request
            DestinationBucket = targetBucketAndObjectName.Item1,

            // Set the target object name into the request
            DestinationKey = targetBucketAndObjectName.Item2,

            // Set the source bucket into the request
            SourceBucket = sourceBucketAndObjectName.Item1,

            // Set the source object name into the request
            SourceKey = sourceBucketAndObjectName.Item2
        };

        // Localize our client into a disposable context
        using AmazonS3Client client = GetClient(configuration);

        // Localize the response
        CopyObjectResponse response = await client.CopyObjectAsync(request);

        // We're done, send the response
        return response;
    }

    /// <summary>
    ///     This method asynchronously deletes <paramref name="objectName" /> from S3 if it exists
    /// </summary>
    /// <param name="objectName">The object to delete if it exists</param>
    /// <param name="configuration">Optional configuration override instance</param>
    /// <returns>An awaitable task containing the AWS SDK object-delete response</returns>
    public static async Task<DeleteObjectResponse> DeleteFileIfExistsAsync(string objectName,
        IAwsSimpleStorageServiceClientConfiguration configuration = null)
    {
        // Make sure the file exists and return
        if (!await ObjectExistsAsync(objectName, configuration)) return null;

        // Grab the bucket and object name
        Tuple<string, string> bucketAndObjectName = BucketAndObjectName(objectName);

        // Check for a bucket name
        if (string.IsNullOrEmpty(bucketAndObjectName.Item1) ||
            string.IsNullOrWhiteSpace(bucketAndObjectName.Item1)) return null;

        // Check for an object name name
        if (string.IsNullOrEmpty(bucketAndObjectName.Item2) ||
            string.IsNullOrWhiteSpace(bucketAndObjectName.Item2)) return null;

        // Instantiate our request
        DeleteObjectRequest request = new()
        {
            // Set the bucket into the request
            BucketName = bucketAndObjectName.Item1,

            // Set the object name into the request
            Key = bucketAndObjectName.Item2
        };

        // Localize our client into a disposable context
        using AmazonS3Client client = GetClient(configuration);

        // Localize the response
        DeleteObjectResponse response = await client.DeleteObjectAsync(request);

        // We're done, send the response
        return response;
    }

    /// <summary>
    ///     This method asynchronously downloads object <paramref name="objectName" /> from S3
    /// </summary>
    /// <param name="objectName">The object to download</param>
    /// <param name="configuration">Optional configuration override instance</param>
    /// <returns>An awaitable task containing a stream of the object's contents</returns>
    /// <exception cref="AwsSimpleStorageServiceObjectNotFoundException">Thrown when the object doesn't exist</exception>
    public static async Task<Stream> DownloadObjectAsync(string objectName,
        IAwsSimpleStorageServiceClientConfiguration configuration = null)
    {
        // Make sure the file exists and return
        if (!await ObjectExistsAsync(objectName, configuration))
            throw new AwsSimpleStorageServiceObjectNotFoundException("S3 Object Not Found");

        // Localize the bucket and object name
        Tuple<string, string> bucketAndObjectName = BucketAndObjectName(objectName);

        // Instantiate our request
        GetObjectRequest request = new()
        {
            // Set the bucket into the request
            BucketName = bucketAndObjectName.Item1,

            // Set the object name into the request
            Key = bucketAndObjectName.Item2
        };

        // Localize our client into a disposable context
        using AmazonS3Client client = GetClient(configuration);

        // Make the request
        GetObjectResponse response = await client.GetObjectAsync(request);

        // We're done, return the stream
        return response.ResponseStream;
    }

    /// <summary>
    ///     This method asynchronously downloads object <paramref name="objectName" /> into a <typeparamref name="TTarget" /> object
    /// </summary>
    /// <param name="objectName">The object to download</param>
    /// <param name="configuration">Optional configuration override instance</param>
    /// <typeparam name="TTarget">The target deserialization type</typeparam>
    /// <returns>An awaitable task contain the deserialized object</returns>
    public static async Task<TTarget> DownloadObjectAsync<TTarget>(string objectName,
        IAwsSimpleStorageServiceClientConfiguration configuration = null)
    {

        // Download the object stream into a disposable context
        await using Stream objectStream = await DownloadObjectAsync(objectName, configuration);

        // Localize our stream reader into a disposable context
        using StreamReader objectStreamReader = new StreamReader(objectStream, Encoding.UTF8);

        // Read the object's content
        string objectContents = await objectStreamReader.ReadToEndAsync();

        // We're done, check the format and deserialize the object's content
        return SerializerService.Deserialize<TTarget>(objectContents,
            (configuration ?? Configuration).SerializationFormat);
    }

    /// <summary>
    ///     This method asynchronously finds an object in S3 that matches the <paramref name="searchPattern" />
    ///     with the prefix path <paramref name="objectPrefix" />
    /// </summary>
    /// <param name="objectPrefix">The prefix path where the result object can be found</param>
    /// <param name="searchPattern">The pattern the object key must match</param>
    /// <param name="configuration">Optional, client configuration override</param>
    /// <returns>An awaitable task containing the object in <paramref name="objectPrefix" /> that matches the <paramref name="searchPattern" /></returns>
    public static async Task<S3Object> FindObjectAsync(string objectPrefix, string searchPattern,
        IAwsSimpleStorageServiceClientConfiguration configuration = null) =>
        (await FindObjectsAsync(objectPrefix, searchPattern, configuration))?.FirstOrDefault();

    /// <summary>
    ///     This method asynchronously finds an object in S3 that contain the <paramref name="metadata" />
    ///     with the prefix path <paramref name="objectPrefix" />
    /// </summary>
    /// <param name="objectPrefix">The prefix path where the result object can be found</param>
    /// <param name="metadata">The metadata the object must contain</param>
    /// <param name="configuration">Optional, client configuration override</param>
    /// <returns>An awaitable task containing the object in <paramref name="objectPrefix" /> that contains the <paramref name="metadata" /></returns>
    public static async Task<S3Object> FindObjectAsync(string objectPrefix, Dictionary<string, string> metadata,
        IAwsSimpleStorageServiceClientConfiguration configuration = null) =>
        (await FindObjectsAsync(objectPrefix, metadata, configuration, true)).FirstOrDefault();

    /// <summary>
    ///     This method asynchronously finds an object in S3 that matches the <paramref name="searchPattern" /> with the prefix path
    ///     <paramref name="objectPrefix" /> and returns it as a deserialized <typeparamref name="TTarget" /> object instance
    /// </summary>
    /// <param name="objectPrefix">The prefix path where the result object can be found</param>
    /// <param name="searchPattern">The pattern the object key must match</param>
    /// <param name="configuration">Optional, client configuration override</param>
    /// <typeparam name="TTarget">The expected type of the deserialized object instance</typeparam>
    /// <returns>An awaitable task containing the object in <paramref name="objectPrefix" /> that matches the <paramref name="searchPattern" /></returns>
    public static async Task<TTarget> FindObjectAsync<TTarget>(string objectPrefix, string searchPattern,
        IAwsSimpleStorageServiceClientConfiguration configuration = null) =>
        (await FindObjectsAsync<TTarget>(objectPrefix, searchPattern, configuration)).FirstOrDefault();

    /// <summary>
    ///     This method asynchronously finds an object in S3 that contains the <paramref name="metadata" /> with the prefix path
    ///     <paramref name="objectPrefix" /> and returns it as a deserialized <typeparamref name="TTarget" /> object instance
    /// </summary>
    /// <param name="objectPrefix">The prefix path where the result object can be found</param>
    /// <param name="metadata">The metadata the object must contain</param>
    /// <param name="configuration">Optional, client configuration override</param>
    /// <typeparam name="TTarget">The expected type of the deserialized object instance</typeparam>
    /// <returns>An awaitable task containing the object in <paramref name="objectPrefix" /> that contain the <paramref name="metadata" /></returns>
    public static async Task<TTarget> FindObjectAsync<TTarget>(string objectPrefix, Dictionary<string, string> metadata,
        IAwsSimpleStorageServiceClientConfiguration configuration = null) =>
        (await FindObjectsAsync<TTarget>(objectPrefix, metadata, configuration, true)).FirstOrDefault();

    /// <summary>
    ///     This method asynchronously finds objects in S3 that match the <paramref name="searchPattern" />
    ///     with the prefix path <paramref name="objectPrefix" />
    /// </summary>
    /// <param name="objectPrefix">The prefix path where the result objects can be found</param>
    /// <param name="searchPattern">The pattern the object keys must match</param>
    /// <param name="configuration">Optional, client configuration override</param>
    /// <returns>An awaitable task containing the objects in <paramref name="objectPrefix" /> that match the <paramref name="searchPattern" /></returns>
    public static async Task<List<S3Object>> FindObjectsAsync(string objectPrefix, string searchPattern,
        IAwsSimpleStorageServiceClientConfiguration configuration = null)
    {
        // List the objects in the directory
        List<S3Object> objects = await ListAllObjectsAsync(objectPrefix, configuration);

        // We're done, filter the objects and return the list
        return objects.Where(o => Regex.IsMatch(o.Key, searchPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase))
            .ToList();
    }

    /// <summary>
    ///     This method asynchronously finds objects in S3 that contain the <paramref name="metadata" />
    ///     with the prefix path <paramref name="objectPrefix" />
    /// </summary>
    /// <param name="objectPrefix">The prefix path where the result objects can be found</param>
    /// <param name="metadata">The metadata the object must contain</param>
    /// <param name="configuration">Optional, client configuration override</param>
    /// <param name="single">Optional, flag denoting exit after one match</param>
    /// <returns>An awaitable task containing the objects in <paramref name="objectPrefix" /> that contain the <paramref name="metadata" /></returns>
    public static async Task<List<S3Object>> FindObjectsAsync(string objectPrefix, Dictionary<string, string> metadata,
        IAwsSimpleStorageServiceClientConfiguration configuration = null, bool single = false)
    {
        // List the objects in the directory
        List<S3Object> objects = await ListAllObjectsAsync(objectPrefix, configuration);

        // Define our response
        List<S3Object> response = new();

        // Define our tasks
        List<Task> tasks = new();

        // Define our local stopping token
        CancellationToken stoppingToken = new();

        // Iterate over the objects
        objects.ForEach(o =>
        {
            // Check the local stopping token for cancellation
            if (stoppingToken.IsCancellationRequested) return;

            // Add the task to the list
            tasks.Add(Task.Run(async () =>
            {
                // Check the cancellation token
                if (stoppingToken.IsCancellationRequested) return;

                // Download the object's metadata
                GetObjectMetadataResponse objectMetadata =
                    await GetClient(configuration).GetObjectMetadataAsync(o.BucketName, o.Key, stoppingToken);

                // Define our matched flag
                bool matched = true;

                // Iterate over the keys in the metadata and check their value
                foreach (string metadataKey in metadata.Keys)
                    if (objectMetadata.Metadata[metadataKey] is null ||
                        objectMetadata.Metadata[metadataKey] != metadata[metadataKey.Replace(" ", "-")])
                        matched = false;

                // Check the matched flag and add the object to the response
                if (matched) response.Add(o);

                // Check the matched flag and the single flag
                if (matched && single && !stoppingToken.CanBeCanceled)
                    CancellationTokenSource.CreateLinkedTokenSource(stoppingToken).Cancel();
            }, stoppingToken));
        });

        // Await all of the tasks
        await Task.WhenAll(tasks);

        // We're done, send the matched objects
        return response;
    }

    /// <summary>
    ///     This method asynchronously finds objects in S3 that match the <paramref name="searchPattern" /> with the prefix path
    ///     <paramref name="objectPrefix" /> and returns them as deserialized <typeparamref name="TTarget" /> object instances
    /// </summary>
    /// <param name="objectPrefix">The prefix path where the result objects can be found</param>
    /// <param name="searchPattern">The pattern the object keys must match</param>
    /// <param name="configuration">Optional, client configuration override</param>
    /// <typeparam name="TTarget">The expected type of the deserialized object instances</typeparam>
    /// <returns>An awaitable task containing the objects in <paramref name="objectPrefix" /> that match the <paramref name="searchPattern" /></returns>
    public static async Task<List<TTarget>> FindObjectsAsync<TTarget>(string objectPrefix, string searchPattern,
        IAwsSimpleStorageServiceClientConfiguration configuration = null)
    {
        // Localize the bucket and object name
        Tuple<string, string> bucketAndObjectName = BucketAndObjectName(objectPrefix);

        // Find the objects
        List<S3Object> objects = await FindObjectsAsync(objectPrefix, searchPattern, configuration);

        // Define our list of awaitable tasks
        List<Task> tasks = new();

        // Define our response
        List<TTarget> response = new();

        // Iterate over the objects
        objects.ForEach(o => tasks.Add(Task.Run(async () =>
            response.Add(await DownloadObjectAsync<TTarget>($"{bucketAndObjectName.Item1}/{o.Key}", configuration)))));

        // Await all of our tasks
        await Task.WhenAll(tasks);

        // We're done, send the response
        return response;
    }

    /// <summary>
    ///     This method asynchronously finds objects in S3 that contain the <paramref name="metadata" /> with the prefix path
    ///     <paramref name="objectPrefix" /> and returns them as deserialized <typeparamref name="TTarget" /> object instances
    /// </summary>
    /// <param name="objectPrefix">The prefix path where the result objects can be found</param>
    /// <param name="metadata">The metadata the object must contain</param>
    /// <param name="configuration">Optional, client configuration override</param>
    /// <param name="single">Optional, flag denoting exit after one match</param>
    /// <typeparam name="TTarget">The expected type of the deserialized object instances</typeparam>
    /// <returns>An awaitable task containing the objects in <paramref name="objectPrefix" /> that contain the <paramref name="metadata" /></returns>
    public static async Task<List<TTarget>> FindObjectsAsync<TTarget>(string objectPrefix,
        Dictionary<string, string> metadata, IAwsSimpleStorageServiceClientConfiguration configuration = null,
        bool single = false)
    {
        // Localize the bucket and object name
        Tuple<string, string> bucketAndObjectName = BucketAndObjectName(objectPrefix);

        // Find the objects
        List<S3Object> objects = await FindObjectsAsync(objectPrefix, metadata, configuration, single);

        // Define our list of awaitable tasks
        List<Task> tasks = new();

        // Define our response
        List<TTarget> response = new();

        // Iterate over the objects
        objects.ForEach(o => tasks.Add(Task.Run(async () =>
            response.Add(await DownloadObjectAsync<TTarget>($"{bucketAndObjectName.Item1}/{o.Key}", configuration)))));

        // Await all of our tasks
        await Task.WhenAll(tasks);

        // We're done, send the response
        return response;
    }

    /// <summary>
    ///     This method generates an authenticated AWS client
    /// </summary>
    /// <param name="configuration">Optional configuration override instance</param>
    /// <returns>An authenticated AWS S3 Client</returns>
    public static AmazonS3Client GetClient(IAwsSimpleStorageServiceClientConfiguration configuration = null)
    {
        // Ensure we have a configuration
        configuration ??= Configuration;

        // Define our credentials
        BasicAWSCredentials credentials = new(configuration.AccessKeyId, configuration.SecretAccessKey);

        // We're done, return our client
        return new(credentials, new AmazonS3Config
        {
            // Define our region
            RegionEndpoint = RegionEndpoint.GetBySystemName(configuration.Region)
        });
    }

    /// <summary>
    ///     This method generates an authenticated transfer utility
    /// </summary>
    /// <param name="configuration">Optional configuration override instance</param>
    /// <returns>An authenticated AWS S3 Transfer Utility</returns>
    public static TransferUtility
        GetTransferUtility(IAwsSimpleStorageServiceClientConfiguration configuration = null) =>
        new(GetClient(configuration));

    /// <summary>
    ///     This method determines whether or not <paramref name="objectName" /> is a directory or not
    /// </summary>
    /// <param name="objectName">The object name or path to query</param>
    /// <returns>A boolean denoting whether <paramref name="objectName" /> is a directory or not</returns>
    public static bool IsDirectory(string objectName) => objectName.Trim().EndsWith("/");

    /// <summary>
    ///     This method determines whether or not <paramref name="objectName" /> is a file or not
    /// </summary>
    /// <param name="objectName">The object name or path to query</param>
    /// <returns>A boolean denoting whether <paramref name="objectName" /> is a file or not</returns>
    public static bool IsFile(string objectName) => !IsDirectory(objectName);

    /// <summary>
    ///     This method asynchronously lists all objects with prefix <paramref name="objectPrefix" />
    /// </summary>
    /// <param name="objectPrefix">The prefix of the objects to list</param>
    /// <param name="configuration">Optional, client configuration override</param>
    /// <returns>An awaitable task containing the list of objects with the prefix <paramref name="objectPrefix" /></returns>
    public static Task<List<S3Object>> ListAllObjectsAsync(string objectPrefix,
        IAwsSimpleStorageServiceClientConfiguration configuration = null) =>
        ListObjectsAsync(objectPrefix, null, configuration);

    /// <summary>
    ///     This method asynchronously lists all objects with prefix <paramref name="objectPrefix" />,
    ///     downloads them then deserializes them into <typeparamref name="TTarget" />
    /// </summary>
    /// <param name="objectPrefix">The prefix of the objects to list</param>
    /// <param name="configuration">Optional, client configuration override</param>
    /// <typeparam name="TTarget">The expected type of the the deserialized objects</typeparam>
    /// <returns>An awaitable task containing the list of <typeparamref name="TTarget" /> typed objects with the prefix <paramref name="objectPrefix" /></returns>
    public static Task<List<TTarget>> ListAllObjectsAsync<TTarget>(string objectPrefix,
        IAwsSimpleStorageServiceClientConfiguration configuration = null) =>
        ListObjectsAsync<TTarget>(objectPrefix, null, configuration);

    /// <summary>
    ///     This method asynchronously and recursively lists the objects matching <paramref name="objectPrefix" />
    /// </summary>
    /// <param name="objectPrefix">The prefix pattern to query S3 with</param>
    /// <param name="delimiter">Optional, directory or object delimiter</param>
    /// <param name="configuration">Optional, configuration override instance</param>
    /// <param name="marker">AWS S3 marker to denote where the request starts</param>
    /// <param name="recursive">Optional, flag to denote recursive subdirectory listings</param>
    /// <returns>An awaitable task containing a list of objects</returns>
    public static async Task<List<S3Object>> ListObjectsAsync(string objectPrefix, string delimiter = null,
        IAwsSimpleStorageServiceClientConfiguration configuration = null, string marker = null, bool recursive = true)
    {
        // Localize our client into a disposable context
        using AmazonS3Client client = GetClient(configuration);

        // Grab the bucket and object prefix
        Tuple<string, string> bucketAndObjectPrefix = BucketAndObjectName(objectPrefix);

        // Define our objects container
        List<S3Object> objects = new();

        // Define our next-token container
        string nextToken;

        // Iterate over the objects
        do
        {
            // Define our request
            ListObjectsRequest request = new()
            {
                // Set the bucket name into the request
                BucketName = bucketAndObjectPrefix.Item1,

                // Set the object prefix into the request
                Prefix = bucketAndObjectPrefix.Item2
            };

            // Check for a delimiter and set it into the request
            if (delimiter is not null or "") request.Delimiter = delimiter;

            // Check for a marker and set it into the request
            if (marker is not null or "") request.Marker = marker;

            // List the objects from S3
            ListObjectsResponse objectsResponse = await client.ListObjectsAsync(request);

            // Add the objects to the list
            objects.AddRange(objectsResponse.S3Objects.Where(o => IsFile(o.Key)));

            // Check the response for truncation and get the rest of the objects
            if (objectsResponse.IsTruncated)
                objects.AddRange(await ListObjectsAsync(objectPrefix, delimiter, configuration,
                    objectsResponse.NextMarker, false));

            // Check for any common prefixes to list
            if (recursive && objectsResponse.CommonPrefixes.Any())
            {
                // Define our task list
                List<Task> tasks = new();

                // Iterate over the common prefixes and list them
                objectsResponse.CommonPrefixes.ForEach(p =>
                    Task.Run(async () => objects.AddRange(await ListObjectsAsync($"{objectsResponse.Name}/{p}"))));

                // Await all of our common prefix tasks
                await Task.WhenAll(tasks);
            }

            // Reset the token
            nextToken = objectsResponse.NextMarker;

        } while (nextToken is not null);

        // We're done, send the response
        return objects;
    }

    /// <summary>
    ///     This method asynchronously and recursively lists the objects matching <paramref name="objectPrefix" /> and maps them to documents <typeparamref name="TTarget" />
    /// </summary>
    /// <param name="objectPrefix">The prefix pattern to query S3 with</param>
    /// <param name="delimiter">Optional, directory or object delimiter</param>
    /// <param name="configuration">Optional configuration override instance</param>
    /// <param name="recursive">Optional, flag to denote recursive subdirectory listings</param>
    /// <typeparam name="TTarget">The output document type</typeparam>
    /// <returns>An awaitable task containing a list of <typeparamref name="TTarget" /> objects</returns>
    public static async Task<List<TTarget>> ListObjectsAsync<TTarget>(string objectPrefix, string delimiter = null,
        IAwsSimpleStorageServiceClientConfiguration configuration = null, bool recursive = true)
    {
        // List the objects from S3
        List<S3Object> objects = await ListObjectsAsync(objectPrefix, delimiter, configuration, null, recursive);

        // Localize the bucket and object name
        Tuple<string, string> bucketAndObjectName = BucketAndObjectName(objectPrefix);

        // Define our list of awaitable tasks
        List<Task> tasks = new();

        // Define our response
        List<TTarget> response = new();

        // Iterate over the objects
        objects.ForEach(o => tasks.Add(Task.Run(async () =>
            response.Add(await DownloadObjectAsync<TTarget>($"{bucketAndObjectName.Item1}/{o.Key}", configuration)))));

        // Await all of our tasks
        await Task.WhenAll(tasks);

        // We're done, send the response
        return response;
    }

    /// <summary>
    ///     This method asynchronously determines whether object <paramref name="objectName" /> exists or not
    /// </summary>
    /// <param name="objectName">The object to query</param>
    /// <param name="configuration">Optional configuration override instance</param>
    /// <returns>An awaitable task containing a boolean denoting whether the object exists or not</returns>
    public static async Task<bool> ObjectExistsAsync(string objectName,
        IAwsSimpleStorageServiceClientConfiguration configuration = null)
    {
        // Grab the bucket and object name
        Tuple<string, string> bucketAndObjectName = BucketAndObjectName(objectName);

        // Check for a bucket name
        if (string.IsNullOrEmpty(bucketAndObjectName.Item1) ||
            string.IsNullOrWhiteSpace(bucketAndObjectName.Item1)) return false;

        // Check for an object name
        if (string.IsNullOrEmpty(bucketAndObjectName.Item2) ||
            string.IsNullOrWhiteSpace(bucketAndObjectName.Item2)) return false;

        // Define our request
        GetObjectMetadataRequest request = new()
        {
            // Set the bucket name into the request
            BucketName = bucketAndObjectName.Item1,

            // Set the object name into the request
            Key = bucketAndObjectName.Item2
        };

        // Localize our client into a disposable context
        using AmazonS3Client client = GetClient(configuration);

        // Try to load the information
        try
        {
            // Make the request
            await client.GetObjectMetadataAsync(request);

            // We're done, the object exists
            return true;
        }
        catch (System.Exception)
        {
            // We're done, the object doesn't exist
            return false;
        }
    }

    /// <summary>
    ///     This method returns a pre-signed URL to <paramref name="objectName" /> object
    /// </summary>
    /// <param name="objectName">The object to get a pre-signed URL for</param>
    /// <param name="configuration">Option configuration override instance</param>
    /// <returns>The pre-signed URL to <paramref name="objectName" /> on Amazon S3</returns>
    public static string ObjectUrl(string objectName, IAwsSimpleStorageServiceClientConfiguration configuration = null)
    {
        // Grab the bucket and object name
        Tuple<string, string> bucketAndObjectName = BucketAndObjectName(objectName);

        // Check for a bucket name
        if (string.IsNullOrEmpty(bucketAndObjectName.Item1) ||
            string.IsNullOrWhiteSpace(bucketAndObjectName.Item1)) return null;

        // Check for a object name
        if (string.IsNullOrEmpty(bucketAndObjectName.Item2) ||
            string.IsNullOrWhiteSpace(bucketAndObjectName.Item2)) return null;

        // Localize our client into a disposable context
        using AmazonS3Client client = GetClient(configuration);

        // Define our request
        GetPreSignedUrlRequest request = new()
        {
            // Set the bucket name into the request
            BucketName = bucketAndObjectName.Item1,

            // Set the object name into the request
            Key = bucketAndObjectName.Item2,

            // Set the protocol into the request
            Protocol = Protocol.HTTPS,

            // Set the verb into the request
            Verb = HttpVerb.GET
        };

        // We're done, return the response
        return client.GetPreSignedURL(request);
    }

    /// <summary>
    ///     This method asynchronously uploads <paramref name="data" /> to <paramref name="objectName" />
    /// </summary>
    /// <param name="objectName">The target object to save <paramref name="data" /> to</param>
    /// <param name="data">The content of the object</param>
    /// <param name="configuration">Optional configuration override instance</param>
    /// <param name="metadata">Optional metadata for the object</param>
    /// <param name="acl">Optional access control for the object</param>
    /// <returns>An awaitable task with no result</returns>
    public static Task UploadAsync(string objectName, Stream data,
        IAwsSimpleStorageServiceClientConfiguration configuration = null, Dictionary<string, object> metadata = null,
        S3CannedACL acl = null)
    {
        // Default our ACL
        acl ??= S3CannedACL.Private;

        // Localize the bucket and object name
        Tuple<string, string> bucketAndObjectName = BucketAndObjectName(objectName);

        // Check for a bucket name
        if (string.IsNullOrEmpty(bucketAndObjectName.Item1) || string.IsNullOrWhiteSpace(bucketAndObjectName.Item1))
            return Task.CompletedTask;

        // Check for an object name
        if (string.IsNullOrEmpty(bucketAndObjectName.Item2) || string.IsNullOrWhiteSpace(bucketAndObjectName.Item2))
            return Task.CompletedTask;

        // Localize our upload request
        TransferUtilityUploadRequest request = new()
        {
            // Set the bucket into the request
            BucketName = bucketAndObjectName.Item1,

            // Set the ACL into the request
            CannedACL = acl,

            // Set our content into the request
            InputStream = data,

            // Set the object name into the request
            Key = bucketAndObjectName.Item2
        };

        // Configure the encryption for the object
        ConfigureEncryption(ref request, configuration);

        // Iterate over the provided metadata and add the values to the request
        metadata?.Keys.ToList().ForEach(k => request.Metadata.Add(k.Replace(" ", "-"), metadata[k]?.ToString()));

        // Localize our transfer utility into a disposable context
        using TransferUtility utility = GetTransferUtility(configuration);

        // We're done, upload the object and send the response
        return utility.UploadAsync(request);
    }

    /// <summary>
    ///     This method asynchronously uploads <paramref name="binary" /> to <paramref name="objectName" />
    /// </summary>
    /// <param name="objectName">The target object to save <paramref name="binary" /> to</param>
    /// <param name="binary">The binary content of the object</param>
    /// <param name="configuration">Optional configuration override instance</param>
    /// <param name="metadata">Optional metadata for the object</param>
    /// <param name="acl">Optional access control for the object</param>
    /// <returns>An awaitable task with no result</returns>
    public static Task UploadAsync(string objectName, byte[] binary,
        IAwsSimpleStorageServiceClientConfiguration configuration = null, Dictionary<string, object> metadata = null,
        S3CannedACL acl = null) =>
        UploadAsync(objectName, new MemoryStream(binary) as Stream, configuration, metadata, acl);

    /// <summary>
    ///     This method asynchronously uploads <paramref name="localPathOrContent" /> to <paramref name="objectName" />
    /// </summary>
    /// <param name="objectName">The target object to save <paramref name="localPathOrContent" /> to</param>
    /// <param name="localPathOrContent">The local path to the file, directory or the content to be uploaded</param>
    /// <param name="configuration">Optional configuration override instance</param>
    /// <param name="metadata">Optional metadata for the object</param>
    /// <param name="acl">Optional access control for the object</param>
    /// <returns>An awaitable task with no result</returns>
    public static async Task UploadAsync(string objectName, string localPathOrContent,
        IAwsSimpleStorageServiceClientConfiguration configuration = null, Dictionary<string, object> metadata = null,
        S3CannedACL acl = null)
    {
        // Check for a directory
        if (Directory.Exists(localPathOrContent))
        {
            // Instantiate the directory
            DirectoryInfo directory = new DirectoryInfo(localPathOrContent);

            // Iterate over the sub-directories and upload them
            foreach (DirectoryInfo subDirectory in directory.EnumerateDirectories())
                await UploadAsync($"{objectName}/{subDirectory.Name}", subDirectory.FullName, configuration, metadata,
                    acl);

            // Iterate over the files and upload them
            foreach (FileInfo file in directory.EnumerateFiles())
                await UploadAsync($"{objectName}/{file.Name}", file.FullName, configuration, metadata, acl);

            // We're done
            return;
        }

        // Check for a file
        if (File.Exists(localPathOrContent))
        {
            // Read the file from the local filesystem and upload it to S3
            await UploadAsync(objectName, await File.ReadAllBytesAsync(localPathOrContent), configuration, metadata,
                acl);

            // We're done
            return;
        }

        // We're done, convert the content to bytes and create the file in S3
        await UploadAsync(objectName, Encoding.UTF8.GetBytes(localPathOrContent ?? string.Empty), configuration,
            metadata, acl);
    }

    /// <summary>
    ///     This method asynchronously serializes <paramref name="content" /> then uploads it to <paramref name="objectName" /> on S3
    /// </summary>
    /// <param name="objectName">The target object to save <paramref name="content" /> to</param>
    /// <param name="content">The object to serialize and upload</param>
    /// <param name="configuration">Optional configuration override instance</param>
    /// <param name="metadata">Optional metadata for the object</param>
    /// <param name="acl">Optional access control for the object</param>
    /// <typeparam name="TSource">The expected source object type</typeparam>
    /// <returns>An awaitable task containing a void result</returns>
    public static Task UploadAsync<TSource>(string objectName, TSource content,
        IAwsSimpleStorageServiceClientConfiguration configuration = null, Dictionary<string, object> metadata = null,
        S3CannedACL acl = null) => UploadAsync(objectName,
        SerializerService.SerializePretty(content, (configuration ?? Configuration).SerializationFormat), configuration,
        metadata, acl);

    /// <summary>
    ///     This method asynchronously uploads a local file or directory, or a serialized string of data to S3 as <typeparamref name="TSource" />
    /// </summary>
    /// <param name="objectName">The object path to upload <paramref name="localDirectoryFileOrContent" /> to</param>
    /// <param name="localDirectoryFileOrContent">The local file or directory path, or a serialized string</param>
    /// <param name="configuration">Optional configuration override instance</param>
    /// <param name="metadata">Optional metadata for the object</param>
    /// <param name="acl">Optional access control for the object</param>
    /// <typeparam name="TSource">The expected source object type</typeparam>
    /// <returns>An awaitable task containing a void result</returns>
    public static async Task UploadAsync<TSource>(string objectName, string localDirectoryFileOrContent,
        IAwsSimpleStorageServiceClientConfiguration configuration = null, Dictionary<string, object> metadata = null,
        S3CannedACL acl = null)
    {
        // Define our tasks
        List<Task> tasks = new();

        // Check for a directory
        if (Directory.Exists(localDirectoryFileOrContent))
        {
            // Instantiate the directory
            DirectoryInfo directory = new DirectoryInfo(localDirectoryFileOrContent);

            // Iterate over the sub-directories and upload them
            foreach (DirectoryInfo subDirectory in directory.EnumerateDirectories())
                tasks.Add(UploadAsync<TSource>($"{objectName}/{subDirectory.Name}", subDirectory.FullName,
                    configuration, metadata, acl));

            // Iterate over the files and upload them
            foreach (FileInfo file in directory.EnumerateFiles())
                tasks.Add(UploadAsync($"{objectName}/{file.Name}", await File.ReadAllTextAsync(file.FullName),
                    configuration, metadata, acl));
        }

        // Check for a file
        else if (File.Exists(localDirectoryFileOrContent))
            tasks.Add(UploadAsync(objectName, await File.ReadAllTextAsync(localDirectoryFileOrContent), configuration,
                metadata, acl));

        // We're done, convert the content to bytes and create the file in S3
        else tasks.Add(UploadAsync(objectName, localDirectoryFileOrContent, configuration, metadata, acl));

        // Await all of the tasks
        await Task.WhenAll(tasks);
    }

    /// <summary>
    ///     This method resets the configuration for the client
    /// </summary>
    /// <param name="configuration">The global configuration override instance</param>
    public static void WithConfiguration(IAwsSimpleStorageServiceClientConfiguration configuration) =>
        Configuration = configuration;
}
