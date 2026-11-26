# AGENTS.md

Business rules, the endpoint list and project-specific technical facts (database provider, the value of `[Project]`, known exceptions) live in `Requirements.md`. Read it before adding or changing a use case. This file describes how the code is written and is the same for every project that uses this architecture.

Placeholders used below: `[Project]` (project name prefix, e.g. for the `DbContext`), `[Module]` (feature area, plural, e.g. `Students`), `[Entity]` (domain entity), `[UseCaseName]` (one feature, e.g. `CreateStudent`), `[Action]`, `[Property]`, `[Reason]`.

## Stack
- ASP.NET Core Web API with **Controllers**. The target framework, nullable and implicit usings are set once in `Directory.Build.props`.
- **Vertical Slice Architecture + CQRS**: the code is organized by feature, not by technical layer.
- **No MediatR/Mediator. No AutoMapper** (direct handler injection, manual mapping only)
- **No repositories and no `DbContext` interface**: handlers use `[Project]DbContext` directly.
- FluentValidation, EF Core, Scrutor, Serilog

## Vertical Slices
- A **slice** is one use case (one command or one query) with everything it needs: request, validator, handler, and the response record when only this slice returns it. Changing a feature should touch one folder.
- Slices are grouped by **module** (`Features/[Module]/`). A module also holds its controller and what several of its slices share (shared response, errors, projections).
- Slices do not call each other's handlers. When two slices need the same logic, move it to the domain entity (business rule) or to the module folder (shared projection or response). Only code used by **several modules** goes to `Common/`.
- Prefer a little duplication between slices over a premature shared abstraction. Extract only when the same code is needed in 2+ places and has the same reason to change.
- A slice may reference another module's errors when it reports that module's failure (e.g. an enrollment slice returns `Students.NotFound`). It must not use another module's slice types (commands, queries, slice-only responses).

## Projects
- `Domain`: entities, enums, domain exceptions. References nothing. Business rules live here and are shared by every slice.
- `WebAPI`: everything else — features, controllers, database, cross-cutting code, `Program.cs`. References `Domain`.

## Structure
```
Domain/
  Entities/
WebAPI/
  Features/
    [Module]/
      [Module]Controller.cs
      [Entity]Response.cs          (when 2+ slices of the module return it)
      [Entity]Mappings.cs          (when 2+ slices share a projection)
      [Module]Errors.cs
      [Module]Module.cs            (only when the module registers its own services)
      [UseCaseName]/
        [UseCaseName]Command.cs  or  [UseCaseName]Query.cs
        [UseCaseName]CommandValidator.cs  or  [UseCaseName]QueryValidator.cs
        [UseCaseName]CommandHandler.cs  or  [UseCaseName]QueryHandler.cs
        [Name]Response.cs          (when only this slice returns it)
        [Action]Request.cs         (when the body differs from the command, e.g. route values)
  Database/
    [Project]DbContext.cs
    [Project]Seed.cs
    SqliteErrors.cs                (provider error codes; name after the provider)
    Configurations/[Entity]Configuration.cs
    Migrations/
  Common/                          (only code used by several modules)
    Cqrs/            (ICommandHandler, IQueryHandler, ValidationDecorator, LoggingDecorator)
    Results/         (Result, Error, PagedResult, ResultExtensions)
    Pagination/      (Pagination, PageQueryValidator)
    ExternalServices/(ExternalServicesOptions, ExternalServiceErrors)
    Diagnostics/     (CorrelationIdMiddleware, GlobalExceptionHandler, ProblemDetailsLoggingFilter)
  DependencyInjection.cs           (AddFeatures, AddDatabase, MigrateDatabaseAsync)
  Program.cs
```
- There are no `Commands/` and `Queries/` folders: the file names (`...Command`, `...Query`) already say which is which, and every slice folder sits directly under its module.
- Namespaces follow folders: `WebAPI.Features.[Module]`, `WebAPI.Features.[Module].[UseCaseName]`, `WebAPI.Database`, `WebAPI.Common.[Area]`.
- Where a response lives:
  - returned by one slice → in that slice's folder;
  - returned by 2+ slices of one module → in the module folder;
  - never in `Common/` unless several modules return it.

## Rules
- Commands change state and return `Result<T>` when there is a response payload (e.g. `Result<Guid>` for a created entity ID), or `Result` when there is no payload. Queries are read-only and return `Result<TResponse>`.
- Commands and queries are plain `public sealed record`s. No marker interfaces.
- Handlers: `internal sealed`, primary constructor, implement `ICommandHandler<TCommand, TResult>` or `IQueryHandler<TQuery, TResult>`, always pass `CancellationToken`.
- For expected failures (NotFound, Validation, Conflict), return a failed `Result` or `Result<T>` containing an `Error`.
- Validation and logging run through `ValidationDecorator` and `LoggingDecorator`, applied to every handler. Follow the Transactions section for transaction handling.
- DI: `AddFeatures()` uses Scrutor to register every command/query handler and every validator in the WebAPI assembly as scoped services (`publicOnly: false` to include `internal sealed` types; open generic decorators are excluded), then applies the decorators. A new slice needs no registration.
- A module that needs its own services (an HTTP client, options) registers them in `Features/[Module]/[Module]Module.cs` (`Add[Module]Module()`), called from `AddFeatures()`.

## Data Access
- Handlers inject `[Project]DbContext` (`internal sealed`, in `Database/`) and use its `DbSet<[Entity]>` properties directly: queries, `Add`, `Remove`, `SaveChangesAsync`. Each slice writes exactly the query it needs.
- Do not add repositories, a generic repository, or an interface over the `DbContext`.
- Database-provider details (e.g. error codes for primary-key or unique violations) live in one file in `Database/` (e.g. `SqliteErrors.IsPrimaryKeyViolation(...)`). Handlers call that helper and never repeat provider numbers.
- When a query is complex and reused by 2+ slices, put it as an extension method on the `DbSet`/`IQueryable` in the module folder. Do not put feature queries in `Database/`.

## EF Core Entity Configuration
- Each persisted entity must define its EF Core mapping in `[Entity]Configuration.cs` under `WebAPI/Database/Configurations` using `IEntityTypeConfiguration<[Entity]>`.
- `OnModelCreating` must apply these configurations and, when the project has seed data, call `[Project]Seed.Configure(modelBuilder)`.
- Column lengths come from the entity's constants (e.g. `[Entity].Maximum[Property]Length`), never from literal numbers.
- Enforce uniqueness in the database (keys or unique indexes), not only in handlers.

## Migrations
- Create migrations with the EF Core CLI only, from the repository root:
  `dotnet ef migrations add <Name> --project WebAPI --output-dir Database/Migrations`
- Do not hand-write new migrations and do not edit a migration that has already been applied. Change the model, then add a new migration, and check that it preserves existing data.
- Seed data lives in `[Project]Seed`. Changing it requires a new migration.
- Known exceptions to these rules (e.g. an old hand-written migration) are listed in `Requirements.md`. Do not copy their style.
- The database is migrated automatically at startup (`MigrateDatabaseAsync` in `Program.cs`).

## Mapping
- Commands: build entities through domain factory methods (`[Entity].Create(...)`), not object initializers.
- Queries: use `AsNoTracking()` and project directly to the response with `Select`. Never load the entity and map it in memory.
- When 2+ slices of a module use the same projection, define it once as a `static readonly Expression<Func<[Entity], [Entity]Response>> ToResponse` in `Features/[Module]/[Entity]Mappings.cs` and pass it to `Select`. A projection used by one slice stays inline in its handler.

## Validation
- Every command has a validator.
- Every query that takes an ID or is paginated has a validator. IDs are checked with `NotEmpty()` (route constraint `{id:guid}` still accepts `Guid.Empty`).
- Paginated query validators inherit from `PageQueryValidator<T>`.
- Validators mirror domain constraints using the entity's constants (`[Entity].Maximum[Property]Length`), so invalid user input is rejected before reaching the domain.
- Domain factory methods still guard their invariants with `ArgumentException` / `ArgumentOutOfRangeException`. These only fire on programming errors and end up in the global exception handler as a 500.

## Errors
- Each module has an `internal static class [Module]Errors` in `Features/[Module]/`, with `public static readonly Error` fields.
- Error codes use the form `[Module].[Reason]` with a plural module name, e.g. `[Module].NotFound`.
- `ErrorType` maps to HTTP status in `ResultExtensions.ToProblemDetails`: `Validation` → 400, `NotFound` → 404, `Conflict` → 409, `BadGateway` → 502, `GatewayTimeout` → 504. The error code is returned in the `code` extension of the `ProblemDetails`.

## Controllers
- One controller per module, in the module folder: `Features/[Module]/[Module]Controller.cs`. It only maps HTTP to slices: no business logic, no data access.
- Inject handlers per action with `[FromServices]`, convert a failed `Result` with `result.ToProblemDetails(HttpContext)`.
- Request body:
  - When the body maps 1:1 to the command, bind the command record directly (e.g. `Create[Entity]Command`).
  - When the command also needs route values, define `[Action]Request` in the slice folder and build the command in the action.
- Responses:
  - Create command returning an ID → `201 Created` via `CreatedAtAction(nameof(Get), new { id }, id)`.
  - Command without payload → `204 No Content`.
  - Query → `200 OK` with the response.
- Every action has `[EndpointSummary]` and a `[ProducesResponseType]` for the success status and for every error status it can return (`ValidationProblemDetails` for 400, `ProblemDetails` for 404/409/502/504).
- List endpoints take `page` and `pageSize` from the query string with defaults from `Pagination`.
- Keep `WebAPI/WebAPI.http` in sync with the endpoints: add an example for every new endpoint and remove examples for deleted ones.
- API documentation, Development only: `MapOpenApi()` serves the OpenAPI document at `/openapi/v1.json`, and `MapScalarApiReference()` (package `Scalar.AspNetCore`) serves an interactive UI at `/scalar`. Both are generated from the controllers, so `[EndpointSummary]` and `[ProducesResponseType]` are what the UI shows. Do not expose them outside Development.

## External Services
Placeholder: `[Service]` (external API name, e.g. the provider's product name).
- Each external service is one module: `Features/[Service]/`, with one controller `[Service]Controller` (route prefix `api/[service]`, lowercase) holding all of that service's endpoints. Do not split a service across modules per resource.
- Slices are named by the resource (e.g. `ListUsers`, `GetUser`, `CreatePost`); responses are `[Service][Resource]Response`; errors are `[Service]Errors` with codes `[Service].[Reason]` (e.g. `[Service].UserNotFound`).
- The HTTP client lives in the module, in `Features/[Service]/Client/`:
  - `I[Service]Client` (`internal`): returns `Result<T>` with the module's response records, so handlers can be tested with a fake.
  - `[Service]Client`: `internal sealed` typed HttpClient.
  - `[Service]Options`: the service's own settings (its `BaseUrl`), bound from `ExternalServices:[Service]`. `BaseUrl` is written in configuration as an absolute URL ending with `/`; it is not validated or normalized in code. Never hard-code URLs or timeouts.
  - `[Service]Models.cs`: `internal` records matching the external JSON. They never leave the `Client` folder; the client maps them manually to response records.
- `[Service]Module.cs` registers the client with `AddHttpClient<I[Service]Client, [Service]Client>`, setting `BaseAddress` from `[Service]Options.BaseUrl` and `Timeout` from `ExternalServicesOptions.TimeoutSeconds`.
- `Common/ExternalServices/ExternalServicesOptions`: settings shared by all external services (one `TimeoutSeconds`), bound from the `ExternalServices` section in `AddFeatures()`. It is set in configuration and not validated in code. Do not add a per-service timeout.
- The client translates every expected failure in one private `SendAsync` helper and never throws for them:
  - HTTP 404 → `ExternalServiceErrors.NotFound`
  - any other non-success status or `HttpRequestException` → `ExternalServiceErrors.Unavailable` (502)
  - `OperationCanceledException` while the caller's token is not cancelled (the timeout elapsed) → `ExternalServiceErrors.Timeout` (504)
  - `JsonException`, an empty body, or a missing expected header → `ExternalServiceErrors.InvalidResponse` (502)
  - a cancellation requested by the caller is rethrown.
  Each failure is logged as a Warning with method and URL.
- Handlers translate `ExternalServiceErrors.NotFound` to the service's own error (e.g. `[Service]Errors.UserNotFound`) and return other errors as they are.
- External list endpoints still return `PagedResult<T>`: use the external API's paging parameters and total-count header when it has them; do not fetch everything and page in memory.
- No automatic retries by default. Never retry non-idempotent requests (POST) automatically.
- External services are not added to `/health`: their outage does not make this application unhealthy.
- A create call to an external API with no local read endpoint returns `201` via `Created((string?)null, value)`.

## Cross-Cutting Rules

**Global error handling**
- Unexpected exceptions are handled by a single `IExceptionHandler` (`GlobalExceptionHandler`) that returns a uniform `ProblemDetails`.
- Never return stack traces or exception messages to the client. Log the exception server-side.
- Every failed response is logged with the exact `ProblemDetails` JSON sent to the client (`Responded {StatusCode} with {ProblemDetails}`): `ProblemDetailsLoggingFilter`, a global result filter, logs it for controller results (Warning for 4xx, Error for 5xx), and `GlobalExceptionHandler` logs it for unexpected exceptions. Successful responses are not logged this way.

**Transactions**
- One `SaveChangesAsync` per command is the default and is already atomic.
- If a command needs multiple saves/operations in one transaction, open an explicit transaction inside that handler. No global transaction decorator.

**Concurrency**
- Use a concurrency token (`RowVersion`, a `Guid` regenerated by the entity on every change) on entities that support concurrent edits.
- Include `rowVersion` in read responses and require it in update requests.
- Use the client-supplied `rowVersion` as the expected version when saving the update.
- Return a `Conflict` error (HTTP 409) if the version is stale or EF Core raises `DbUpdateConcurrencyException`.
- An operation that changes a counter or state on another entity as a side effect does not take a client `rowVersion`, but still catches `DbUpdateConcurrencyException` and returns a `Conflict` error.
- Catch `DbUpdateConcurrencyException` before `DbUpdateException` (it derives from it).

**Pagination and filtering**
- Every list query returns `Result<PagedResult<T>>` (items, page, pageSize, totalCount) using `ToPageAsync`.
- Page size has a default (`Pagination.DefaultPageSize`) and a hard maximum (`Pagination.MaximumPageSize`) enforced in the query validator.
- Every list query has a deterministic order: a business field first, then `Id` as a tie-breaker.
- Client-side filtering and sorting use one shared pattern in `Common/Pagination`. If it does not exist yet, design it, document it here, and only then use it. Do not add ad-hoc filter parameters to a single endpoint.

**Domain entities**
- Entities are not anemic: business rules live inside them (`[Entity].Create(...)`, `entity.Cancel()`), with private setters and a private parameterless constructor for EF Core. This is how slices share business rules without depending on each other.
- Handlers orchestrate (load, call domain methods, save). They do not contain business rules.
- Checks that need the database (an entity exists, a duplicate exists) are done in the handler before calling the domain method, and return the matching `Error`.

**Observability**
- Use **Serilog** for structured logging (inject `ILogger<T>`, no static `Log` calls in application code).
- Serilog sinks and minimum levels are configured only in the `Serilog` section of `appsettings*.json` (read with `ReadFrom.Configuration`), never in code. Logs go to the console and to daily rolling, human-readable text files `logs/log-yyyyMMdd.txt` under the app's working directory, kept for 14 days. The `logs/` folder is git-ignored. The file uses a `Serilog.Expressions` template that draws a separator block before every event whose `EventId.Name` is `RequestStarted`; `CorrelationIdMiddleware` logs that event (method, path, query string, correlation id) at the start of each request. Do not log other events with that EventId. To trace a request, search the files for its `CorrelationId` and read down from its separator. Lines from concurrent requests can interleave.
- EF Core sensitive data logging (SQL parameter values in logs and in EF exception messages) is switched by `Database:EnableSensitiveDataLogging` in `appsettings*.json` and passed to `AddDatabase()`. Never hard-code it. Where it is enabled, treat log files as containing personal data. The value each project uses is recorded in `Requirements.md`.
- Every request has a correlation id set by `CorrelationIdMiddleware` from `HttpContext.TraceIdentifier`. It is server-generated (an incoming header is not trusted), returned in the `X-Correlation-Id` response header, added to the log scope, and included as `correlationId` in every `ProblemDetails`.
- Expose `/health` with a database readiness check from the start.

## Testing
- If the solution has a test project, add or update tests for every changed slice and run them.
- Always verify changes by building the solution file (`*.slnx` / `*.sln`) at the repository root with `dotnet build` and by running the affected requests in `WebAPI/WebAPI.http`, including the failure cases.

## New Use Case Checklist
1. Check `Requirements.md` for the business rules; add the new rule there if it is missing.
2. Create the slice folder `Features/[Module]/[UseCaseName]/` (and the module folder with its controller and `[Module]Errors.cs` if the module is new).
3. Add the command or query record, its validator and its handler (see Rules, Validation). Put the response in the slice folder, or in the module folder if another slice already returns it.
4. Query the database through `[Project]DbContext` directly (see Data Access).
5. Add errors to `[Module]Errors.cs` if needed.
6. If the model changed, add a migration with the EF Core CLI (see Migrations).
7. Add the action to `[Module]Controller` following the Controllers section, and an example request in `WebAPI.http`. No DI registration needed.
8. Add the endpoint to `Requirements.md`.
9. Run `dotnet build` on the solution file at the repository root.
