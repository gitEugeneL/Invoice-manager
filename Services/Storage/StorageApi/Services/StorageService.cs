using Minio;
using Minio.DataModel.Args;

namespace StorageApi.Services;

public interface IStorageService
{
    Task<bool> BucketExists(string name);
    
    Task CreateBucket(string name);
    
    Task UploadFile(string bucketName, string fileName, byte[] fileData);
}

public class StorageService(IMinioClient client) : IStorageService
{
    public async Task<bool> BucketExists(string name)
    {
        return await client.BucketExistsAsync(
            new BucketExistsArgs()
                .WithBucket(name)
        );
    }
    
    public Task<bool> FileExists(string bucketName, string fileName)
    {
        var names = new List<string>();
        var args = new ListObjectsArgs()
            .WithBucket(bucketName)
            .WithRecursive(true);
        var observable = client.ListObjectsAsync(args);
        var subscription = observable.Subscribe(item => names.Add(item.Key));
        
        return Task.FromResult(names.Contains(fileName));
    }
    
    public async Task CreateBucket(string name)
    {
        await client.MakeBucketAsync(
            new MakeBucketArgs()
                .WithBucket(name)
        );
    }
    
    public  async Task UploadFile(string bucketName, string fileName, byte[] fileData)
    {
        var stream = new MemoryStream(fileData);
        var size = fileData.Length;
            
        await client.PutObjectAsync(
            new PutObjectArgs()
                .WithBucket(bucketName)
                .WithObject(fileName)
                .WithStreamData(stream)
                .WithObjectSize(size)
                .WithContentType("application/pdf")
        );
    }
}