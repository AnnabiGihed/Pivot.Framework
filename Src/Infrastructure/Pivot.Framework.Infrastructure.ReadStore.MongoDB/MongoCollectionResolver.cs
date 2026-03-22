using MongoDB.Driver;

namespace Pivot.Framework.Infrastructure.ReadStore.MongoDB;

internal static class MongoCollectionResolver
{
	public static IMongoCollection<T> GetCollection<T>(IMongoDatabase database)
		=> database.GetCollection<T>(typeof(T).Name);
}
