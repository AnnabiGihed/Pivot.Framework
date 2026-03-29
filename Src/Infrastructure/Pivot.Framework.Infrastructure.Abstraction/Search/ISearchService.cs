using Pivot.Framework.Domain.Shared;

namespace Pivot.Framework.Infrastructure.Abstraction.Search;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Abstraction for full-text search operations backed by OpenSearch/Elasticsearch.
///              Provides indexing, searching, and index management for read-side search projections.
/// </summary>
/// <typeparam name="TDocument">The document type stored in the search index.</typeparam>
public interface ISearchService<TDocument> where TDocument : class
{
	/// <summary>Indexes a single document. Creates or updates based on document ID.</summary>
	Task<Result> IndexAsync(string indexName, string documentId, TDocument document, CancellationToken ct = default);

	/// <summary>Indexes multiple documents in bulk.</summary>
	Task<Result> BulkIndexAsync(string indexName, IEnumerable<(string Id, TDocument Document)> documents, CancellationToken ct = default);

	/// <summary>Searches for documents matching the query string.</summary>
	Task<SearchResult<TDocument>> SearchAsync(string indexName, SearchRequest request, CancellationToken ct = default);

	/// <summary>Deletes a document by ID.</summary>
	Task<Result> DeleteAsync(string indexName, string documentId, CancellationToken ct = default);

	/// <summary>Creates an index with the specified settings and mappings.</summary>
	Task<Result> CreateIndexAsync(string indexName, CancellationToken ct = default);

	/// <summary>Deletes an index entirely (used during projection rebuild).</summary>
	Task<Result> DeleteIndexAsync(string indexName, CancellationToken ct = default);

	/// <summary>Checks if an index exists.</summary>
	Task<bool> IndexExistsAsync(string indexName, CancellationToken ct = default);
}

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Search request model for full-text and faceted queries.
/// </summary>
public sealed class SearchRequest
{
	/// <summary>The search query string.</summary>
	public string Query { get; init; } = string.Empty;

	/// <summary>Fields to search across. Null means all fields.</summary>
	public string[]? Fields { get; init; }

	/// <summary>Filter conditions (field name → values).</summary>
	public Dictionary<string, string[]>? Filters { get; init; }

	/// <summary>Zero-based offset for pagination.</summary>
	public int From { get; init; }

	/// <summary>Maximum number of results to return. Defaults to 20.</summary>
	public int Size { get; init; } = 20;

	/// <summary>Sort field and direction.</summary>
	public string? SortBy { get; init; }

	/// <summary>Sort ascending (true) or descending (false).</summary>
	public bool SortAscending { get; init; } = true;
}

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Search result wrapper with pagination metadata.
/// </summary>
public sealed class SearchResult<TDocument>
{
	public IReadOnlyList<TDocument> Documents { get; init; } = Array.Empty<TDocument>();
	public long TotalCount { get; init; }
	public int From { get; init; }
	public int Size { get; init; }
}
