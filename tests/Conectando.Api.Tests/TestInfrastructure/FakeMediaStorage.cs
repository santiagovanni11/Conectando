using Conectando.Api.Exceptions;
using Conectando.Api.Interfaces;

namespace Conectando.Api.Tests.TestInfrastructure;

public class FakeMediaStorage : IMediaStorage
{
    public List<string> Destroyed { get; } = [];
    public List<string> Uploaded { get; } = [];
    public int FailOnUploadAfter { get; set; }

    public Task<StoredMedia> UploadAsync(Stream stream, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        Uploaded.Add(fileName);

        if (FailOnUploadAfter > 0 && Uploaded.Count > FailOnUploadAfter)
        {
            throw new MediaStorageException();
        }

        var id = Guid.NewGuid().ToString("N");
        return Task.FromResult(new StoredMedia($"https://media.test/{id}.jpg", $"posts/{id}"));
    }

    public Task DeleteAsync(string publicId, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(publicId))
        {
            Destroyed.Add(publicId);
        }

        return Task.CompletedTask;
    }
}