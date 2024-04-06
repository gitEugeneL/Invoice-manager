using System.Reactive.Linq;
using Minio;
using Minio.DataModel.Args;

namespace StorageApi.Services;

public interface IStorageService
{
    Task<bool> BucketExists(string name);
    
    Task CreateBucket(string name);
    
    Task UploadFile(string bucketName, string fileName, byte[] fileData);

    Task<MemoryStream> DownloadFile(string bucketName, string fileName);

    Task<List<string>> GetObjectsList(string bucketName);
}

internal class StorageService(IMinioClient client) : IStorageService
{
    public async Task<bool> BucketExists(string name)
    {
        return await client.BucketExistsAsync(
            new BucketExistsArgs()
                .WithBucket(name.ToLower())
        );
    }
    
    public async Task CreateBucket(string name)
    {
        await client.MakeBucketAsync(
            new MakeBucketArgs()
                .WithBucket(name.ToLower())
        );
    }
    
    public  async Task UploadFile(string bucketName, string fileName, byte[] fileData)
    {
        var stream = new MemoryStream(fileData);
        var size = fileData.Length;
            
        await client.PutObjectAsync(
            new PutObjectArgs()
                .WithBucket(bucketName.ToLower())
                .WithObject(fileName.ToLower())
                .WithStreamData(stream)
                .WithObjectSize(size)
                .WithContentType("application/pdf")
        );
    }

    public async Task<MemoryStream> DownloadFile(string bucketName, string fileName)
    {
        var stream = new MemoryStream();
        var tsc = new TaskCompletionSource<bool>();

        var getObjectArgs = new GetObjectArgs()
            .WithBucket(bucketName.ToLower())
            .WithObject(fileName.ToLower())
            .WithCallbackStream(cs =>
            {
                cs.CopyTo(stream);
                tsc.SetResult(true);
            });
        await client.GetObjectAsync(getObjectArgs);
        await tsc.Task;
        stream.Seek(0, SeekOrigin.Begin);
        return stream;
    }

    public async Task<List<string>> GetObjectsList(string bucketName)
    {
        List<string> itemNames = [];

        var args = new ListObjectsArgs()
            .WithBucket(bucketName.ToLower());
        var observable = client.ListObjectsAsync(args);
        
        await observable.ForEachAsync(item => itemNames.Add(item.Key));
        
        return itemNames;
    }
}