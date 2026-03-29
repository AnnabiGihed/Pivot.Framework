using Pivot.Framework.Domain.Shared;

namespace Pivot.Framework.Infrastructure.Abstraction.ObjectStorage;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Abstraction for object/blob storage operations.
///              Supports MinIO, S3-compatible, and filesystem backends.
///              Used for raw payload archives, export bundles, and large file storage.
/// </summary>
public interface IObjectStorageService
{
	/// <summary>Uploads an object to the specified bucket and key.</summary>
	Task<Result> UploadAsync(string bucket, string key, Stream content, string contentType, CancellationToken ct = default);

	/// <summary>Downloads an object by bucket and key.</summary>
	Task<Result<Stream>> DownloadAsync(string bucket, string key, CancellationToken ct = default);

	/// <summary>Deletes an object by bucket and key.</summary>
	Task<Result> DeleteAsync(string bucket, string key, CancellationToken ct = default);

	/// <summary>Checks if an object exists.</summary>
	Task<bool> ExistsAsync(string bucket, string key, CancellationToken ct = default);

	/// <summary>Lists objects in a bucket with an optional prefix filter.</summary>
	Task<IReadOnlyList<ObjectInfo>> ListAsync(string bucket, string? prefix = null, CancellationToken ct = default);

	/// <summary>Ensures a bucket exists, creating it if necessary.</summary>
	Task<Result> EnsureBucketExistsAsync(string bucket, CancellationToken ct = default);
}

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Metadata about a stored object.
/// </summary>
public sealed class ObjectInfo
{
	public string Key { get; init; } = string.Empty;
	public long Size { get; init; }
	public string ContentType { get; init; } = string.Empty;
	public DateTime LastModifiedUtc { get; init; }
}
