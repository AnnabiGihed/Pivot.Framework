namespace Templates.Core.Domain.Primitives;

public abstract class ProjectionRoot<TId>
{
	public virtual TId Id { get; protected set; }

	public ProjectionRoot(TId id)
	{
		Id = id ?? throw new ArgumentNullException(nameof(id));
	}
}
