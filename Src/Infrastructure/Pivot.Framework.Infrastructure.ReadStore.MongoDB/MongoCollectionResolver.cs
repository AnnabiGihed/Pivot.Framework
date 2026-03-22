using MongoDB.Driver;

namespace Pivot.Framework.Infrastructure.ReadStore.MongoDB;

/// <summary>
/// Resolves MongoDB collections using the type name as the collection name convention.
/// </summary>
internal static class MongoCollectionResolver
{
	/// <summary>
	/// Returns the <see cref="IMongoCollection{T}"/> for the specified type,
	/// using the type name as the collection name.
	/// </summary>
	/// <typeparam name="T">The document type.</typeparam>
	/// <param name="database">The MongoDB database.</param>
	/// <returns>The MongoDB collection for type <typeparamref name="T"/>.</returns>
	public static IMongoCollection<T> GetCollection<T>(IMongoDatabase database)
		=> database.GetCollection<T>(typeof(T).Name);
}
