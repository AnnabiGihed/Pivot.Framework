using FluentAssertions;
using Pivot.Framework.Infrastructure.Abstraction.Search;

namespace Pivot.Framework.Infrastructure.Abstraction.Tests.Search;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Unit tests for <see cref="SearchRequest"/> and <see cref="SearchResult{TDocument}"/>.
///              Verifies default values and property assignment for search models.
/// </summary>
public class SearchModelsTests
{
	#region SearchRequest Tests

	[Fact]
	public void SearchRequest_ShouldHaveDefaults()
	{
		var request = new SearchRequest();

		request.Query.Should().BeEmpty();
		request.Fields.Should().BeNull();
		request.Filters.Should().BeNull();
		request.From.Should().Be(0);
		request.Size.Should().Be(20);
		request.SortBy.Should().BeNull();
		request.SortAscending.Should().BeTrue();
	}

	[Fact]
	public void SearchRequest_ShouldSetAllProperties()
	{
		var request = new SearchRequest
		{
			Query = "test query",
			Fields = new[] { "Name", "Description" },
			Filters = new Dictionary<string, string[]> { ["Status"] = new[] { "Active" } },
			From = 10,
			Size = 50,
			SortBy = "Name",
			SortAscending = false
		};

		request.Query.Should().Be("test query");
		request.Fields.Should().HaveCount(2);
		request.Filters.Should().ContainKey("Status");
		request.From.Should().Be(10);
		request.Size.Should().Be(50);
		request.SortBy.Should().Be("Name");
		request.SortAscending.Should().BeFalse();
	}

	#endregion

	#region SearchResult Tests

	[Fact]
	public void SearchResult_ShouldHaveDefaults()
	{
		var result = new SearchResult<string>();

		result.Documents.Should().BeEmpty();
		result.TotalCount.Should().Be(0);
		result.From.Should().Be(0);
		result.Size.Should().Be(0);
	}

	[Fact]
	public void SearchResult_ShouldSetAllProperties()
	{
		var result = new SearchResult<string>
		{
			Documents = new[] { "doc1", "doc2" },
			TotalCount = 100,
			From = 0,
			Size = 2
		};

		result.Documents.Should().HaveCount(2);
		result.TotalCount.Should().Be(100);
		result.From.Should().Be(0);
		result.Size.Should().Be(2);
	}

	#endregion
}
