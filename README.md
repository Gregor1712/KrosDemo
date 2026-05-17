# KrosDemo

A small but production-style invoicing API plus an Angular client, built as a portfolio demo of patterns frequently asked about in enterprise .NET interviews: Clean Architecture, JWT auth, ETag/If-Match optimistic concurrency, Stripe-style idempotency, and the **Transactional Outbox** pattern with domain events.

> **Domain:** *Faktúra* (Invoice) + *Položky faktúry* (Invoice items). Modeled after the kind of accounting/ERP entities used by KROS s.r.o.

---

## Table of contents

- [Stack](#stack)
- [Repository layout](#repository-layout)
- [Quick start (Docker)](#quick-start-docker)
- [Local development (without Docker)](#local-development-without-docker)
- [Configuration](#configuration)
- [API overview](#api-overview)
- [How it works](#how-it-works)
  - [Clean Architecture](#clean-architecture)
  - [Authentication & authorization](#authentication--authorization)
  - [Optimistic concurrency with ETag / If-Match](#optimistic-concurrency-with-etag--if-match)
  - [Idempotent POSTs (Idempotency-Key)](#idempotent-posts-idempotency-key)
  - [Domain events + Transactional Outbox](#domain-events--transactional-outbox)
  - [Filter & sort system](#filter--sort-system)
  - [Global error handling](#global-error-handling)
- [Testing](#testing)
- [Known caveats](#known-caveats)

---

## Stack

| Layer            | Tech                                                               |
| ---------------- | ------------------------------------------------------------------ |
| API              | ASP.NET Core 8, controllers, NSwag/OpenAPI, NLog                   |
| Persistence      | EF Core 8 (SQL Server in containers, SQLite in tests)              |
| Validation       | FluentValidation (auto-validation)                                 |
| Mapping          | AutoMapper                                                         |
| Auth             | JWT bearer (HMAC-SHA256)                                           |
| Background       | `BackgroundService` (Outbox dispatcher)                            |
| Frontend         | Angular 21 + Material, served by nginx in container                |
| Tests            | xUnit, `WebApplicationFactory`, Moq                                |
| Orchestration    | Docker Compose (api + client + mssql 2022)                         |

---

## Repository layout

```
KrosDemo/
├── src/
│   ├── KrosDemo.Domain/              Entities, value objects, domain events
│   ├── KrosDemo.Application/         Services, DTOs, validators, mapping,
│   │                                 outbox handlers, filter/spec primitives
│   ├── KrosDemo.Infrastructure/      EF Core DbContext, repositories,
│   │                                 outbox processor, JWT, seeders
│   └── KrosDemo.Api/                 Controllers, middleware, Program.cs
├── client/                           Angular SPA (separately Dockerized)
├── tests/KrosDemo.Tests/             Unit + integration tests
├── docker-compose.yml                api + client + db
└── start.bat                         One-command launcher (Windows)
```

The four backend projects map to a textbook Clean Architecture: `Domain` has no dependencies, `Application` depends on `Domain`, `Infrastructure` depends on both, and `Api` composes the lot.

---

## Quick start (Docker)

**Prerequisites:** Docker Desktop running.

```cmd
start.bat
```

…or the equivalent without the helper:

```bash
docker compose up --build -d
```

This builds the API + client images and brings up three containers:

| Container          | Host port        | What                                 |
| ------------------ | ---------------- | ------------------------------------ |
| `krosdemo-db`      | `localhost,1433` | SQL Server 2022 (Developer)          |
| `krosdemo-api`     | `localhost:5000` | The ASP.NET Core API                 |
| `krosdemo-client`  | `localhost:4200` | nginx serving the Angular SPA        |

Once everything is healthy:

- App: <http://localhost:4200>
- Swagger UI: <http://localhost:5000/swagger> (also proxied at `/swagger` on the client)
- Health-check the DB: `docker compose logs db | head`

**Stop / clean up:**

```bash
docker compose down            # stop containers, keep DB volume
docker compose down -v         # also wipe the SQL Server volume
```

**See logs live:**

```bash
docker compose logs -f api
```

---

## Local development (without Docker)

If you'd rather run the API on the host directly (e.g. in JetBrains Rider or Visual Studio) and only use Docker for SQL Server:

1. Bring up just the database:

   ```bash
   docker compose up -d db
   ```

2. Point the API at it. Either edit `src/KrosDemo.Api/appsettings.Development.json` or override via user-secrets:

   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost,1433;Database=KrosDB;User Id=sa;Password=Your_password123;TrustServerCertificate=true;Encrypt=false"
     }
   }
   ```

3. Run the API:

   ```bash
   dotnet run --project src/KrosDemo.Api
   ```

4. Run the Angular app:

   ```bash
   cd client
   npm ci
   npm start          # http://localhost:4200, proxies /api → https://localhost:5001
   ```

   The dev server's proxy is defined in `client/proxy.conf.json` and wired up via `angular.json`'s `serve` target.

---

## Configuration

All settings can be overridden via standard ASP.NET Core configuration sources (env vars take precedence over `appsettings*.json`). Most relevant keys:

| Key                                                | Default (dev)                                                | Purpose                              |
| -------------------------------------------------- | ------------------------------------------------------------ | ------------------------------------ |
| `ConnectionStrings:DefaultConnection`              | local SQL Server                                             | EF Core DB connection                |
| `JwtSettings:Secret`                               | dev-only placeholder                                         | HMAC-SHA256 signing key for JWT      |
| `ASPNETCORE_ENVIRONMENT`                           | `Development` (in compose), `Production` (default in image)  | controls Swagger, dev-time settings  |

In Docker, env vars are set in `docker-compose.yml`. The `__` (double underscore) is the standard ASP.NET Core delimiter for nested keys, e.g.:

```yaml
ConnectionStrings__DefaultConnection: "Server=db,1433;Database=KrosDB;User Id=sa;Password=Your_password123;TrustServerCertificate=true;Encrypt=false"
JwtSettings__Secret: "DevOnly-LocalSecret-AtLeast32BytesLong-For-HMAC-SHA256"
```

---

## API overview

All endpoints are under `/api`. The collection endpoints support paging, filtering, and sorting via query string.

| Method | Route                                | Auth        | Notes                                     |
| ------ | ------------------------------------ | ----------- | ----------------------------------------- |
| POST   | `/api/Auth/register`                 | anonymous   | returns `{ token }`                       |
| POST   | `/api/Auth/login`                    | anonymous   | returns `{ token }`                       |
| POST   | `/api/Auth/logout`                   | bearer      | client-side token discard                 |
| GET    | `/api/invoices`                      | `User`      | paged list, filterable                    |
| GET    | `/api/invoices/{id}`                 | `User`      | returns `ETag` header                     |
| POST   | `/api/invoices`                      | `User`      | honours `Idempotency-Key`                 |
| PUT    | `/api/invoices/{id}`                 | `User`      | requires `If-Match`                       |
| DELETE | `/api/invoices/{id}`                 | `User`      | requires `If-Match`                       |
| POST   | `/api/invoices/{id}/send`            | `User`      | raises `InvoiceSentEvent`                 |
| CRUD   | `/api/invoiceitems/...`              | `Admin`     | child entity CRUD                         |

Browse the full surface at `/swagger`.

---

## How it works

### Clean Architecture

```
Domain  ←  Application  ←  Infrastructure
                   ↖                  ↑
                    └── Api ──────────┘
```

- **Domain** — entities (`Invoice`, `InvoiceItem`, `User`), enums (`InvoiceStatus`), and domain events (`InvoiceIssuedEvent`, `InvoiceSentEvent`). `BaseEntity` carries a small list of pending domain events so an aggregate can announce *what happened* without knowing *who reacts*.
- **Application** — service interfaces, DTOs, AutoMapper profiles, FluentValidation validators, the filter/specification system, and the outbox **handler** interface plus its concrete implementations.
- **Infrastructure** — `ApplicationDbContext`, repositories, the `OutboxProcessor` background service, JWT issuance, and CSV/user seeders.
- **Api** — controllers, middleware (`GlobalExceptionHandlingMiddleware`, `IdempotencyMiddleware`), composition root in `Program.cs`.

### Authentication & authorization

Auth is JWT bearer (HMAC-SHA256). `AuthController.Register` and `Login` mint a token containing `NameIdentifier`, `Name`, and a `Role` claim (`User` or `Admin`). Every other controller is `[Authorize]` and methods opt into role-specific access (`[Authorize(Roles = "User")]`, `[Authorize(Roles = "Admin")]`).

The frontend stores the token and attaches it via an HTTP interceptor (`client/src/app/service/auth.interceptor.ts`). A second interceptor (`unauthorized.interceptor.ts`) routes 401 responses back to the login screen.

### Optimistic concurrency with ETag / If-Match

`Invoice` and `InvoiceItem` carry a SQL Server `rowversion` column (`RowVersion : byte[]`) mapped via `.IsRowVersion()`. The API exposes that token as an HTTP `ETag` and requires the client to echo it back on mutating requests:

| Step | Server                                                                                              |
| ---- | --------------------------------------------------------------------------------------------------- |
| GET  | response includes `ETag: "<base64 rowversion>"`                                                     |
| PUT/DELETE | client must send `If-Match: "<that ETag>"`                                                    |
| Missing header | server returns **428 Precondition Required** (`application/problem+json`)                 |
| Stale ETag | server returns **409 Conflict** with the current DB values in `currentDatabaseValues`         |
| Success    | response carries the *new* ETag for the next round-trip                                       |

Header parsing lives in `KrosDemo.Api.Infrastructure.ETag`. Conflict detection rides on EF Core's built-in `DbUpdateConcurrencyException`, surfaced through `GlobalExceptionHandlingMiddleware`.

This is the same pattern AWS S3, GitHub's API, and Stripe use for resource versioning — it lets clients safely retry without trampling concurrent edits.

### Idempotent POSTs (Idempotency-Key)

Stripe-style: a client can opt in by sending an `Idempotency-Key` header on a `POST`. The first request runs normally; the resulting status code, headers (Location, ETag), and body are cached in `IDistributedCache` for 24 hours under `idem:{METHOD}:{PATH}:{KEY}`. Any subsequent request with the same key + path **never reaches the controller** — it's replayed from cache and marked with `X-Idempotent-Replay: true`.

```
Client → POST /api/invoices, Idempotency-Key: abc
         ↓
IdempotencyMiddleware
    ├── cache hit?  → return cached response + X-Idempotent-Replay: true
    └── cache miss  → run controller, buffer response, write to cache, forward
```

Implemented in `KrosDemo.Api.Infrastructure.IdempotencyMiddleware`. The current backend uses an in-memory `IDistributedCache` for simplicity; swap to Redis (`AddStackExchangeRedisCache`) for multi-instance deployments — middleware code doesn't change.

### Domain events + Transactional Outbox

This is the centerpiece of the demo.

**The problem.** When `Invoice.MarkAsIssued()` happens, we want to do two things: persist the state change *and* notify the outside world (audit log, email, etc.). The naive approach — `db.SaveChangesAsync()` followed by `emailService.SendAsync()` — has a well-known failure mode: the email send can fail (network, crash, queue down) *after* the DB commit, so the system loses an event. Conversely, if you send the email first and the DB then fails, you've notified about something that never happened. **Dual-write inconsistency.**

**The fix: Transactional Outbox.** Persist the *intention* to publish into the same DB transaction as the state change, and have a background process drain that intention later.

```
   Aggregate                    DbContext.SaveChangesAsync               Outbox table
 ┌───────────┐    AddDomain   ┌──────────────────────────────┐         ┌────────────┐
 │  Invoice  │───  Event ───▶ │  begin tx                    │         │ Id         │
 │  (issued) │                │  base.SaveChanges() (state)  │  +new   │ Type       │
 └───────────┘                │  for each entity → enqueue   │ ──────▶ │ Payload    │
                              │  base.SaveChanges() (outbox) │         │ Occurred   │
                              │  commit tx                   │         │ Processed  │
                              └──────────────────────────────┘         │ RetryCount │
                                                                       └─────┬──────┘
                                                                             │
                                          OutboxProcessor (every 5s)         │
                                       ┌─────────────────────────────────┐   │
                                       │ SELECT unprocessed, ORDER BY    │◀──┘
                                       │ OccurredOnUtc, TAKE 50          │
                                       │                                 │
                                       │ for each:                       │
                                       │   resolve Type → IDomainEvent   │
                                       │   resolve registered handlers   │
                                       │   await handler.HandleAsync()   │
                                       │   on success → Processed = now  │
                                       │   on error   → RetryCount++,    │
                                       │                Error = ex.Msg   │
                                       └─────────────────────────────────┘
```

**Flow, end to end:**

1. A controller (e.g. `POST /api/invoices/{id}/send`) calls `invoice.Send()`. The aggregate flips its status and calls `AddDomainEvent(new InvoiceSentEvent(...))` — events sit on the entity, *not* dispatched yet.
2. The service calls `IUnitOfWork.SaveChangesAsync()`. The overridden `ApplicationDbContext.SaveChangesAsync` does three things in **one** transaction:
   1. `base.SaveChangesAsync()` — persists entity changes.
   2. Iterates over each entity's `DomainEvents`, serializes them as JSON, and inserts an `OutboxMessage` per event.
   3. `base.SaveChangesAsync()` again to flush the outbox rows, then commits.
   If any step throws, the whole transaction rolls back — **no half-states, ever**.
3. `OutboxProcessor` (a `BackgroundService`) polls every 5 s for `OutboxMessage` rows where `ProcessedOnUtc IS NULL AND RetryCount < 5`. It deserializes the JSON payload back to its concrete `IDomainEvent` type (`AssemblyQualifiedName` stored as the discriminator), resolves all registered `IOutboxMessageHandler<TEvent>` instances from DI, and `await`s them.
4. On success, `ProcessedOnUtc` is stamped. On failure, `RetryCount++` and the exception message lands in `Error`. After 5 failures the row is poisoned and ignored (no death loop, no blocking the queue).

**Properties this gives you:**

| Property             | How                                                                                 |
| -------------------- | ----------------------------------------------------------------------------------- |
| Atomicity            | State + outbox row are written in the same transaction                              |
| At-least-once delivery | Polling keeps retrying unprocessed rows until they succeed or hit the retry cap   |
| Ordered per source   | Rows are drained `ORDER BY OccurredOnUtc`                                           |
| Crash-safe           | A crash between state commit and dispatch is recoverable — the row is still there  |
| Pluggable handlers   | Add an `IOutboxMessageHandler<TEvent>` in DI; the processor picks it up reflectively |

Handlers are **idempotent by convention** — at-least-once delivery means a handler may run more than once for the same event after a crash. Real handlers should be safe to retry; the demo handlers (`InvoiceIssuedAuditHandler`, `InvoiceSentEmailHandler`) just log, which trivially satisfies that.

**Key files:**

- `src/KrosDemo.Domain/Entities/BaseEntity.cs` — pending event list
- `src/KrosDemo.Domain/Events/*.cs` — event records
- `src/KrosDemo.Infrastructure/Data/ApplicationDbContext.cs` — overridden `SaveChangesAsync` (writes to outbox in same tx)
- `src/KrosDemo.Infrastructure/Data/OutboxMessage.cs` — the persisted row
- `src/KrosDemo.Infrastructure/Outbox/OutboxProcessor.cs` — the polling dispatcher
- `src/KrosDemo.Application/Outbox/IOutboxMessageHandler.cs` — handler contract
- `src/KrosDemo.Application/Outbox/Handlers/*.cs` — sample handlers

### Filter & sort system

`KrosDemo.Application.Filters` ships a small expression-tree-based filter system so collection endpoints accept rich query strings without hand-rolling SQL or LINQ-by-string:

- `ConditionBase<T>` + concrete conditions (`StringCondition`, `IntCondition`, `BoolCondition`, `DateTimeCondition`) build `Expression<Func<T,bool>>` predicates at runtime.
- `FilterBase.Apply<T>(IQueryable<T>)` walks every condition property on a filter DTO, builds the expression, and `Where`s it on the query.
- `OrderByExtension.ApplyOrderBy` does the same for `OrderBy` with reflection-driven type dispatch.

This is paired with the `BaseSpecification<T>` pattern (`Application/Specifications`) for repository-level query composition.

### Global error handling

`GlobalExceptionHandlingMiddleware` maps domain exceptions to RFC 7807 `application/problem+json`:

| Exception                              | Status | Notes                                                |
| -------------------------------------- | ------ | ---------------------------------------------------- |
| `ValidationException` (FluentValidation) | 400   | per-field `errors`                                   |
| `NotFoundException`                    | 404    | `detail` includes the entity id                      |
| `MissingIfMatchException`              | 428    | thrown by controllers for missing `If-Match`         |
| `DbUpdateConcurrencyException`         | 409    | includes `currentDatabaseValues` snapshot            |
| anything else                          | 500    |                                                      |

---

## Testing

```bash
dotnet test
```

The `tests/KrosDemo.Tests` project runs three categories:

- **Unit** — `Services/`, `Filters/`, `Infrastructure/` — plain xUnit + Moq.
- **Integration** — `Integration/` — uses `WebApplicationFactory<Program>` to spin the entire pipeline in-process, swapping SQL Server for in-memory SQLite (one connection kept open for the factory's lifetime).

The integration factory (`TestWebAppFactory.cs`) does two notable swaps:

1. **DB** — replaces `DbContextOptions<ApplicationDbContext>` with a shared SQLite connection. It also installs `SqliteRowVersionModelCustomizer` and `BumpRowVersionInterceptor` because SQLite has no native `rowversion` — the interceptor stamps a monotonically increasing `byte[]` on every save so `If-Match` semantics keep working in tests.
2. **Auth** — installs a `TestAuthHandler` (always-succeeds `AuthenticationHandler`) as the default scheme so `[Authorize]` endpoints are reachable without minting real JWTs. The handler issues both `User` and `Admin` role claims.

---

## Known caveats

- **AutoMapper 12.0.1 has a high-severity NuGet advisory (NU1903).** This is suppressed in `KrosDemo.Application.csproj` via `<NoWarn>` because upgrading crosses a license boundary (AutoMapper 14+ moved to a commercial license). For production use, audit the advisory or migrate to e.g. Mapperly.
- **Cold-start race between API and SQL Server.** On a fresh `docker compose up`, mssql's healthcheck can pass on its local socket a few hundred ms before it accepts remote TCP connections. The API's EF migration runs once on boot and has a 15 s `SqlClient` connect timeout — usually fine, occasionally not. If the API exits during a cold start, just `docker compose start api` once mssql is healthy.
- **JWT secret is committed.** The default `JwtSettings:Secret` is fine for local dev only. Rotate before any real deployment.
- **Idempotency cache is in-process.** `AddDistributedMemoryCache` is wired up; for production behind multiple API instances, swap to Redis (`AddStackExchangeRedisCache`) — the middleware is agnostic.