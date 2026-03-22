using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Pivot.Framework.Application.Abstractions.ReadModels;

namespace Pivot.Framework.Infrastructure.ReadStore.MongoDB.Extensions;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : DI registration extensions for MongoDB read model infrastructure.
///              Registers <see cref="MongoReadModelRepository{TReadModel,TId}"/> and
///              <see cref="MongoReadModelStore{TReadModel,TId}"/> as the implementations
///              for <see cref="IReadModelRepository{TReadModel,TId}"/> and
///              <see cref="IReadModelStore{TReadModel,TId}"/> respectively.
///
///              Usage:
///              <code>
///              services.AddMongoReadModelStore("mongodb://localhost:27017", "myapp_reads");
///              </code>
/// </summary>
public static class MongoReadModelExtensions
{
	/// <summary>
	/// Registers MongoDB implementations of <see cref="IReadModelRepository{TReadModel,TId}"/>
	/// and <see cref="IReadModelStore{TReadModel,TId}"/>.
	/// </summary>
	/// <param name="services">The service collection to register into.</param>
	/// <param name="connectionString">The MongoDB connection string.</param>
	/// <param name="databaseName">The MongoDB database name for read models.</param>
	/// <returns>The same <paramref name="services"/> instance for chaining.</returns>
	/// <exception cref="ArgumentException">
	/// Thrown when <paramref name="connectionString"/> or <paramref name="databaseName"/>
	/// is null or whitespace.
	/// </exception>
	public static IServiceCollection AddMongoReadModelStore(
		this IServiceCollection services,
		string connectionString,
		string databaseName)
	{
		if (string.IsNullOrWhiteSpace(connectionString))
			throw new ArgumentException("Connection string must not be null or whitespace.", nameof(connectionString));
		if (string.IsNullOrWhiteSpace(databaseName))
			throw new ArgumentException("Database name must not be null or whitespace.", nameof(databaseName));

		services.AddSingleton<IMongoClient>(new MongoClient(connectionString));
		services.AddSingleton(sp => sp.GetRequiredService<IMongoClient>().GetDatabase(databaseName));
		services.AddScoped(typeof(IReadModelRepository<,>), typeof(MongoReadModelRepository<,>));
		services.AddScoped(typeof(IReadModelStore<,>), typeof(MongoReadModelStore<,>));

		return services;
	}

	/// <summary>
	/// Registers MongoDB implementations of <see cref="IReadModelRepository{TReadModel,TId}"/>
	/// and <see cref="IReadModelStore{TReadModel,TId}"/> using pre-configured <see cref="MongoClientSettings"/>.
	/// </summary>
	/// <param name="services">The service collection to register into.</param>
	/// <param name="settings">The MongoDB client settings.</param>
	/// <param name="databaseName">The MongoDB database name for read models.</param>
	/// <returns>The same <paramref name="services"/> instance for chaining.</returns>
	public static IServiceCollection AddMongoReadModelStore(
		this IServiceCollection services, MongoClientSettings settings, string databaseName)
	{
		ArgumentNullException.ThrowIfNull(settings);
		if (string.IsNullOrWhiteSpace(databaseName))
			throw new ArgumentException("Database name must not be null or whitespace.", nameof(databaseName));

		services.AddSingleton<IMongoClient>(new MongoClient(settings));
		services.AddSingleton(sp => sp.GetRequiredService<IMongoClient>().GetDatabase(databaseName));
		services.AddScoped(typeof(IReadModelRepository<,>), typeof(MongoReadModelRepository<,>));
		services.AddScoped(typeof(IReadModelStore<,>), typeof(MongoReadModelStore<,>));
		return services;
	}
}
