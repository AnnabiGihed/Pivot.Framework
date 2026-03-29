using FluentAssertions;
using Pivot.Framework.Infrastructure.Abstraction.ObjectStorage;

namespace Pivot.Framework.Infrastructure.Abstraction.Tests.ObjectStorage;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Unit tests for <see cref="ObjectInfo"/>.
///              Verifies default values and property assignment for object storage metadata.
/// </summary>
public class ObjectInfoTests
{
	[Fact]
	public void ObjectInfo_ShouldHaveDefaults()
	{
		var info = new ObjectInfo();

		info.Key.Should().BeEmpty();
		info.Size.Should().Be(0);
		info.ContentType.Should().BeEmpty();
		info.LastModifiedUtc.Should().Be(default);
	}

	[Fact]
	public void ObjectInfo_ShouldSetAllProperties()
	{
		var timestamp = new DateTime(2026, 3, 15, 10, 30, 0, DateTimeKind.Utc);
		var info = new ObjectInfo
		{
			Key = "exports/batch-2026-03.zip",
			Size = 1024 * 1024,
			ContentType = "application/zip",
			LastModifiedUtc = timestamp
		};

		info.Key.Should().Be("exports/batch-2026-03.zip");
		info.Size.Should().Be(1048576);
		info.ContentType.Should().Be("application/zip");
		info.LastModifiedUtc.Should().Be(timestamp);
	}
}
