# Claims Handling service

## Purpose

Claims Handling is a .NET 9 HTTP service for managing vessel insurance covers and the claims associated with those covers. It supports cover and claim creation, retrieval, and deletion, plus premium estimates. The service is backed by SQL Server and exposes an OpenAPI/Swagger interface.

## Solution structure

The solution follows onion architecture: dependencies point inward toward the domain, while the Web API is the outermost delivery boundary.

| Project | Responsibility |
| --- | --- |
| `Claims.Domain` | Claim and cover models, enums, domain validation exception, and premium calculation. Has no infrastructure dependency. |
| `Claims.Application` | MediatR commands and queries, handlers, FluentValidation validators and validation pipeline, plus repository, clock, and audit abstractions. |
| `Claims.Infrastructure` | SQL Server EF Core persistence, repository implementation, startup schema initialization, system clock, and the in-memory audit queue/background worker. |
| `Claims.WebApi` | HTTP controllers, request DTOs, dependency registration, Swagger/OpenAPI, CORS, HTTPS redirection, and global exception handling. |
| `Claims.UnitTests` | Domain and application unit tests. |
| `Claims.RepositoryTests` | Repository tests using isolated SQLite. |
| `Claims.IntegrationTests` | HTTP pipeline/API integration tests using an isolated test database. |
| `Claims.Client` | Standalone React and TypeScript API playground. |

## Request flow

1. An HTTP controller binds and validates request shape, then sends one MediatR command or query.
2. `ValidationBehavior<TRequest,TResponse>` invokes all registered FluentValidation validators before the handler runs.
3. Application handlers coordinate domain rules and `IClaimsRepository` operations.
4. Infrastructure persists data through EF Core and SQL Server.
5. Create and delete handlers enqueue audit messages. `AuditQueue`, a hosted background service, persists those messages separately from request handling.
6. `GlobalExceptionHandler` maps validation and domain errors to Problem Details responses and logs unexpected exceptions.

## Domain rules

### Claims

- Name is required and cannot exceed 200 characters.
- Damage cost must be from 0 through 100,000 inclusive.
- The referenced cover must exist.
- Claim `Created` must be within the cover dates, inclusive of both start and end dates.

### Covers

- Start date cannot be before the server's current local date.
- End date must be after start date.
- The period cannot exceed one year.
- A cover with claims cannot be deleted.

### Premium

Premium uses an elapsed-day count (`EndDate - StartDate`) and a base daily rate of 1,250. Type multipliers are Yacht 1.10, Passenger ship 1.20, Tanker 1.50, and all other cover types 1.30. Days 1–30 have no duration discount; days 31–180 receive a 5% Yacht or 2% other-type discount; days after 180 receive an 8% Yacht or 3% other-type total discount. The latter is cumulative, so the additional discount is added to the middle-band discount.

## Identifiers and persistence

Each claim and cover has a GUID `Id` as its stable unique key and a database-generated integer `DisplayId` for URLs and human-facing references. Claim records refer to covers by GUID `CoverId`; create-claim clients must therefore send the cover's GUID `id`, not its integer `displayId`. Audit records store the entity name, GUID entity ID, operation, and event timestamp.

The default connection string targets `localhost`, database `Claims`, using Windows integrated authentication. It can be overridden through the `ConnectionStrings:Claims` configuration key or the `ConnectionStrings__Claims` environment variable. On SQL Server startup, EF Core applies the initial Claims/Covers and audit migrations. The Claims/Covers baseline creates missing tables and adds integer display IDs to the previous `EnsureCreated` schema if needed; the audit baseline copies legacy `AuditEntries` into the dedicated audit tables before removing the old table. SQLite test providers use independent in-memory schemas. Use migrations and a controlled deployment process for production schema changes.

## Audit delivery and reliability

Every successful MediatR action queues an audit event using nonblocking `TryWrite` to a bounded in-memory `Channel<T>` (capacity 2,000); a single hosted background worker inserts it into `ClaimAudits` or `CoverAudits`. The HTTP request does not await the audit insert. The queue is volatile: queued events can be lost on process termination, is not shared across service instances, and persistence failures are logged without automatic retry. If the queue is full or stopping, admission fails immediately and the worker logs the dropped event. Collection reads use `*` as the entity ID; premium calculations use `premium-calculation`. For production durability, use a transactional outbox and a durable broker such as Azure Service Bus, with idempotent consumers and retry/dead-letter handling.

## Run locally

Prerequisites: .NET 9 SDK and a reachable local SQL Server with Windows authentication enabled.

```powershell
dotnet run --project .\Claims.WebApi\Claims.WebApi.csproj
```

The development launch profile listens at `http://localhost:5180` and opens Swagger at `/swagger`.

Run the tests from the solution root:

```powershell
dotnet test .\Claims.slnx
```

The test projects use isolated SQLite databases and do not require the local Claims SQL database.

## Related documentation

- [HTTP API reference](api-reference.md)
- [Postman collection](../postman/ClaimsApi.postman_collection.json)
- [Repository README](../README.md)
