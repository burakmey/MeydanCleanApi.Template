# MeydanCleanApi.Template

An ASP.NET Core Web API starter built on Clean Architecture and CQRS, targeting .NET 10 and
PostgreSQL.

This page covers getting a new project running. Each layer has its own README with the detail for
that layer.

---

## What is in the box

| Area | What it provides |
| :--- | :--- |
| Architecture | Domain, Application, Infrastructure, Persistence, WebApi, with no reverse references |
| CQRS | Commands, queries and handlers dispatched by a small built-in mediator, no MediatR licence needed |
| Validation | FluentValidation as a pipeline behavior; failures return HTTP 400 with per-field messages |
| Persistence | EF Core on PostgreSQL, generic repositories, unit of work, seeders |
| Auth | ASP.NET Core Identity, JWT access tokens, refresh token rotation, Google sign-in, role policies |
| Files | Direct-to-storage uploads with presigned URLs; local disk and Supabase providers included |
| Localization | `.resx` resources in English and Turkish, selected per request |
| Operations | Serilog, correlation ids, rate limiting, CORS, health checks, settings validated at startup |

---

## Layer map

```text
Domain          entities, enums, domain exceptions, constants        depends on nothing
   ▲
Application     commands, queries, handlers, abstractions            depends on Domain
   ▲
   ├── Infrastructure   auth, tokens, storage providers, clock       depends on Application
   └── Persistence      DbContext, repositories, seeders             depends on Application
          ▲
        WebApi          controllers, middleware, DI composition      depends on all of the above
```

---

## Requirements

- .NET 10 SDK
- Visual Studio 2026, or any editor with C# support
- PostgreSQL 14 or newer
- The EF Core CLI: `dotnet tool install --global dotnet-ef`

Docker replaces the PostgreSQL and EF Core requirements. See [Running in Docker](#running-in-docker).

---

## Getting started

### 1. Create a project

Install the template, then create a project from it:

```bash
dotnet new install .
```

```bash
dotnet new meydanclean -n YourApi
```

The name after `-n` replaces `MeydanCleanApi.Template` everywhere: folder names, project names,
namespaces and the text of these documents. `-n YourApi` produces `YourApi.Domain`,
`YourApi.Application`, `YourApi.Infrastructure`, `YourApi.Persistence`, `YourApi.WebApi` and
`YourApi.slnx`. **Every command below is written with the template name, and reads with the chosen
name inside a generated project.**

Two options are available:

- `--IncludeSamples false` leaves out the `SampleProduct` reference feature.
- `--DatabaseName your_db` sets the database name used in the sample connection string.

Cloning this repository and renaming the projects by hand works too.

### 2. Supply the settings

`appsettings.Development.json` and `.env.example` carry throwaway signing keys so a clone runs
straight away. They are in the repository, which means they are public, so the API refuses to start
outside Development while any of them is still configured and names the key at fault.

Nothing else is committed. Run these from the `*.WebApi` folder; they are stored outside the
repository, so nothing typed here can be committed by accident:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=meydanclean_db;Username=postgres;Password=YOUR_PASSWORD"
```

```bash
dotnet user-secrets set "Tokens:Jwt:JwtSecurityKey" "replace-with-32-or-more-random-characters"
```

```bash
dotnet user-secrets set "Tokens:Jwt:RefreshSecurityKey" "replace-with-a-different-32-plus-character-string"
```

An administrator account is created on first run when these two are also set. The seeder only acts
on a completely empty users table, so it never replaces an existing account:

```bash
dotnet user-secrets set "SeedData:SuperAdminEmail" "admin@example.com"
```

```bash
dotnet user-secrets set "SeedData:SuperAdminPassword" "YOUR_STRONG_PASSWORD"
```

Production reads the same keys from environment variables, with `__` for nesting, for example
`Tokens__Jwt__JwtSecurityKey`. The full explanation is in the
[WebApi README](MeydanCleanApi.Template.WebApi/README.md#configuration--secrets).

### 3. Create the first migration

The repository ships an `InitialCreate` migration, so cloning and running works straight away. The
API applies pending migrations itself at startup, so no command is needed to get going.

A project created with `dotnet new` does **not** receive it, because the migration must match the
chosen options and the entity names of the new domain. Generate the first one there:

```bash
dotnet ef migrations add InitialCreate --project MeydanCleanApi.Template.Persistence --startup-project MeydanCleanApi.Template.WebApi
```

After renaming entities in a cloned copy, delete the `Migrations` folder and run the same command to
regenerate from the new model.

### 4. Run it in Visual Studio 2026

Open `MeydanCleanApi.Template.slnx`, make sure the `*.WebApi` project is the startup project, and
press F5. The browser opens Swagger UI.

From the command line:

```bash
dotnet run --project MeydanCleanApi.Template.WebApi
```

Swagger UI is at `/swagger` in Development.

Three health endpoints are mapped:

| Endpoint | Checks | Access |
| :--- | :--- | :--- |
| `/health` | nothing, only that the process answers | anonymous |
| `/health/ready` | the database | anonymous |
| `/health/details` | everything, with per-check timings and failure reasons | Admin |

The first two stay anonymous because load balancers and orchestrators probe without a token.

---

## Running in Docker

Docker Compose runs the API and a PostgreSQL 18 container together, so neither PostgreSQL nor the
.NET SDK has to be installed:

```bash
cp .env.example .env
```

```bash
docker compose up -d --build
```

The API is then at http://localhost:5080/swagger.

**[DOCKER.md](DOCKER.md) is the full guide** — the Docker words explained, what the images,
containers and volumes are for, daily commands, disk cleanup and troubleshooting.

---

## Where to read more

| Layer | Covers |
| :--- | :--- |
| [Domain](MeydanCleanApi.Template.Domain/README.md) | Entities, base classes, enums, domain exceptions, error codes |
| [Application](MeydanCleanApi.Template.Application/README.md) | CQRS flow, the mediator, pipeline behaviors, validators, localization, removing the sample feature |
| [Infrastructure](MeydanCleanApi.Template.Infrastructure/README.md) | Authentication, tokens, refresh cookies, file storage providers |
| [Persistence](MeydanCleanApi.Template.Persistence/README.md) | DbContext, EF Core mappings, repositories, transactions, seeders |
| [WebApi](MeydanCleanApi.Template.WebApi/README.md) | Controllers, middleware order, authorization policies, startup behaviour, configuration and secrets |
| [Tests](MeydanCleanApi.Template.Tests/README.md) | What the unit tests cover, what they deliberately do not, and why |
| [DOCKER.md](DOCKER.md) | Everything about running this project in containers |

Run the tests with `dotnet test MeydanCleanApi.Template.slnx`. CI runs them on every push.

They are unit tests only: no database, no HTTP host. Anything needing real SQL — audit timestamps,
the soft-delete query filter, repository paging — is still uncovered and wants an integration project
with a PostgreSQL container.

---

## Licence

See [LICENSE.txt](LICENSE.txt).
