namespace Pivot.Framework.Infrastructure.Abstraction.Persistence;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 01-2026
/// Purpose     : Marker interface for persistence contexts (e.g., EF Core DbContext).
///              Used as a generic constraint in infrastructure abstractions to avoid
///              coupling the abstraction layer to a specific ORM.
/// </summary>
public interface IPersistenceContext { }
