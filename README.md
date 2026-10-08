# Claims API (onion architecture)

For the microservice architecture, domain rules, configuration, and runtime guidance, see [Service overview](docs/service-overview.md). For request and response examples for every endpoint, see the [HTTP API reference](docs/api-reference.md). Swagger UI is also served at `/swagger` when the API is running.

This is a separate replacement implementation; the original `backend-coding-task` project remains unchanged. The solution uses .NET 9, ASP.NET Core, MediatR, FluentValidation, EF Core with SQL Server, and xUnit.

## Projects

- **Claims.Domain** — entities, invariants, and premium calculation.
- **Claims.Application** — CQRS queries/commands, handlers, FluentValidation rules and MediatR validation behavior, plus repository/audit abstractions.
- Each CQRS operation has its own feature folder under `Claims.Application/Queries` or `Claims.Application/Commands`; each folder contains separate message, handler, and validator files.
- **Claims.Infrastructure** — EF Core repository, SQL Server setup, and asynchronous in-memory audit queue.
- **Claims.WebApi** — HTTP controllers, dependency wiring, and global errors through ASP.NET Core exception handling middleware.
- **Claims.UnitTests**, **Claims.RepositoryTests**, **Claims.IntegrationTests** — domain/application, SQLite repository, and HTTP pipeline coverage.

## Run

From this directory run `dotnet run --project Claims.WebApi`. The API connects to the local `Claims` database using Windows integrated authentication and creates its tables automatically. The Development launch profiles open Swagger UI at `/swagger` automatically. Unit and integration tests continue to use isolated in-memory SQLite databases.

Endpoints: `GET/POST /Claims`, `GET/DELETE /Claims/{id}`, `GET/POST /Covers`, `GET/DELETE /Covers/{id}`, and `POST /Covers/compute?startDate=YYYY-MM-DD&endDate=YYYY-MM-DD&coverType=Yacht`.

Enums are serialized as strings. FluentValidation runs before each MediatR handler through an open pipeline behavior. Validation failures return HTTP 400 ProblemDetails through the global exception handler; missing records return 404. Cover dates are inclusive for claim validation; premium days are the elapsed whole days between start and end dates.

Claims and covers retain GUID `Id` values as stable unique identifiers and have database-generated integer `DisplayId` values for routes and UI tables. A claim references its cover by GUID `CoverId`; a cover with claims cannot be deleted. On startup, the API adds searchable integer display ID columns to an existing local database when they are missing.

## Premium bands

The first 30 days use the type-adjusted base daily rate. Days 31–180 apply a 5% Yacht discount or 2% discount for other vessels. Days after 180 use a cumulative total discount of 8% for Yacht or 3% for other types. That interprets “additional” as additive percentage points against the base rate.

## Audit processing

Every successful MediatR API action queues an audit message through an in-memory bounded `Channel<T>`; a hosted background worker persists it to `ClaimAudits` or `CoverAudits` after the handler returns. Queue admission uses nonblocking `TryWrite`, so the HTTP request never waits for an audit SQL insert. The queue is volatile and drops/logs events if full; use a transactional outbox and durable broker (for example Azure Service Bus) if audit delivery must survive process restarts or scale across instances. EF migrations create the Claims/Covers tables and audit tables on SQL Server startup.

## Tests

Run `dotnet test Claims.slnx` to execute domain, repository, and API integration tests. Repository and integration tests use SQLite in-memory databases; no external database or Docker service is required to run tests.

## Manual API testing

Import `postman/ClaimsApi.postman_collection.json` into Postman. Start the API with `dotnet run --project Claims.WebApi`; the launch profile opens Swagger and the collection targets `http://localhost:5180` by default. Run the requests in order: the collection creates a cover and claim, saves their IDs, exercises premium and validation behavior, then deletes the test records. Change the collection variable `baseUrl` if the API is listening on a different address.
# React client

The React and TypeScript API playground lives in `Claims.Client`.

1. In `Claims.Client`, run `yarn install` once, then `yarn start` to launch the React UI by itself.
2. Open `http://localhost:3000`. Start `Claims.WebApi` separately when you want the endpoint actions to reach the backend. The API base URL is configurable in the top bar; the Swagger link opens the API documentation.

The left panel documents the application layers and business rules. The right panel provides claims and covers list/create/read/delete actions and premium calculation. API validation errors are shown inline. Set `VITE_API_BASE_URL` in a local `.env` file to override the default API URL.
