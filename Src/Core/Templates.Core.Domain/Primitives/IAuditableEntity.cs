using CSharpFunctionalExtensions;
using System.ComponentModel.DataAnnotations.Schema;

namespace Templates.Core.Domain.Primitives;

public interface IAuditableEntity
{
	AuditInfo Audit { get; }
}


[ComplexType]
public class AuditInfo : ValueObject<AuditInfo>
{
	public string? CreatedBy { get; set; } = default!;
	public string? ModifiedBy { get; set; } = default!;
	public DateTime CreatedOnUtc { get; set; }
	public DateTime? ModifiedOnUtc { get; set; } = default!;

	public static AuditInfo Create(DateTime date, string author)
	{

		if (date == DateTime.MaxValue || date == DateTime.MinValue)
			throw new ArgumentException(nameof(date));

		if (string.IsNullOrEmpty(author))
			throw new ArgumentNullException(nameof(author));

		return new AuditInfo
		{
			CreatedOnUtc = date,
			ModifiedOnUtc = date,
			CreatedBy = author,
			ModifiedBy = author
		};
	}

	public void Modify(DateTime date, string author)
	{

		if (date == DateTime.MaxValue || date == DateTime.MinValue)
			throw new ArgumentException(nameof(date));

		if (string.IsNullOrEmpty(author))
			throw new ArgumentNullException(nameof(author));

		ModifiedBy = author;
		ModifiedOnUtc = date;
	}

	protected override bool EqualsCore(AuditInfo other)
	{
		return this == other;
	}

	protected override int GetHashCodeCore()
	{
		return this.GetHashCodeCore();
	}
}