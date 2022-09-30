using System.Text;
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
/// This class maintains the structure of our S3 client
/// </summary>
public class AwsSimpleStorageServiceClient
{
    /// <summary>
    /// This property contains the instance of our configuration
    /// </summary>
    public static IAwsSimpleStorageServiceClientConfiguration Configuration { get; private set; }

    /// <summary>
    /// This method generates an authenticated AWS client
    /// </summary>
    /// <param name="configuration">Optional configuration override instance</param>
    /// <returns>An authenticated AWS S3 Client</returns>
    public static AmazonS3Client GetClient(IAwsSimpleStorageServiceClientConfiguration configuration = null) => new(
        new BasicAWSCredentials((configuration ?? Configuration)?.AccessKeyId,
            (configuration ?? Configuration)?.SecretAccessKey),
        RegionEndpoint.GetBySystemName((configuration ?? Configuration)?.Region));

    /// <summary>
    /// This property contains the instance of our transfer utility
    /// </summary>
    /// <param name="configuration">Optional configuration override instance</param>
    /// <returns>An authenticated AWS S3 Transfer Utility</returns>
    public static TransferUtility GetTransferUtility(IAwsSimpleStorageServiceClientConfiguration configuration = null) =>
        new(GetClient(configuration));

    /// <summary>
    /// This method resets the configuration for the client
    /// </summary>
    /// <param name="configuration">The global configuration override instance</param>
    public static void WithConfiguration(IAwsSimpleStorageServiceClientConfiguration configuration) =>
        Configuration = configuration;

    /// <summary>
    /// This method instantiates our client
    /// </summary>
    public AwsSimpleStorageServiceClient()
    {
    }

    /// <summary>
    /// This method instantiates our client with a configuration object
    /// </summary>
    /// <param name="configuration">The configuration object with AWS credentials and region</param>
    public AwsSimpleStorageServiceClient(IAwsSimpleStorageServiceClientConfiguration configuration) =>
        WithConfiguration(configuration);

    /// <summary>
    /// This method configures the encryption for an upload
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
    /// This method determines the bucket and name of an object
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
        List<string> parts = objectName.Split('/', 2, StringSplitOptions.TrimEntries).ToList();

        // Check for parts
        if (!parts.Any()) return new(objectName, string.Empty);

        // We're done, return the bucket and objet name
        return new(parts.FirstOrDefault(), parts.LastOrDefault());
    }

    /// <summary>
    /// This method asynchronously determines whether or not a bucket <paramref name="objectName" /> exists
    /// </summary>
    /// <param name="objectName">The object to query</param>
    /// <param name="configuration">Optional configuration override instance</param>
    /// <returns>An awaitable task containing a boolean denoting the existence of the bucket</returns>
    public static async Task<bool> BucketExistsAsync(string objectName, IAwsSimpleStorageServiceClientConfiguration configuration = null)
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
    /// This method asynchronously copies <paramref name="sourceObjectName" /> to <paramref name="targetObjectName" /> in Amazon S3
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
    /// This method asynchronously deletes <paramref name="objectName" /> from S3 if it exists
    /// </summary>
    /// <param name="objectName">The object to delete if it exists</param>
    /// <param name="configuration">Optional configuration override instance</param>
    /// <returns>An awaitable task containing the AWS SDK object-delete response</returns>
    public static async Task<DeleteObjectResponse> DeleteFileIfExistsAsync(string objectName,
        IAwsSimpleStorageServiceClientConfiguration configuration = null)
    {
        // Make sure the file exists and return
        if (!await ObjectExistsAsync(objectName)) return null;

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
    /// This method asynchronously downloads object <paramref name="objectName" /> from S3
    /// </summary>
    /// <param name="objectName">The object to download</param>
    /// <param name="configuration">Optional configuration override instance</param>
    /// <returns>An awaitable task containing a stream of the object's contents</returns>
    /// <exception cref="AwsSimpleStorageServiceObjectNotFoundException">Thrown when the object doesn't exist</exception>
    public static async Task<Stream> DownloadObjectAsync(string objectName, IAwsSimpleStorageServiceClientConfiguration configuration = null)
    {
        // Make sure the file exists and return
        if (!await ObjectExistsAsync(objectName)) throw new AwsSimpleStorageServiceObjectNotFoundException("S3 Object Not Found");

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
    /// This method asynchronously downloads object <paramref name="objectName" /> and deserializes it from <paramref name="format" /> into <typeparamref name="TTarget" />
    /// </summary>
    /// <param name="objectName">The object to download</param>
    /// <param name="format">The serialization format of the object</param>
    /// <param name="configuration">Optional configuration override instance</param>
    /// <typeparam name="TTarget">The target deserialization type</typeparam>
    /// <returns>An awaitable task contain the deserialized object</returns>
    public static async Task<TTarget> DownloadObjectAsync<TTarget>(string objectName,
        SerializerFormat format = SerializerFormat.Json, IAwsSimpleStorageServiceClientConfiguration configuration = null)
    {

        // Download the object stream into a disposable context
        await using Stream objectStream = await DownloadObjectAsync(objectName, configuration);

        // Localize our stream reader into a disposable context
        using StreamReader objectStreamReader = new StreamReader(objectStream, Encoding.UTF8);

        // Read the object's content
        string objectContents = await objectStreamReader.ReadToEndAsync();

        // We're done, check the format and deserialize the object's content
        return format is SerializerFormat.Xml
            ? XmlSerializer.Deserialize<TTarget>(objectContents)
            : JsonSerializer.Deserialize<TTarget>(objectContents);
    }

    /// <summary>
    /// This method determines whether or not <paramref name="objectName" /> is a directory or not
    /// </summary>
    /// <param name="objectName">The object name or path to query</param>
    /// <returns>A boolean denoting whether <paramref name="objectName" /> is a directory or not</returns>
    public static bool IsDirectory(string objectName) => objectName.Trim().EndsWith("/");

    /// <summary>
    /// This method determines whether or not <paramref name="objectName" /> is a file or not
    /// </summary>
    /// <param name="objectName">The object name or path to query</param>
    /// <returns>A boolean denoting whether <paramref name="objectName" /> is a file or not</returns>
    public static bool IsFile(string objectName) => !IsDirectory(objectName);

    /// <summary>
    /// This method asynchronously and recursively lists the objects matching <paramref name="objectPrefix" />
    /// </summary>
    /// <param name="objectPrefix">The prefix pattern to query S3 with</param>
    /// <param name="configuration">Optional configuration override instance</param>
    /// <returns>An awaitable task containing a list of objects</returns>
    public static async Task<List<S3Object>> ListObjectsAsync(string objectPrefix, IAwsSimpleStorageServiceClientConfiguration configuration = null)
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
            ListObjectsRequest request = new ListObjectsRequest()
            {
                // Set the bucket name into the request
                BucketName = bucketAndObjectPrefix.Item1,
                // Set the object prefix into the request
                Prefix = bucketAndObjectPrefix.Item2
            };

            // List the objects from S3
            ListObjectsResponse objectsResponse = await client.ListObjectsAsync(request);

            // Add the objects to the list
            objects.AddRange(objectsResponse.S3Objects.Where(o => IsFile(o.Key)));

            // Reset the token
            nextToken = objectsResponse.NextMarker;

        } while (nextToken is not null);


        // We're done, send the response
        return objects;
    }

    /// <summary>
    /// This method asynchronously and recursively lists the objects matching <paramref name="objectPrefix" /> and maps them to documents <typeparamref name="TOutput" />
    /// </summary>
    /// <param name="objectPrefix">The prefix pattern to query S3 with</param>
    /// <param name="format">The serialization format of the document in S3</param>
    /// <param name="configuration">Optional configuration override instance</param>
    /// <typeparam name="TOutput">The output document type</typeparam>
    /// <returns></returns>
    public static async Task<List<TOutput>> ListObjectsAsync<TOutput>(string objectPrefix,
        SerializerFormat format = SerializerFormat.Json, IAwsSimpleStorageServiceClientConfiguration configuration = null)
    {
        // List the objects from S3
        List<S3Object> objects = await ListObjectsAsync(objectPrefix, configuration);
        // Define our list of awaitables
        List<Task> tasks = new();
        // Define our response
        List<TOutput> response = new();

        // Iterate over the objects
        objects.ForEach(o => tasks.Add(Task.Run(async () =>
        {
            // Check for a file object and download the document from S3 and add it to the response
            if (IsFile(o.Key))
                response.Add(await DownloadObjectAsync<TOutput>(o.Key, format, configuration));

            // Otherwise, list the objects in the directory and add them to the response
            else
                response.AddRange(await ListObjectsAsync<TOutput>(o.Key, format, configuration));
        })));

        // Await all of our tasks
        await Task.WhenAll(tasks);

        // We're done, send the response
        return response;
    }

    /// <summary>
    /// This method asynchronously determines whether object <paramref name="objectName" /> exists or not
    /// </summary>
    /// <param name="objectName">The object to query</param>
    /// <param name="configuration">Optional configuration override instance</param>
    /// <returns>An awaitable task containing a boolean denoting whether the object exists or not</returns>
    public static async Task<bool> ObjectExistsAsync(string objectName, IAwsSimpleStorageServiceClientConfiguration configuration = null)
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
    /// This method returns a pre-signed URL to <paramref name="objectName" /> object
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
    /// This method asynchronously uploads <paramref name="data" /> to <paramref name="objectName" />
    /// </summary>
    /// <param name="objectName">The target object to save <paramref name="data" /> to</param>
    /// <param name="data">The content of the object</param>
    /// <param name="metadata">Optional metadata for the object</param>
    /// <param name="acl">Optional access control for the object</param>
    /// <param name="configuration">Optional configuration override instance</param>
    /// <returns>An awaitable task with no result</returns>
    public static Task UploadAsync(string objectName, Stream data, MetadataCollection metadata = null,
        S3CannedACL acl = null, IAwsSimpleStorageServiceClientConfiguration configuration = null)
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

        // Iterate over the metadata keys
        foreach (string key in metadata?.Keys ?? new string[] { })
            if (!string.IsNullOrEmpty(metadata?[key]) && !string.IsNullOrWhiteSpace(metadata[key]))
                request.Metadata.Add(key, metadata[key]);

        // Localize our transfer utility into a disposable context
        using TransferUtility utility = GetTransferUtility(configuration);

        // We're done, upload the object and send the response
        return utility.UploadAsync(request);
    }

    /// <summary>
    /// This method asynchronously uploads <paramref name="binary" /> to <paramref name="objectName" />
    /// </summary>
    /// <param name="objectName">The target object to save <paramref name="binary" /> to</param>
    /// <param name="binary">The binary content of the object</param>
    /// <param name="metadata">Optional metadata for the object</param>
    /// <param name="acl">Optional access control for the object</param>
    /// <param name="configuration">Optional configuration override instance</param>
    /// <returns>An awaitable task with no result</returns>
    public static Task UploadAsync(string objectName, byte[] binary, MetadataCollection metadata = null,
        S3CannedACL acl = null, IAwsSimpleStorageServiceClientConfiguration configuration = null) =>
        UploadAsync(objectName, new MemoryStream(binary), metadata, acl, configuration);

    /// <summary>
    /// This method asynchronously uploads <paramref name="localPathOrContent" /> to <paramref name="objectName" />
    /// </summary>
    /// <param name="objectName">The target object to save <paramref name="localPathOrContent" /> to</param>
    /// <param name="localPathOrContent">The local path to the file, directory or the content to be uploaded</param>
    /// <param name="metadata">Optional metadata for the object</param>
    /// <param name="acl">Optional access control for the object</param>
    /// <param name="configuration">Optional configuration override instance</param>
    /// <returns>An awaitable task with no result</returns>
    public static async Task UploadAsync(string objectName, string localPathOrContent,
        MetadataCollection metadata = null, S3CannedACL acl = null, IAwsSimpleStorageServiceClientConfiguration configuration = null)
    {
        // Check for a directory
        if (Directory.Exists(localPathOrContent))
        {
            // Instantiate the directory
            DirectoryInfo directory = new DirectoryInfo(localPathOrContent);

            // Iterate over the sub-directories and upload them
            foreach (DirectoryInfo subDirectory in directory.EnumerateDirectories())
                await UploadAsync($"{objectName}/{subDirectory.Name}", subDirectory.FullName, metadata, acl,
                    configuration);

            // Iterate over the files and upload them
            foreach (FileInfo file in directory.EnumerateFiles())
                await UploadAsync($"{objectName}/{file.Name}", file.FullName, metadata, acl, configuration);

            // We're done
            return;
        }

        // Check for a file
        if (File.Exists(localPathOrContent))
        {
            // Read the file from the local filesystem and upload it to S3
            await UploadAsync(objectName, await File.ReadAllBytesAsync(localPathOrContent), metadata, acl,
                configuration);

            // We're done
            return;
        }

        // We're done, convert the content to bytes and create the file in S3
        await UploadAsync(objectName, Encoding.UTF8.GetBytes(localPathOrContent ?? string.Empty), metadata, acl,
            configuration);
    }

    /// <summary>
    /// This method asynchronously serializes <paramref name="content" /> into <paramref name="format" /> then uploads it to <paramref name="objectName" /> on S3
    /// </summary>
    /// /// <param name="objectName">The target object to save <paramref name="content" /> to</param>
    /// <param name="content">The object to serialize and upload</param>
    /// <param name="metadata">Optional metadata for the object</param>
    /// <param name="acl">Optional access control for the object</param>
    /// <param name="format">The serialization format of the object</param>
    /// <param name="configuration">Optional configuration override instance</param>
    /// <typeparam name="TSource"></typeparam>
    /// <returns></returns>
    public static Task UploadAsync<TSource>(string objectName, TSource content, MetadataCollection metadata = null,
        S3CannedACL acl = null, SerializerFormat format = SerializerFormat.Json,
        IAwsSimpleStorageServiceClientConfiguration configuration = null) => UploadAsync(objectName,
        format is SerializerFormat.Xml
            ? XmlSerializer.SerializePretty(content)
            : JsonSerializer.SerializePretty(content), metadata, acl, configuration);

    /// <summary>
    /// This method asynchronously uploads a local file or directory, or a serialized string of data to S3 as <typeparamref name="TSource" />
    /// </summary>
    /// <param name="objectName">The object path to upload <paramref name="localDirectoryFileOrContent" /> to</param>
    /// <param name="localDirectoryFileOrContent">The local file or directory path, or a serialized string</param>
    /// <param name="metadata">Optional metadata for the object</param>
    /// <param name="acl">Optional access control for the object</param>
    /// <param name="format">The serialization format of the object</param>
    /// <param name="configuration">Optional configuration override instance</param>
    /// <typeparam name="TSource"></typeparam>
    public static async Task UploadAsync<TSource>(string objectName, string localDirectoryFileOrContent,
        MetadataCollection metadata = null, S3CannedACL acl = null, SerializerFormat format = SerializerFormat.Json,
        IAwsSimpleStorageServiceClientConfiguration configuration = null)
    {
        // Check for a directory
        if (Directory.Exists(localDirectoryFileOrContent))
        {
            // Instantiate the directory
            DirectoryInfo directory = new DirectoryInfo(localDirectoryFileOrContent);

            // Iterate over the sub-directories and upload them
            foreach (DirectoryInfo subDirectory in directory.EnumerateDirectories())
                await UploadAsync<TSource>($"{objectName}/{subDirectory.Name}", subDirectory.FullName, metadata, acl,
                    format, configuration);

            // Iterate over the files and upload them
            foreach (FileInfo file in directory.EnumerateFiles())
                await UploadAsync($"{objectName}/{file.Name}", await File.ReadAllTextAsync(file.FullName), metadata,
                    acl, configuration);

            // We're done
            return;
        }

        // Check for a file
        if (File.Exists(localDirectoryFileOrContent))
        {
            // Read the file from the local filesystem and upload it to S3
            await UploadAsync(objectName, await File.ReadAllTextAsync(localDirectoryFileOrContent), metadata, acl,
                configuration);

            // We're done
            return;
        }

        // We're done, convert the content to bytes and create the file in S3
        await UploadAsync(objectName, localDirectoryFileOrContent, metadata, acl, configuration);
    }
}
