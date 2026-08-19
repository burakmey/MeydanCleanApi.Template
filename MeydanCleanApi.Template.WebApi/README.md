# WebApi Layer

## What this layer is responsible for

WebApi is the entry point. It wires up dependency injection, exposes the controllers, configures
authentication and authorization, and orders the HTTP middleware pipeline.

**It depends on Application, Infrastructure and Persistence.** Controllers hold no business logic:
each action binds a request, sends it through the mediator, and returns the result.

---

## Folder breakdown

```text
MeydanCleanApi.Template.WebApi/
├── Configurations/
│   ├── Authorization/    AuthorizationExtensions, the policies and the fallback rule
│   ├── HealthChecks/     HealthCheckExtensions, the three /health endpoints
│   ├── JwtBearer/        JwtBearerOptionsSetup, token validation and 401/403 responses
│   ├── Localization/     LocalizationExtensions, request culture selection
│   ├── StartupChecks/    StartupCheckExtensions, database reachability and migrations
│   └── Swagger/          SwaggerServiceExtensions
├── Controllers/
│   ├── Auth/             AuthController
│   ├── Base/             ApiControllerBase
│   ├── Cultures/         CulturesController
│   ├── Files/            FilesController
│   └── Samples/          SampleProductsController, AttachSampleProductFileRequest
├── Middlewares/
│   ├── CorrelationIdMiddleware.cs
│   └── GlobalExceptionHandler.cs
├── Properties/           launchSettings.json
├── wwwroot/              Public static files. Uploads are deliberately NOT here.
├── appsettings.json                Every key the API reads, secrets left empty
├── appsettings.Development.json    Non-secret local overrides
├── GlobalUsings.cs
├── Program.cs
└── ServiceRegistration.cs
```

---

### Middlewares/

Middleware forms an ordered chain. A request passes through each one in turn on the way in, and the
response travels back through them in reverse. Order therefore decides what can see what.

**`CorrelationIdMiddleware`** runs first so everything after it can be traced. It reads the
`X-Correlation-ID` header or generates one, caps it at 128 characters to prevent log injection, pushes
it into the Serilog context so every log line carries it, and writes it to the response headers before
the pipeline continues, so even failed requests return the trace id.

**`GlobalExceptionHandler`** catches whatever the pipeline throws and turns it into one JSON shape.
Domain exceptions carry their own status code and error code, so they become the right HTTP response
and are logged at `Information` level. Anything unexpected is logged at `Error` with the full stack
trace and returned as a plain 500, so internal detail never leaks.

```json
{
  "status": 400,
  "errorCode": "ERR_VALIDATION",
  "message": "One or more fields are invalid.",
  "errors": { "Email": [ "Please enter a valid email address." ] }
}
```

Validation codes are translated here through `ILocalizationService<ValidationMessages>`, so the client
receives readable text in the request's language while the codes stay stable.

### Configurations/Authorization/

`FallbackPolicy` requires a signed-in user on every endpoint unless it opts out with
`[AllowAnonymous]`. Without it, forgetting `[Authorize]` on a new controller silently publishes it.
Remember to mark genuinely public endpoints, such as login and the health probes, as anonymous.

Three policies are registered: `SuperAdmin`, `Admin` (which includes SuperAdmin, so the highest
privilege satisfies ordinary admin endpoints), and `Customer`.

### Configurations/JwtBearer/

`JwtBearerOptionsSetup` configures token validation. The settings that matter most:

- `MapInboundClaims = false` keeps the short claim names the token was written with: `sub`, `jti`,
  `email`, `role`.
- `NameClaimType` and `RoleClaimType` therefore also use the short names. This has to agree with the
  setting above. Pointing `RoleClaimType` at the long `ClaimTypes.Role` URI would mean
  `[Authorize(Roles = ...)]` and every policy never match, returning 403 to users who are in fact
  authorized.
- `ClockSkew = TimeSpan.Zero` enforces exact expiry with no grace period.
- `OnChallenge` and `OnForbidden` write the same `ErrorResponse` JSON as everything else, so every
  failure looks alike to a client.

`OnTokenValidated` only checks that `sub` and `jti` are present and well-formed. It does not check
whether the session was logged out, because that would mean a database read on every request. Instead
access tokens are short lived and logout deletes the refresh token.

### Configurations/StartupChecks/

Runs before the API accepts traffic. It verifies the database is reachable and **throws** when it is
not, because an API that starts without a database reports itself healthy and then fails every
request, which is harder to diagnose than a crash at boot. It then applies pending migrations, and
when no migrations exist at all it logs the exact command to create the first one.

### Configurations/Localization/

Reads the active culture codes from `ISupportedCultureProvider` at startup, validates each with
`CultureInfo.GetCultureInfo`, and falls back to `tr-TR` and `en-US` when the table is empty or
unreachable. Culture is then chosen per request from the query string, a cookie, or the
`Accept-Language` header.

### Configurations/Swagger/

Registers the Swagger generator, the JWT authorize button, and `IncludeXmlComments`, so the
documentation written in code reaches the Swagger page. Swagger only runs in Development.

### Controllers/

Every controller derives from `ApiControllerBase`, which supplies `[ApiController]`, the
`api/v1/[controller]` route, and a lazily resolved `Mediator`.

Actions return `BaseResponse<T>` from the handler directly. There is deliberately no helper that wraps
results in an anonymous object: two response shapes in one API mean clients cannot write a single
deserializer, and anonymous objects give Swagger nothing to document.

| Controller | Endpoints | Access |
| :--- | :--- | :--- |
| `AuthController` | `POST /auth/login`, `/refresh`, `/logout`, `/external-login` | Anonymous except logout; whole controller is rate limited |
| `CulturesController` | `GET /cultures` | Anonymous, since clients need it before signing in |
| `FilesController` | `POST /files/upload-url`, `PUT /files/{id}/confirm` | Authenticated |
| `FilesController` | `GET /files`, `DELETE /files/{id}` | Admin policy, for clearing unattached uploads |
| `SampleProductsController` | `GET /sampleproducts`, `POST /sampleproducts`, `DELETE /{id}` | Read anonymous; create and soft delete require Admin |
| `SampleProductsController` | `POST /{id}/files`, `DELETE /{id}/files/{fileId}` | Admin policy, since a file is reached through its owner |

Health checks are not controllers. `MapApplicationHealthChecks` in
`Configurations/HealthChecks/` maps three endpoints:

| Endpoint | Runs | Access | Used by |
| :--- | :--- | :--- | :--- |
| `/health` | no checks at all | anonymous | Docker, load balancers |
| `/health/ready` | checks tagged `ready`, so the database | anonymous | Kubernetes readiness, compose `depends_on` |
| `/health/details` | every check, with timings and failure reasons | `Admin` policy | operators |

The first two are explicitly anonymous, because the authorization fallback policy would otherwise
demand a token that no probe carries. They answer only Healthy or Unhealthy, so an anonymous caller
learns nothing about the internals.

`/health/details` is the opposite: it returns component names, durations and error messages, which
help an operator and equally help an attacker. That is why it needs the Admin policy, and why it
returns exception messages but never stack traces.

Liveness deliberately runs no checks. Pointing a load balancer at a database-backed endpoint means
one slow query can remove a healthy instance from rotation.

`AuthController` never touches cookies. The handlers call `IRefreshTokenDeliveryService`, which writes
an HttpOnly, Secure, SameSite=Strict cookie scoped to `/api/v1/auth`. The refresh token is absent from
every response body, because returning it there would undo the point of `HttpOnly`.

---

## Program.cs

```csharp
// STEP 1: Serilog
var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((context, loggerConfig) => { ... });

// STEP 2: Layer registrations
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddPersistenceServices(builder.Configuration);
builder.Services.AddWebApiServices(builder.Configuration);

// STEP 3: Build, check the database, seed
var app = builder.Build();
await app.PerformStartupChecksAsync();     // throws if unreachable; applies migrations
await app.Services.SeedDatabaseAsync();    // safe to repeat

// STEP 4: Middleware pipeline, order matters
app.UseForwardedHeaders(...);                       // 1. real client IP and scheme behind a proxy
app.UseCorrelationId();                             // 2. trace id for every log line
await app.UseApplicationRequestLocalizationAsync(); // 3. request culture
app.UseSerilogRequestLogging();                     // 4. method, path, status, duration
app.UseExceptionHandler();                          // 5. converts exceptions to JSON
app.UseHsts(); app.UseHttpsRedirection();           // 6. HTTPS, HSTS outside Development
app.UseStaticFiles();                               // 7. public wwwroot assets, uploads are elsewhere
if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(...); }  // 8.
app.UseRouting();                                   // 9. match the route
app.UseCors(CorsPolicyName);                        // 10. allowed origins from CorsOrigins
app.UseRateLimiter();                               // 11. throttle before authentication
app.UseAuthentication();                            // 12. identify the caller
app.UseAuthorization();                             // 13. check permission
app.MapControllers();                               // 14. endpoints
app.MapApplicationHealthChecks();                   // /health, /health/ready, /health/details

// STEP 5
await app.RunAsync();
```

Two ordering decisions are worth keeping:

- `UseForwardedHeaders` runs first because rate limiting partitions clients by IP address. Behind a
  proxy every request would otherwise look like it came from the proxy, so one noisy client could
  throttle everybody.
- `UseRateLimiter` runs before authentication, because the main thing worth throttling is password
  guessing against `/login`, which is unauthenticated by definition.

`Program` is a normal class with a `Main` method rather than top-level statements, so the startup
sequence reads in order and can be stepped through in a debugger.

---

## How startup behaves

Startup fails fast rather than running in a broken state:

- A missing connection string, signing key, issuer or audience stops the app with a message naming
  the setting. Placeholder values are rejected too, so a forgotten override cannot ship.
- An unreachable database stops startup instead of reporting healthy and then failing every request.
- When no migrations exist yet, the log carries a warning with the exact command to create the first
  one.

Two defaults are worth knowing before adding endpoints:

- **Endpoints require authentication unless they opt out.** An authorization fallback policy is
  registered, so a new controller is protected even without `[Authorize]`. Genuinely public endpoints
  need `[AllowAnonymous]`, which is why login and the health probes carry it.
- **Unknown paths answer 401, not 404.** That is the same fallback policy at work. It keeps anonymous
  callers from discovering which routes exist.

---

## Service registration

```csharp
builder.Services.AddWebApiServices(builder.Configuration);
```

Registers controllers, the exception handler, `.resx` localization, health checks including a database
check, JWT bearer authentication, the authorization policies, CORS, rate limiting and Swagger.

CORS reads its allowed origins from the `CorsOrigins` array in configuration. Browser clients that
send the refresh token cookie need `AllowCredentials`, and ASP.NET Core forbids combining that with
`AllowAnyOrigin`, so origins have to be listed explicitly. With none configured the policy allows
nothing, which fails closed rather than open.

Rate limiting applies a global limit of 100 requests per minute per client, plus a stricter policy of
10 per minute on the auth endpoints to slow down password guessing.

---

## Configuration & Secrets

Two different things live in configuration, and they are kept apart on purpose.

**Shape** — which keys exist, and defaults that are safe to read. Committed to git.

**Secret values** — never committed, in any environment.

### The three files

| File | Loaded by the app | In git | In the Docker image | Purpose |
| :--- | :--- | :--- | :--- | :--- |
| `appsettings.json` | always | yes | yes | The full schema. Every key the solution reads, with secrets as `""` |
| `appsettings.Development.json` | only when `ASPNETCORE_ENVIRONMENT=Development` | yes | **no** | Non-secret local overrides so the project runs straight after clone |
| `appsettings.Production.json` | when environment is Production | **no, gitignored** | no | Only for hosts that cannot supply environment variables |

JSON has no comment syntax, so none of these files carry explanatory keys. Everything they used to
say is written here instead.

### Precedence, lowest to highest

```text
appsettings.json
  → appsettings.{Environment}.json
    → user-secrets            (Development only, never in Docker or Production)
      → environment variables (always wins)
```

The last source to define a key wins. So an environment variable overrides a user-secret, which
overrides `appsettings.Development.json`, which overrides `appsettings.json`.

### Every configuration section

`Options record` names the strongly-typed class the section binds to. Sections marked "read directly"
have no record; they are read from `IConfiguration` at the call site.

| Configuration key | Options record | Environment variable |
| :--- | :--- | :--- |
| `ConnectionStrings:DefaultConnection` | read directly, `GetConnectionString` | `ConnectionStrings__DefaultConnection` |
| `Tokens:Jwt:JwtSecurityKey` | `JwtOptions` | `Tokens__Jwt__JwtSecurityKey` |
| `Tokens:Jwt:RefreshSecurityKey` | `JwtOptions` | `Tokens__Jwt__RefreshSecurityKey` |
| `Tokens:Jwt:Issuer` | `JwtOptions` | `Tokens__Jwt__Issuer` |
| `Tokens:Jwt:Audience` | `JwtOptions` | `Tokens__Jwt__Audience` |
| `Tokens:Jwt:AccessTokenExpiryMinutes` | `JwtOptions` | `Tokens__Jwt__AccessTokenExpiryMinutes` |
| `Tokens:Jwt:RefreshTokenExpirationDays` | `JwtOptions` | `Tokens__Jwt__RefreshTokenExpirationDays` |
| `Authentication:Google:ClientId` | `GoogleOptions` | `Authentication__Google__ClientId` |
| `Authentication:Google:Issuer` | `GoogleOptions` | `Authentication__Google__Issuer` |
| `Authentication:Google:MetaData` | `GoogleOptions` | `Authentication__Google__MetaData` |
| `Storage:Local:RootDirectory` | `LocalStorageOptions` | `Storage__Local__RootDirectory` |
| `Storage:Local:BaseUrl` | `LocalStorageOptions` | `Storage__Local__BaseUrl` |
| `Storage:Supabase:Url` | `SupabaseOptions` | `Storage__Supabase__Url` |
| `Storage:Supabase:ApiKey` | `SupabaseOptions` | `Storage__Supabase__ApiKey` |
| `Storage:Supabase:Bucket` | `SupabaseOptions` | `Storage__Supabase__Bucket` |
| `Storage:Supabase:DownloadUrlExpiryMinute` | `SupabaseOptions` | `Storage__Supabase__DownloadUrlExpiryMinute` |
| `CorsOrigins` (array) | read directly, `GetSection(...).Get<string[]>()` | `CorsOrigins__0`, `CorsOrigins__1`, … |
| `SeedData:SuperAdminEmail` | read directly, in `AdminUserSeederService` | `SeedData__SuperAdminEmail` |
| `SeedData:SuperAdminPassword` | read directly, in `AdminUserSeederService` | `SeedData__SuperAdminPassword` |
| `Logging`, `Serilog`, `AllowedHosts` | framework sections | `Logging__LogLevel__Default`, … |

The rule for the environment variable column is mechanical: replace every `:` with `__`. A
configuration array is indexed by position, which is why `CorsOrigins` needs one numbered key per
origin.

The four options records live in `Infrastructure/Options/`, each exposing a `SectionName` so binding
never uses a loose string. Values required for the app to run are validated once at startup with
`ValidateOnStart`, so a missing signing key stops the app immediately rather than producing tokens
nothing can verify.

### Which values are secret

| Key | Secret? | Notes |
| :--- | :--- | :--- |
| `ConnectionStrings:DefaultConnection` | **yes** | contains the database password |
| `Tokens:Jwt:JwtSecurityKey` | **yes** | 32 characters minimum. A short key can be forged |
| `Tokens:Jwt:RefreshSecurityKey` | **yes** | 32 characters minimum, and different from the one above |
| `Storage:Supabase:ApiKey` | **yes** | service key, grants full bucket access |
| `SeedData:SuperAdminPassword` | **yes** | only read on first run, against an empty users table |
| `Authentication:Google:ClientId` | no | public by design, but environment specific |
| everything else | no | safe to commit |

`appsettings.Development.json` does hold two JWT keys. They are throwaway strings that exist so the
project runs immediately after clone, they are marked as development-only, and they must never be
reused anywhere else. Every other secret is absent from git entirely.

### Supplying secrets locally

`dotnet user-secrets` stores values in the Windows user profile, outside the repository, so nothing
typed here can be committed. Run from the `*.WebApi` folder:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=meydanclean_db;Username=postgres;Password=YOUR_PASSWORD"
```

```bash
dotnet user-secrets set "Tokens:Jwt:JwtSecurityKey" "replace-with-32-or-more-random-characters"
```

```bash
dotnet user-secrets set "Tokens:Jwt:RefreshSecurityKey" "replace-with-a-different-32-plus-character-string"
```

Optional, to get an administrator account on first run:

```bash
dotnet user-secrets set "SeedData:SuperAdminEmail" "admin@example.com"
```

```bash
dotnet user-secrets set "SeedData:SuperAdminPassword" "YOUR_STRONG_PASSWORD"
```

List what is stored:

```bash
dotnet user-secrets list
```

User-secrets load **only in Development**. They are never read in Docker or Production, which is
deliberate: the same file cannot leak into a deployed image.

### Supplying secrets in Docker

`.dockerignore` keeps `appsettings.Development.json` and `.env` out of the image, so the container
holds `appsettings.json` and nothing else. Every real value arrives as an environment variable, which
is exactly how a deployed server behaves — a missing setting therefore fails locally first.

`.env` in the repository root holds them, and `docker-compose.yml` maps each one to the container
name from the table above. `.env` is gitignored; `.env.example` is the committed template.

```bash
cp .env.example .env
```

Generate real keys with:

```bash
openssl rand -base64 48
```

### Supplying secrets in Production

Use the platform's environment variables or secret manager, with the same `__` names from the table
above. No configuration file is needed, and this is the recommended route on every modern host:
Azure App Service, AWS, Railway, Kubernetes and Docker all set environment variables natively.

Beyond the secrets, four values are usually worth overriding in production. They are listed in
`appsettings.json` with development-friendly defaults:

| Key | Default | Production value |
| :--- | :--- | :--- |
| `AllowedHosts` | `*` | the real host names, for example `yourdomain.com;admin.yourdomain.com` |
| `CorsOrigins` | `[]` | the real front end origins, for example `https://yourdomain.com` |
| `Logging:LogLevel:Default` | `Information` | `Warning`, so the log holds problems rather than traffic |
| `Serilog:WriteTo` file path | `Logs/log-.txt` | a path on a mounted volume, or a log sink instead of a file |

As environment variables those are `AllowedHosts`, `CorsOrigins__0`, `Logging__LogLevel__Default`
and so on.

**If a host genuinely cannot set environment variables**, create `appsettings.Production.json` on
that server by copying `appsettings.json` and filling in the real values. `.gitignore` already blocks
that filename, and `.dockerignore` keeps it out of any image, so it can never travel with the code.
This template ships no `.example` file for it: `appsettings.json` is the schema, and the tables above
are the documentation.
