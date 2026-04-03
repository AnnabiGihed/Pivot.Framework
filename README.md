# Pivot.Framework

[![Tests](https://github.com/AnnabiGihed/Pivot.Framework/actions/workflows/test.yml/badge.svg)](https://github.com/AnnabiGihed/Pivot.Framework/actions/workflows/test.yml)

A collection of production-ready .NET 10 NuGet packages that provide plug-and-play infrastructure for Clean Architecture applications. Covers domain primitives, application plumbing, persistence, caching, messaging, scheduling, and Keycloak authentication for ASP.NET Core, Blazor Server, and MAUI.

Published to GitHub Packages: `https://nuget.pkg.github.com/AnnabiGihed/index.json`

---

## Table of Contents

- [Packages](#packages)
- [Installation](#installation)
- [Core — Domain](#core--domain)
- [Core — Application](#core--application)
- [Tools — DependencyInjection](#tools--dependencyinjection)
- [Infrastructure — Abstraction](#infrastructure--abstraction)
- [Infrastructure — Persistence (EF Core)](#infrastructure--persistence-ef-core)
- [Infrastructure — Messaging (EF Core)](#infrastructure--messaging-ef-core)
- [Infrastructure — Caching](#infrastructure--caching)
- [Infrastructure — Read Store (MongoDB)](#infrastructure--read-store-mongodb)
- [Infrastructure — Scheduling](#infrastructure--scheduling)
- [Containers — API](#containers--api)
- [Authentication — Core](#authentication--core)
- [Authentication — ASP.NET Core](#authentication--aspnet-core)
- [Authentication — Blazor Server](#authentication--blazor-server)
- [Authentication — MAUI](#authentication--maui)
- [Authentication — Caching (Redis)](#authentication--caching-redis)
- [Authentication — Hangfire Dashboard](#authentication--hangfire-dashboard)
- [Versioning and Publishing](#versioning-and-publishing)

---

## Packages

| Package | Description |
|---|---|
| `Pivot.Framework.Domain` | DDD primitives — aggregates, domain events, strongly-typed IDs, auditing, soft delete, `Result<T>`, `DomainError` |
| `Pivot.Framework.Application` | CQRS with MediatR, FluentValidation pipeline, read models, `ICurrentUser` |
| `Pivot.Framework.Infrastructure.Abstraction` | Outbox contracts, message broker interfaces, repository contracts, scheduling interfaces |
| `Pivot.Framework.Infrastructure.Persistence.EntityFrameworkCore` | EF Core unit of work, outbox repository, audit interceptors, transaction manager |
| `Pivot.Framework.Infrastructure.Messaging.EntityFrameworkCore` | Outbox processor — dispatches persisted domain events; RabbitMQ receiver; projection dispatcher |
| `Pivot.Framework.Infrastructure.Caching` | Generic `ICacheService` backed by Redis |
| `Pivot.Framework.Infrastructure.ReadStore.MongoDB` | MongoDB read model repository and store |
| `Pivot.Framework.Infrastructure.Scheduling` | Hangfire integration for recurring background jobs |
| `Pivot.Framework.Tools.DependencyInjection` | `IServiceInstaller` convention — auto-discover and install DI modules by assembly scan |
| `Pivot.Framework.Containers.API` | ASP.NET Core API base — `ApiController`, exception middleware, transaction middleware, outbox middleware |
| `Pivot.Framework.Authentication` | Core Keycloak models — `KeycloakOptions`, `ICurrentUser`, `IKeycloakAuthService` |
| `Pivot.Framework.Authentication.AspNetCore` | JWT bearer setup for ASP.NET Core APIs (`AddKeycloakBackend`) + Swagger OAuth2 |
| `Pivot.Framework.Authentication.Blazor` | PKCE login flow + Redis session store for Blazor Server (`AddKeycloakBlazor`) |
| `Pivot.Framework.Authentication.Maui` | PKCE login flow via `WebAuthenticator` for .NET MAUI (`AddKeycloakMaui`) |
| `Pivot.Framework.Authentication.Caching` | Redis-backed JWT token cache and revocation blacklist for ASP.NET Core |
| `Pivot.Framework.Authentication.Hangfire` | Keycloak browser-based auth for the Hangfire dashboard |

---

## Installation

Add the GitHub Packages feed to your `NuGet.config`:

```xml
<configuration>
  <packageSources>
    <add key="github" value="https://nuget.pkg.github.com/AnnabiGihed/index.json" />
  </packageSources>
  <packageSourceCredentials>
    <github>
      <add key="Username" value="YOUR_GITHUB_USERNAME" />
      <add key="ClearTextPassword" value="YOUR_GITHUB_PAT" />
    </github>
  </packageSourceCredentials>
</configuration>
```

Then install the packages you need:

```bash
dotnet add package Pivot.Framework.Domain
dotnet add package Pivot.Framework.Application
dotnet add package Pivot.Framework.Infrastructure.Persistence.EntityFrameworkCore
dotnet add package Pivot.Framework.Authentication.Blazor
# etc.
```

---

## Core — Domain

**Package:** `Pivot.Framework.Domain`

Provides all DDD building blocks your domain layer builds on. Has zero infrastructure dependencies — only `CSharpFunctionalExtensions` for the `Result` type.

### Entity hierarchy

Choose the base class that matches your aggregate's needs:

| Base class | Identity | Auditing | Soft delete | Domain events |
|---|---|---|---|---|
| `Entity<TId>` | Yes | No | No | No |
| `AuditableEntity<TId>` | Yes | Yes | No | No |
| `FullEntity<TId>` | Yes | Yes | Yes | No |
| `LightweightAggregateRoot<TId>` | Yes | No | No | Yes |
| `AuditableAggregateRoot<TId>` | Yes | Yes | No | Yes |
| `AggregateRoot<TId>` | Yes | Yes | Yes | Yes |

All entities use **identity-based equality** — two instances are equal if and only if they share the same type and the same `Id`.

### Strongly-typed IDs

Every entity identifier must implement `IStronglyTypedId<TSelf>`. Use `StronglyTypedGuidId<TSelf>` for GUID-backed IDs:

```csharp
// Define an ID type for each aggregate
public sealed record OrderId(Guid Value) : StronglyTypedGuidId<OrderId>(Value);
public sealed record CustomerId(Guid Value) : StronglyTypedGuidId<CustomerId>(Value);

// IDs of different types are never interchangeable at compile time
Order order = repo.GetById(new OrderId(Guid.NewGuid())); // OK
Order order = repo.GetById(new CustomerId(Guid.NewGuid())); // compile error
```

`StronglyTypedGuidId<TSelf>` guards against `Guid.Empty` and implements `IComparable<TSelf>`.

### Aggregate roots

```csharp
public class Order : AggregateRoot<OrderId>
{
    public OrderStatus Status { get; private set; }

    // Factory method — preferred over public constructors
    public static Order Create(CustomerId customerId)
    {
        var order = new Order(new OrderId(Guid.NewGuid()));
        order.RaiseDomainEvent(new OrderCreatedEvent(order.Id, customerId));
        return order;
    }

    public void Ship()
    {
        if (Status != OrderStatus.Paid)
            throw new DomainException("Order must be paid before shipping.");

        Status = OrderStatus.Shipped;
        RaiseDomainEvent(new OrderShippedEvent(Id));
    }

    // Soft delete (available on AggregateRoot<TId> and FullEntity<TId>)
    public void Cancel(string actor) => Delete(DateTime.UtcNow, actor);
}
```

- `RaiseDomainEvent(IDomainEvent)` — queues an event; dispatched by infrastructure after `SaveChangesAsync`.
- `GetDomainEvents()` — returns the pending event list (called by infrastructure).
- `ClearDomainEvents()` — explicit-interface method; only callable by infrastructure code casting to `IAggregateRoot`.

### Domain events

Implement `IDomainEvent` (which is a marker interface). Use `DomainEvent` as a convenient base:

```csharp
public sealed record OrderCreatedEvent(OrderId OrderId, CustomerId CustomerId) : DomainEvent;
public sealed record OrderShippedEvent(OrderId OrderId) : DomainEvent;
```

### Result type

All application-layer operations return `Result` or `Result<T>` (from `CSharpFunctionalExtensions`). Never throw exceptions for expected business failures.

```csharp
// Success
Result<OrderId> result = Result.Success(new OrderId(Guid.NewGuid()));

// Failure
Result<OrderId> result = Result.Failure<OrderId>("Order.Customer.NotFound");

// Consuming
if (result.IsFailure)
    return BadRequest(result.Error);

return Ok(result.Value);
```

### DomainError

A value object that carries a stable machine-readable `Code` and a human-readable `Message`. Equality is code-based so the same error compares equal regardless of localization.

```csharp
public static class OrderErrors
{
    public static readonly DomainError NotFound =
        new("Order.NotFound", "The requested order was not found.");

    public static readonly DomainError AlreadyShipped =
        new("Order.AlreadyShipped", "The order has already been shipped.");
}

// Serialize / deserialize (for outbox transport, logging, etc.)
string serialized = OrderErrors.NotFound.Serialize();   // "Order.NotFound||The requested order..."
Error error = DomainError.Deserialize(serialized);
```

Use `DomainError.ForField(name)` and `DomainError.ValueForField(name, value)` to build field-specific error labels.

### BaseDomainErrors

Pre-built, localizable error factories for common scenarios:

```csharp
BaseDomainErrors.General.ValueIsRequired("Email");
BaseDomainErrors.General.ValueIsInvalid("PhoneNumber", "+invalid");
BaseDomainErrors.General.ValueOutOfRange("Quantity", "1", "100");
```

### Domain exceptions

Throw these for truly exceptional conditions (not for validation):

```csharp
throw new DomainException("Something unexpected happened.");
throw new AggregateDomainException(orderId, "Concurrent modification detected.");
throw new AlreadyExistsDomainException("email", email);
throw new NotExistsDomainException("orderId", id.ToString());
throw new RequiredDomainException("CustomerId");
throw new OutOfRangeDomainException("Quantity", "1", "100");
```

These are mapped to RFC 7807 `ProblemDetails` responses by `ExceptionHandlerMiddleware`.

### Auditing interfaces

- `IAuditableEntity` — exposes `CreatedOn`, `CreatedBy`, `ModifiedOn`, `ModifiedBy`.
- `ISoftDeletableEntity` — exposes `IsDeleted`, `DeletedOn`, `DeletedBy`.

EF Core automatically populates audit fields on `SaveChangesAsync` when your `DbContext` inherits from the provided base.

---

## Core — Application

**Package:** `Pivot.Framework.Application`

Wires up the application layer with MediatR, FluentValidation, and AutoMapper. Defines the CQRS contracts used across the entire solution.

### Commands

Commands represent intent to change state and always return a `Result` or `Result<T>`:

```csharp
// No return value
public sealed record DeleteOrderCommand(OrderId OrderId) : ICommand;

// Returns a value
public sealed record CreateOrderCommand(CustomerId CustomerId, IReadOnlyList<OrderLineDto> Lines)
    : ICommand<OrderId>;
```

Implement `ICommandHandler<TCommand>` or `ICommandHandler<TCommand, TResponse>`:

```csharp
internal sealed class CreateOrderCommandHandler
    : ICommandHandler<CreateOrderCommand, OrderId>
{
    private readonly IUnitOfWork<AppDbContext> _unitOfWork;
    private readonly ICurrentUser _currentUser;

    public CreateOrderCommandHandler(IUnitOfWork<AppDbContext> unitOfWork, ICurrentUser currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Result<OrderId>> Handle(CreateOrderCommand command, CancellationToken ct)
    {
        var order = Order.Create(command.CustomerId);
        await _unitOfWork.Repository<Order, OrderId>().AddAsync(order, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Success(order.Id);
    }
}
```

### Queries

Queries are read-only and return `Result<T>`:

```csharp
public sealed record GetOrderQuery(OrderId OrderId) : IQuery<OrderDto>;

internal sealed class GetOrderQueryHandler : IQueryHandler<GetOrderQuery, OrderDto>
{
    public async Task<Result<OrderDto>> Handle(GetOrderQuery query, CancellationToken ct)
    {
        // query the read model or repository
    }
}
```

### Domain event handlers

Implement `IDomainEventHandler<TEvent>` to react to domain events published through MediatR:

```csharp
internal sealed class OrderCreatedEventHandler : IDomainEventHandler<OrderCreatedEvent>
{
    public async Task<Result> HandleWithResultAsync(OrderCreatedEvent domainEvent, CancellationToken ct)
    {
        // e.g. send a confirmation email, update a read model
        return Result.Success();
    }
}
```

> **Note:** When MediatR invokes the handler via its notification path, the `Result` returned by `HandleWithResultAsync` is intentionally discarded. If you need to collect results, call `HandleWithResultAsync` directly or implement a custom `IDomainEventDispatcher`.

### Validation pipeline

Register a `ValidationPipelineBehavior` as a MediatR pipeline behavior. It automatically runs all `IValidator<TRequest>` validators before the handler and short-circuits with a `ValidationException` on failure.

```csharp
// In your DI setup
services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(AssemblyReference).Assembly);
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationPipelineBehavior<,>));
});

// Validators are discovered automatically from the assembly
public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(c => c.CustomerId).NotEmpty();
        RuleFor(c => c.Lines).NotEmpty().WithMessage("At least one order line is required.");
    }
}
```

### Read models

Use read models (projections) for query-optimized views. The read model's `TId` is unconstrained — use `Guid`, `int`, `string`, etc.

```csharp
// Define a read model
public class OrderSummary : IReadModel<Guid>
{
    public Guid Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
}

// Query via IReadModelRepository<TReadModel, TId>
public class GetOrderSummaryQueryHandler : IQueryHandler<GetOrderSummaryQuery, OrderSummary>
{
    private readonly IReadModelRepository<OrderSummary, Guid> _repo;

    public async Task<Result<OrderSummary>> Handle(GetOrderSummaryQuery query, CancellationToken ct)
    {
        var summary = await _repo.GetByIdAsync(query.OrderId, ct);
        return summary is not null
            ? Result.Success(summary)
            : Result.Failure<OrderSummary>(OrderErrors.NotFound.Code);
    }
}
```

Project read models from domain events using `IProjectionDispatcher` and `ProjectionHandler<TEvent, TReadModel>`.

### ICurrentUser

Available in any handler, controller, or service via injection:

```csharp
public class MyService(ICurrentUser currentUser)
{
    public void DoSomething()
    {
        Guid? userId        = currentUser.UserId;
        string? name        = currentUser.DisplayName;
        string? email       = currentUser.Email;
        bool isAdmin        = currentUser.IsInRole("admin");
        bool isAuthenticated = currentUser.IsAuthenticated;
    }
}
```

Register with `AddKeycloakBackend` (ASP.NET Core) or `AddKeycloakBlazor` (Blazor Server).

### Application exceptions

Throw these from handlers for known error conditions — `ExceptionHandlerMiddleware` maps them automatically:

```csharp
throw new NotFoundException(nameof(Order), id);         // → HTTP 404
throw new BadRequestException("Invalid payload.");      // → HTTP 400
throw new ValidationException(validationFailures);      // → HTTP 422
```

---

## Tools — DependencyInjection

**Package:** `Pivot.Framework.Tools.DependencyInjection`

Convention-based DI registration. Implement `IServiceInstaller` in each project layer and let the framework discover and run them.

```csharp
// 1. Define an installer in each project
public class InfrastructureServiceInstaller : IServiceInstaller
{
    public void Install(
        IServiceCollection services,
        IConfiguration configuration,
        bool includeConventionBasedRegistration = true)
    {
        services.AddScoped<IOrderRepository, OrderRepository>();

        if (includeConventionBasedRegistration)
        {
            // e.g. Scrutor-based scanning
            services.Scan(scan => scan.FromAssemblyOf<InfrastructureServiceInstaller>()
                .AddClasses(c => c.AssignableTo<IRepository>())
                .AsImplementedInterfaces()
                .WithScopedLifetime());
        }
    }
}

// 2. In Program.cs — discovers all IServiceInstaller implementations in the provided assemblies
services.InstallServices(
    configuration,
    includeConventionBasedRegistration: true,
    typeof(InfrastructureServiceInstaller).Assembly,
    typeof(ApplicationServiceInstaller).Assembly);
```

`BaseServiceInstaller` is an abstract base you can extend for shared installer behavior.

---

## Infrastructure — Abstraction

**Package:** `Pivot.Framework.Infrastructure.Abstraction`

Pure interface library — no EF Core or Redis dependency. Reference this from your application layer to avoid coupling to infrastructure implementations.

### Outbox interfaces

| Interface | Purpose |
|---|---|
| `IOutboxRepository` | Persist and query `OutboxMessage` records |
| `IOutboxProcessor` | Process pending outbox messages |
| `IDomainEventPublisher` | Persist domain events to the outbox within the current transaction |

### Outbox drain modes

```csharp
public enum OutboxDrainMode
{
    ImmediateAfterRequest = 1,  // Process inline, after each HTTP request
    BackgroundPolling     = 2   // Process via a background service at a configurable interval
}
```

Configure drain mode when registering the outbox:

```csharp
// Inline — simple; adds latency to the HTTP response
services.AddOutboxDraining<AppDbContext>(options =>
{
    options.Mode = OutboxDrainMode.ImmediateAfterRequest;
});

// Background — recommended for production
services.AddOutboxDraining<AppDbContext>(options =>
{
    options.Mode = OutboxDrainMode.BackgroundPolling;
    options.PollingInterval = TimeSpan.FromSeconds(5);
});
```

> Only one drain mode can be active per application. Registering both throws `InvalidOperationException` at startup.

### Message broker interfaces

```csharp
IMessagePublisher   // Publish a message to the broker
IMessageReceiver    // Receive messages from the broker
IMessageSerializer  // Serialize / deserialize message payloads
IMessageCompressor  // Optional payload compression
IMessageEncryptor   // Optional payload encryption
```

**RabbitMQ settings** — bind from `appsettings.json`:

```json
{
  "RabbitMQ": {
    "Host": "localhost",
    "Port": 5672,
    "Username": "guest",
    "Password": "guest",
    "VirtualHost": "/",
    "Exchange": "pivot.exchange"
  }
}
```

`Exchange` and `RoutingKey` act as fallback publish defaults. For service-specific per-message routing, register an `IOutboxRoutingResolver` and declare the matching topology with `AddRabbitMQTopology(...)`.

### Scheduling interfaces

```csharp
public interface IRecurringJobService
{
    void AddOrUpdate(string jobId, Expression<Action> methodCall, RecurrenceConfiguration config);
    void RemoveIfExists(string jobId);
}

// Configure recurrence
var config = new RecurrenceConfiguration
{
    Type     = RecurrenceType.Daily,
    TimeOfDay = TimeOnly.Parse("02:00"),
    TimeZone  = TimeZoneInfo.Utc
};
```

---

## Infrastructure — Persistence (EF Core)

**Package:** `Pivot.Framework.Infrastructure.Persistence.EntityFrameworkCore`

### DbContext base

Inherit your `DbContext` from `TemplatesCoreDbContextBase<TContext>` to get automatic audit stamping and domain event → outbox dispatch on `SaveChangesAsync`:

```csharp
public class AppDbContext : TemplatesCoreDbContextBase<AppDbContext>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); // required — applies audit config and soft-delete filters
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
```

### Unit of work

`IUnitOfWork<TContext>` provides a single `SaveChangesAsync` entry point and exposes per-aggregate repositories:

```csharp
public class CreateOrderCommandHandler(IUnitOfWork<AppDbContext> unitOfWork)
{
    public async Task<Result<OrderId>> Handle(CreateOrderCommand cmd, CancellationToken ct)
    {
        var repo = unitOfWork.Repository<Order, OrderId>();

        var order = Order.Create(cmd.CustomerId);
        await repo.AddAsync(order, ct);
        await unitOfWork.SaveChangesAsync(ct); // stamps audit fields + writes outbox
        return Result.Success(order.Id);
    }
}
```

Register in DI:

```csharp
public sealed class AppUnitOfWork
    : UnitOfWork<AppDbContext>
{
    public AppUnitOfWork(
        AppDbContext dbContext,
        ICurrentUserProvider currentUserProvider,
        IDomainEventPublisher<AppDbContext> domainEventPublisher)
        : base(dbContext, currentUserProvider, domainEventPublisher)
    {
    }
}

services.AddPostgreSqlContext<AppDbContext>(configuration.GetConnectionString("Default")!);
services.AddEfCoreWritePersistence<AppDbContext, AppUnitOfWork>(includeEventStore: true);
```

### Repository

`BaseAsyncCommandRepository<TEntity, TId, TContext>` implements `IAsyncCommandRepository<TEntity, TId>`:

```csharp
// Available via IUnitOfWork — no need to register separately
var repo = unitOfWork.Repository<Order, OrderId>();

await repo.AddAsync(order, ct);
await repo.UpdateAsync(order, ct);
await repo.DeleteAsync(order, ct);
Order? found = await repo.GetByIdAsync(orderId, ct);
```

### Specifications

Use the specification pattern to build reusable, composable queries:

```csharp
public class PendingOrdersSpecification : EntitySpecification<Order>
{
    public PendingOrdersSpecification() : base(o => o.Status == OrderStatus.Pending)
    {
        AddInclude(o => o.Lines);
        ApplyOrderBy(o => o.CreatedOn);
        ApplyPaging(skip: 0, take: 20);
    }
}

// Apply via the evaluator
IQueryable<Order> query = EntitySpecificationEvaluator<Order>.GetQuery(dbContext.Set<Order>(), spec);
```

### Transaction manager

`ITransactionManager<TContext>` gives you explicit transaction control:

```csharp
await transactionManager.BeginTransactionAsync();
try
{
    // ... do work
    await transactionManager.CommitTransactionAsync();
}
catch
{
    await transactionManager.RollbackTransactionAsync();
    throw;
}
```

`TransactionMiddleware<TContext>` (in `Pivot.Framework.Containers.API`) uses this automatically for all non-GET requests.

### Write-side persistence bundle

For a production-ready EF Core write-side setup, register the transport-agnostic persistence stack first, then choose your transport and outbox drain mode separately:

```csharp
services.AddPostgreSqlContext<AppDbContext>(configuration.GetConnectionString("Default")!);
services.AddEfCoreWritePersistence<AppDbContext, AppUnitOfWork>(includeEventStore: true);

// Choose one transport
services.AddRabbitMQPublisher(configuration);
// or
services.AddInProcessMessagePublisher();

// Optional: per-message RabbitMQ routing
services.AddOutboxRoutingResolver<MyOutboxRoutingResolver>();

// Choose one drain mode
services.AddOutboxDraining<AppDbContext>(options =>
{
    options.Mode = OutboxDrainMode.BackgroundPolling;
    options.PollingInterval = TimeSpan.FromSeconds(5);
});
```

`AddEfCoreWritePersistence<TContext, TUnitOfWork>()` registers:

- `ITransactionManager<TContext>`
- `IOutboxRepository<TContext>`
- `IDomainEventPublisher<TContext>`
- `IDomainEventPublisher` (for backwards compatibility in single-context applications)
- `ICurrentUserProvider`
- `IHttpContextAccessor`
- `IUnitOfWork<TContext>`

When multiple write contexts coexist in the same process, prefer injecting `IDomainEventPublisher<TContext>` directly rather than the non-generic `IDomainEventPublisher`.

To translate domain events into integration events in the same transaction, opt in separately:

```csharp
services.AddIntegrationEventMapping<AppDbContext>();
```

This keeps existing write-side behaviour unchanged unless the service explicitly enables mapping.

### EF Core model configuration helpers

`DomainPrimitivesModelBuilderExtensions` configures strongly-typed IDs and audit navigation names for EF Core:

```csharp
protected override void OnModelCreating(ModelBuilder builder)
{
    base.OnModelCreating(builder);
    builder.ConfigureDomainPrimitives(); // maps StronglyTypedGuidId value converters
}
```

### Read model repositories (EF Core)

`EfCoreReadModelRepository<TReadModel, TId>` and `EfCoreReadModelStore<TReadModel, TId>` implement `IReadModelRepository` and `IReadModelStore` backed by EF Core:

```csharp
services.AddScoped<IReadModelRepository<OrderSummary, Guid>,
                   EfCoreReadModelRepository<OrderSummary, Guid, AppDbContext>>();
```

Or use the extension:

```csharp
services.AddEfCoreReadModel<OrderSummary, Guid, AppDbContext>();
```

---

## Infrastructure — Messaging (EF Core)

**Package:** `Pivot.Framework.Infrastructure.Messaging.EntityFrameworkCore`

### Outbox processing

The outbox pattern ensures domain events are reliably published even if the message broker is temporarily unavailable. Events are first persisted to the database inside the same transaction as the aggregate, then published asynchronously.

```csharp
// Register outbox draining in ImmediateAfterRequest mode (inline, after each HTTP request)
services.AddOutboxDraining<AppDbContext>(options =>
{
    options.Mode = OutboxDrainMode.ImmediateAfterRequest;
});

// Or background polling mode
services.AddOutboxDraining<AppDbContext>(opts =>
{
    opts.Mode = OutboxDrainMode.BackgroundPolling;
    opts.PollingInterval = TimeSpan.FromSeconds(10);
});

// Add the middleware if using ImmediateAfterRequest
app.UseImmediateOutboxDraining<AppDbContext>();
```

> Calling `AddOutboxDraining` twice with different modes (or calling `UseImmediateOutboxDraining` when background mode is registered) throws `InvalidOperationException` to prevent accidental dual-processing.

### Per-message routing

When a service needs different routes for different outbox messages, implement `IOutboxRoutingResolver` and register it with:

```csharp
services.AddOutboxRoutingResolver<MyOutboxRoutingResolver>();
```

The resolver is used only at publish time. When routes differ from the fallback `RabbitMQSettings`, declare the corresponding exchanges, queues, and bindings explicitly with `AddRabbitMQTopology(...)`.

### Domain-to-integration mapping

To enqueue integration events derived from domain events in the same transaction as the aggregate changes, register mappers implementing `IIntegrationEventMapper<TDomainEvent>` and opt in with:

```csharp
services.AddIntegrationEventMapping<AppDbContext>();
```

This feature is transport-agnostic at the outbox-write level, but delivery of the resulting integration-event messages requires a transport capable of handling integration events, such as RabbitMQ.

### Projection dispatcher

`ProjectionDispatcher` routes domain events to `ProjectionHandler<TEvent, TReadModel>` implementations, keeping read models in sync:

```csharp
public class OrderSummaryProjectionHandler
    : ProjectionHandler<OrderCreatedEvent, OrderSummary>
{
    protected override Task<OrderSummary> HandleAsync(OrderCreatedEvent evt, CancellationToken ct)
    {
        return Task.FromResult(new OrderSummary
        {
            Id     = evt.OrderId.Value,
            Status = "Pending"
        });
    }
}
```

### RabbitMQ receiver

`RabbitMQReceiver` implements `IMessageReceiver` and consumes messages from a RabbitMQ queue, deserializing them and forwarding to your handler pipeline.

---

## Infrastructure — Caching

**Package:** `Pivot.Framework.Infrastructure.Caching`

A strongly-typed distributed cache abstraction over `IDistributedCache` using JSON serialization. Callers never deal with raw byte arrays.

### ICacheService

```csharp
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken ct = default) where T : class;
    Task SetAsync<T>(string key, T value, TimeSpan absoluteExpiry, CancellationToken ct = default) where T : class;
    Task<bool> ExistsAsync(string key, CancellationToken ct = default);
    Task RemoveAsync(string key, CancellationToken ct = default);
}
```

### Registration

```csharp
// Registers IDistributedCache (StackExchange.Redis) + ICacheService in one call
builder.Services.AddRedisCache(builder.Configuration);
```

**appsettings.json:**

```json
{
  "ConnectionStrings": {
    "Redis": "localhost:6379"
  }
}
```

### Usage

```csharp
public class ProductCatalogService(ICacheService cache)
{
    private const string Key = "catalog:featured";

    public async Task<List<ProductDto>> GetFeaturedAsync(CancellationToken ct)
    {
        var cached = await cache.GetAsync<List<ProductDto>>(Key, ct);
        if (cached is not null)
            return cached;

        var products = await LoadFromDatabaseAsync(ct);
        await cache.SetAsync(Key, products, TimeSpan.FromMinutes(15), ct);
        return products;
    }
}
```

---

## Infrastructure — Read Store (MongoDB)

**Package:** `Pivot.Framework.Infrastructure.ReadStore.MongoDB`

MongoDB-backed `IReadModelRepository<TReadModel, TId>` and `IReadModelStore<TReadModel, TId>`.

### Registration

```csharp
// Register MongoDB read model infrastructure for a specific read model
services.AddMongoReadModel<OrderSummary, Guid>(configuration);
```

**appsettings.json:**

```json
{
  "MongoDB": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "pivot_readstore"
  }
}
```

### Usage

```csharp
public class GetOrderSummaryQueryHandler(IReadModelRepository<OrderSummary, Guid> repo)
    : IQueryHandler<GetOrderSummaryQuery, OrderSummary>
{
    public async Task<Result<OrderSummary>> Handle(GetOrderSummaryQuery query, CancellationToken ct)
    {
        var result = await repo.GetByIdAsync(query.OrderId, ct);
        return result is not null
            ? Result.Success(result)
            : Result.Failure<OrderSummary>(OrderErrors.NotFound.Code);
    }
}
```

`MongoReadModelSpecificationEvaluator` translates `ReadModelSpecification<TReadModel>` objects into MongoDB filter expressions.

---

## Infrastructure — Scheduling

**Package:** `Pivot.Framework.Infrastructure.Scheduling`

Hangfire integration for recurring background jobs with an `IRecurringJobService` abstraction.

### Registration

```csharp
services.AddScheduling(configuration);
```

**appsettings.json:**

```json
{
  "ConnectionStrings": {
    "Hangfire": "Server=.;Database=HangfireDb;Trusted_Connection=True;"
  }
}
```

### Defining a recurring job

```csharp
public class ReportGenerationJob : IRecurringJob
{
    public async Task RunAsync(CancellationToken ct)
    {
        // generate and send daily report
    }
}

// Register and schedule the job
public class SchedulingInstaller : IServiceInstaller
{
    public void Install(IServiceCollection services, IConfiguration configuration, bool includeConventions = true)
    {
        services.AddTransient<ReportGenerationJob>();
    }
}

// In the host startup
IRecurringJobService jobs = app.Services.GetRequiredService<IRecurringJobService>();
jobs.AddOrUpdate("daily-report", () => ..., new RecurrenceConfiguration
{
    Type     = RecurrenceType.Daily,
    TimeOfDay = TimeOnly.Parse("06:00"),
    TimeZone  = TimeZoneInfo.Utc
});
```

### Hangfire dashboard (unauthenticated)

```csharp
app.UseHangfireDashboard(); // /hangfire — no auth
```

For Keycloak-protected dashboard, see [Authentication — Hangfire Dashboard](#authentication--hangfire-dashboard).

---

## Containers — API

**Package:** `Pivot.Framework.Containers.API`

### ApiController base

Provides `HandleResult` overloads that convert `Result<T>` and `Result` into the correct HTTP responses:

```csharp
[ApiController]
[Route("api/orders")]
public sealed class OrdersController : ApiController
{
    public OrdersController(ISender sender) : base(sender) { }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderCommand command, CancellationToken ct)
        => HandleResult(await Sender.Send(command, ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
        => HandleResult(await Sender.Send(new GetOrderQuery(new OrderId(id)), ct));
}
```

`HandleResult` maps:
- `Result.Success(value)` → `200 OK`
- `Result.Failure` with `NotFoundException` → `404 Not Found`
- `Result.Failure` with `ValidationException` → `422 Unprocessable Entity`
- `Result.Failure` with `BadRequestException` → `400 Bad Request`
- Unknown failure → `500 Internal Server Error`

### Exception handler middleware

Catches all unhandled exceptions and returns RFC 7807 `ProblemDetails` JSON. Register before all other middleware:

```csharp
app.UseExceptionHandling(); // extension from Pivot.Framework.Containers.API
```

| Exception type | HTTP status |
|---|---|
| `NotFoundException` | 404 |
| `BadRequestException` | 400 |
| `ValidationException` | 422 |
| `DomainException` | 422 |
| Any other | 500 |

### Transaction middleware

Wraps every non-GET request in a database transaction. Commits on `2xx` and `422`; rolls back on everything else:

```csharp
app.UseMiddleware<TransactionMiddleware<AppDbContext>>();
```

Or via the typed extension:

```csharp
app.UseTransactions<AppDbContext>();
```

### Outbox processing middleware

When using `OutboxDrainMode.ImmediateAfterRequest`, register this middleware to drain the outbox after each successful request:

```csharp
app.UseImmediateOutboxDraining<AppDbContext>();
```

---

## Authentication — Core

**Package:** `Pivot.Framework.Authentication`

Shared models and contracts used by all authentication packages.

### KeycloakOptions

```json
{
  "Keycloak": {
    "BaseUrl": "https://auth.example.com",
    "Realm": "my-realm",
    "ClientId": "my-app",
    "ClientSecret": "optional-for-confidential-clients",
    "Audience": "my-app",
    "Scopes": "openid profile email offline_access",
    "RequireHttpsMetadata": true
  }
}
```

The `SectionName` constant is `"Keycloak"`.

### ICurrentUser

```csharp
public interface ICurrentUser
{
    Guid? UserId { get; }
    string? DisplayName { get; }
    string? Email { get; }
    bool IsAuthenticated { get; }
    bool IsInRole(string role);
    IReadOnlyCollection<string> Roles { get; }
}
```

### IKeycloakAuthService

Common auth service contract (login, logout, refresh, callback):

```csharp
public interface IKeycloakAuthService
{
    Task LoginAsync(CancellationToken ct = default);
    Task LogoutAsync(CancellationToken ct = default);
    Task<string?> HandleCallbackAsync(string code, string state, CancellationToken ct = default);
    Task<bool> RefreshAsync(CancellationToken ct = default);
    bool IsAuthenticated { get; }
    ICurrentUser? CurrentUser { get; }
}
```

### KeycloakAuthorizationMessageHandler

An `HttpMessageHandler` that attaches the current Bearer token to outgoing HTTP requests:

```csharp
builder.Services.AddHttpClient("MyApi", c => c.BaseAddress = new Uri("https://api.example.com"))
    .AddKeycloakHandler(); // available on both Blazor and MAUI extension packages
```

---

## Authentication — ASP.NET Core

**Package:** `Pivot.Framework.Authentication.AspNetCore`

JWT bearer authentication for ASP.NET Core APIs backed by Keycloak.

### Registration

```csharp
// Registers JWT bearer, ICurrentUser, IHttpContextAccessor, and claims transformation
builder.Services.AddKeycloakBackend(builder.Configuration);
```

### Swagger OAuth2 support

```csharp
// Adds Keycloak OAuth2 authorization to the Swagger UI
builder.Services.AddSwaggerGen(options =>
{
    options.AddKeycloakSecurityDefinition(builder.Configuration);
});

// Or via the single convenience extension
builder.Services.AddSwaggerWithKeycloakAuth(builder.Configuration);
```

The Swagger UI will show an "Authorize" button pre-configured with your Keycloak realm's authorization endpoint.

### Claims transformation

`KeycloakClaimsTransformer` automatically maps Keycloak-specific claims (realm roles, resource roles, `preferred_username`, `email`) to standard ASP.NET Core claim types on every authenticated request.

### KeycloakAuthenticationOptions

Fine-tune the JWT bearer validation:

```csharp
builder.Services.AddKeycloakBackend(builder.Configuration, options =>
{
    options.ValidateAudience  = true;
    options.ValidateLifetime  = true;
    options.ClockSkew         = TimeSpan.FromMinutes(1);
});
```

---

## Authentication — Blazor Server

**Package:** `Pivot.Framework.Authentication.Blazor`

Full PKCE login flow for Blazor Server. Tokens are stored **server-side in Redis** — the browser only receives an opaque `HttpOnly` session cookie. Compatible with `<AuthorizeView>` and `[Authorize]` out of the box.

### Prerequisites

- Redis registered as `IDistributedCache` (`AddStackExchangeRedisCache`)
- `AddAuthentication()` called before `AddKeycloakBlazor`

### Registration

```csharp
builder.Services.AddAuthentication();
builder.Services.AddStackExchangeRedisCache(o =>
    o.Configuration = builder.Configuration.GetConnectionString("Redis"));

builder.Services.AddKeycloakBlazor(builder.Configuration);
```

### Routes.razor

Wrap `RouteView` inside `CascadingAuthenticationState`:

```razor
<Router AppAssembly="typeof(Program).Assembly">
    <Found Context="routeData">
        <CascadingAuthenticationState>
            <RouteView RouteData="routeData" DefaultLayout="typeof(MainLayout)" />
            <FocusOnNavigate RouteData="routeData" Selector="h1" />
        </CascadingAuthenticationState>
    </Found>
</Router>
```

### MainLayout — rehydrate the session

Call `InitialiseFromCookieAsync()` in your layout so the auth state is restored on every circuit start:

```csharp
@inject IBlazorKeycloakAuthService Auth

protected override async Task OnInitializedAsync()
{
    await Auth.InitialiseFromCookieAsync();
}
```

### Required pages

Create these three pages:

**`/auth/callback`** — completes the PKCE exchange:

```razor
@page "/auth/callback"
@inject IBlazorKeycloakAuthService Auth
@inject NavigationManager Nav

@code {
    [SupplyParameterFromQuery(Name = "code")]  private string? Code  { get; set; }
    [SupplyParameterFromQuery(Name = "state")] private string? State { get; set; }

    protected override async Task OnInitializedAsync()
    {
        if (string.IsNullOrEmpty(Code) || string.IsNullOrEmpty(State))
        {
            Nav.NavigateTo("/", forceLoad: true);
            return;
        }
        var returnUrl = await Auth.HandleCallbackAsync(Code, State);
        Nav.NavigateTo(returnUrl ?? "/auth/login-failed", forceLoad: true);
    }
}
```

**`/auth/logout`** — revokes tokens and redirects to Keycloak end-session:

```razor
@page "/auth/logout"
@inject IBlazorKeycloakAuthService Auth

@code {
    protected override async Task OnInitializedAsync() => await Auth.LogoutAsync();
}
```

**`/auth/login-failed`** — shown when the callback fails (state mismatch, expired session, etc.).

### Login / logout button

```razor
@inject IBlazorKeycloakAuthService Auth
@inject NavigationManager Nav

<AuthorizeView>
    <Authorized>
        <span>Hello, @context.User.Identity?.Name!</span>
        <button @onclick="() => Nav.NavigateTo('/auth/logout', forceLoad: true)">Logout</button>
    </Authorized>
    <NotAuthorized>
        <button @onclick="Login">Login</button>
    </NotAuthorized>
</AuthorizeView>

@code {
    async Task Login() => await Auth.LoginAsync();
}
```

### Keycloak configuration

Add your callback URL to Keycloak's **Valid Redirect URIs**: `https://your-app/auth/callback`

### Authenticated API calls

```csharp
builder.Services
    .AddHttpClient("MyApi", c => c.BaseAddress = new Uri("https://api.example.com"))
    .AddKeycloakHandler();
```

### IBlazorKeycloakAuthService

```csharp
public interface IBlazorKeycloakAuthService : IKeycloakAuthService
{
    Task InitialiseFromCookieAsync(CancellationToken ct = default);
    Task LoginAsync(string? returnUrl = null, CancellationToken ct = default);
    Task LogoutAsync(CancellationToken ct = default);
    Task<string?> HandleCallbackAsync(string code, string state, CancellationToken ct = default);
    Task<bool> RefreshAsync(CancellationToken ct = default);
}
```

---

## Authentication — MAUI

**Package:** `Pivot.Framework.Authentication.Maui`

PKCE login flow for .NET MAUI Blazor Hybrid via the OS system browser (`WebAuthenticator`). Tokens are stored in OS secure storage (`SecureStorage`).

### Registration

```csharp
// MauiProgram.cs
builder.Services.AddKeycloakMaui(builder.Configuration);

// Attach Bearer tokens to outgoing API calls
builder.Services.AddHttpClient("MyApi", c => c.BaseAddress = new Uri("https://api.example.com"))
    .AddKeycloakHandler();
```

### Platform setup

The PKCE callback uses a custom URI scheme `{ClientId}://callback`. Register the scheme on each platform:

| Platform | Action |
|---|---|
| **Android** | Add an `IntentFilter` with `android:scheme="{ClientId}"` to `MainActivity` |
| **iOS / macOS** | Add the scheme to `CFBundleURLSchemes` in `Info.plist` and forward `OpenUrl` to `WebAuthenticator.Default.OpenUrl(url)` |
| **Windows** | Register the protocol in `Package.appxmanifest` under `Extensions > Protocol` |

Omitting platform registration causes the OAuth2 callback to fail silently — the browser will not return control to the app.

### Usage

```razor
@inject IKeycloakAuthService Auth

<AuthorizeView>
    <Authorized>
        <span>Hello, @context.User.Identity?.Name!</span>
        <button @onclick="Logout">Logout</button>
    </Authorized>
    <NotAuthorized>
        <button @onclick="Login">Login</button>
    </NotAuthorized>
</AuthorizeView>

@code {
    async Task Login()  => await Auth.LoginAsync();
    async Task Logout() => await Auth.LogoutAsync();
}
```

---

## Authentication — Caching (Redis)

**Package:** `Pivot.Framework.Authentication.Caching`

Redis-backed JWT token cache and revocation blacklist. Reduces repeated JWT parsing overhead and enables **immediate logout** before token expiry.

### Registration

```csharp
// Registers Redis + JWT bearer + claims cache + revocation cache in one call
builder.Services.AddKeycloakRedisCache(builder.Configuration);
```

### appsettings.json

```json
{
  "ConnectionStrings": {
    "Redis": "localhost:6379"
  },
  "Keycloak": {
    "BaseUrl": "https://auth.example.com",
    "Realm": "my-realm",
    "ClientId": "my-api"
  },
  "TokenRevocation": {
    "DefaultTtlDays": 30
  }
}
```

### Revoking a token on logout

```csharp
app.MapPost("/auth/logout", async (HttpContext ctx, ITokenRevocationCache revocation) =>
{
    var token = ctx.Request.Headers.Authorization
        .ToString().Replace("Bearer ", string.Empty, StringComparison.OrdinalIgnoreCase);

    if (!string.IsNullOrEmpty(token))
        await revocation.RevokeAsync(token);

    return Results.Ok();
}).RequireAuthorization();
```

### How it works

1. On the first request for a JWT, `KeycloakRedisJwtEvents.OnTokenValidated` parses claims once and caches `CachedTokenClaims` in Redis with a TTL matching the token's expiry.
2. On subsequent requests, claims are read from the cache — no re-parsing.
3. On logout (or token compromise), `ITokenRevocationCache.RevokeAsync(token)` adds the token JTI to a Redis revocation set.
4. `KeycloakRedisJwtEvents.OnTokenValidated` checks the revocation set on every request and rejects revoked tokens immediately.

### IDistributedTokenCache

```csharp
public interface IDistributedTokenCache
{
    Task<CachedTokenClaims?> GetAsync(string token, CancellationToken ct = default);
    Task SetAsync(string token, CachedTokenClaims claims, DateTimeOffset expiry, CancellationToken ct = default);
    Task RemoveAsync(string token, CancellationToken ct = default);
}
```

### ITokenRevocationCache

```csharp
public interface ITokenRevocationCache
{
    Task RevokeAsync(string token, CancellationToken ct = default);
    Task<bool> IsRevokedAsync(string token, CancellationToken ct = default);
}
```

---

## Authentication — Hangfire Dashboard

**Package:** `Pivot.Framework.Authentication.Hangfire`

Adds a Cookie + Keycloak OIDC browser authentication flow exclusively for the Hangfire dashboard, independent of your API's JWT Bearer scheme.

### Registration

```csharp
// 1. Service registration (DI)
builder.Services.AddHangfireKeycloakBrowserAuth(builder.Configuration);

// 2. Pipeline — mounts /hangfire, /hangfire-login, /hangfire-logout
app.UseHangfireDashboardWithKeycloakAuth();

// Optional — customize dashboard options
app.UseHangfireDashboardWithKeycloakAuth(options =>
{
    options.DarkModeEnabled = true;
    options.AppPath = "/";
});
```

### How it works

- `GET /hangfire-login` → redirects to Keycloak via OIDC (`/realms/{realm}/protocol/openid-connect/auth`).
- After successful login, Keycloak redirects back to `/hangfire-callback`, a `HangfireCookie` session is set, and the user lands on `/hangfire`.
- `GET /hangfire-logout` → clears the local cookie and signs out of Keycloak SSO.
- `HangfireCookieDashboardAuthorizationFilter` gates all `/hangfire` requests — unauthenticated requests are redirected to `/hangfire-login`.

### Required Keycloak configuration

Add `https://your-app/hangfire-callback` to the Keycloak client's **Valid Redirect URIs**.

---

## Authentication - API

**Package:** `Pivot.Framework.Authentication.API`

Backend auth endpoint helpers for provider-neutral login, callback, refresh, logout, profile, and token introspection flows.

### Registration

```csharp
builder.Services.AddAuthenticationApi();
builder.Services.AddKeycloakIdentityProviderServices(builder.Configuration);
builder.Services.AddInMemoryAuthSessions(); // optional
```

### Endpoint mapping

```csharp
app.MapAuthenticationApi("/auth");
```

Mapped endpoints:
- `POST /auth/login`
- `POST /auth/callback`
- `POST /auth/refresh`
- `POST /auth/logout`
- `GET /auth/profile`
- `POST /auth/introspect`

These endpoints are backed by:
- `IIdentityProviderAuthService`
- `IIdentityProviderAdminService`
- `ITokenIntrospectionService`
- `ITokenRevocationService`
- `IAuthSessionStore`

---

## Containers - gRPC

**Package:** `Pivot.Framework.Containers.Grpc`

Provides framework-aligned gRPC registration, exception handling, transaction boundaries, and result-to-status translation.

### Registration

```csharp
builder.Services.AddEfCoreWritePersistence<AppDbContext, AppUnitOfWork>();
builder.Services.AddPivotGrpc();
builder.Services.AddPivotGrpcTransactions<AppDbContext>();
```

Map your service with the framework helper:

```csharp
app.MapPivotGrpcService<OrdersGrpcService>();
```

### Exception handling

`AddPivotGrpc()` registers a global interceptor that maps framework exceptions to gRPC status codes:

| Exception type | gRPC status |
|---|---|
| `ValidationException` | `InvalidArgument` |
| `BadRequestException` | `InvalidArgument` |
| `NotFoundException` | `NotFound` |
| Any other | `Internal` |

Validation errors are added to response trailers under `validation-errors`.

### Transaction interceptor

`AddPivotGrpcTransactions<TContext>()` adds a transaction interceptor backed by `ITransactionManager<TContext>`.

By default:
- unary calls are wrapped in a transaction
- successful `OK` responses commit
- validation-style `InvalidArgument` failures commit
- all other failures roll back

Streaming interception is disabled by default so long-lived streams do not hold open database transactions. You can opt in explicitly:

```csharp
builder.Services.AddPivotGrpcTransactions<AppDbContext>(options =>
{
    options.InterceptServerStreamingCalls = true;
});
```

### Result helpers

Use the provided mapper and extensions to convert framework `Result` failures into transport-safe `RpcException` instances:

```csharp
public override async Task<GetOrderReply> Get(GetOrderRequest request, ServerCallContext context)
{
    var result = await _sender.Send(new GetOrderQuery(request.Id), context.CancellationToken);
    return result.GetValueOrThrow(_grpcResultStatusMapper);
}
```

### Proto conventions

The package ships build-transitive defaults for protobuf items:
- `ProtoRoot="Protos"`
- `GrpcServices="Server"`
- `Access="Public"`

That keeps generated server stubs aligned across services while still allowing service-level overrides in each `.csproj`.

---

## Versioning and Publishing

Packages are published automatically to GitHub Packages when a tag matching `v*.*.*` is pushed:

```bash
git tag v1.2.3
git push origin v1.2.3
```

The CI workflow in `.github/workflows/publish.yml` updates the version in all `.csproj` files, builds, packs, and pushes to `https://nuget.pkg.github.com/AnnabiGihed/index.json`.

---

## Author

Gihed Annabi — [github.com/AnnabiGihed](https://github.com/AnnabiGihed)
