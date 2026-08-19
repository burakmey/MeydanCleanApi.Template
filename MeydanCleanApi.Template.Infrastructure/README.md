# Infrastructure Layer

## What this layer is responsible for

Infrastructure implements the interfaces the Application layer declares, for everything that talks to
the outside world: JWT tokens, Google sign-in, file storage providers, localization lookup, the system
clock, and the current user.

**It depends on Application and Domain only.** It must not reference the WebApi layer, and it must not
reference Entity Framework. Anything that needs a `DbContext` belongs in Persistence, which is why
`IUserSessionService` is implemented there instead of here.

---

## Folder breakdown

```text
MeydanCleanApi.Template.Infrastructure/
├── Common/
│   ├── Constants/        JwtClaimTypes: "sub", "email", "name", "role", "jti", "email_verified"
│   ├── Extensions/       JwtClaimExtensions, for reading claims out of a JsonWebToken
│   └── Logging/          LogEvents, the EventId values used in structured logs
├── Options/              Strongly-typed settings. Each folder mirrors its configuration section.
│   ├── Authentication/   GoogleOptions, bound from Authentication:Google
│   ├── Tokens/           JwtOptions, bound from Tokens:Jwt
│   └── Storage/          LocalStorageOptions, SupabaseOptions
├── Services/
│   ├── Auth/
│   │   ├── AuthService.cs                Login, refresh rotation, external account linking
│   │   ├── UserAdminService.cs           Role assignment and lockout
│   │   ├── RefreshTokenCookieService.cs  Delivers the refresh token as an HttpOnly cookie
│   │   ├── Tokens/                       JwtTokenService
│   │   └── Verifiers/                    GoogleAuthVerifier
│   ├── Clock/            DateTimeService
│   ├── FileStorage/
│   │   ├── FileStorageHandler.cs         Routes a call to the right provider
│   │   └── Providers/                    LocalStorageService, SupabaseStorageService
│   ├── Localization/     LocalizationService<T>, SupportedCultureProvider
│   ├── Resolvers/        FileStorageHandlerResolver, ExternalAuthVerifierResolver
│   ├── Tracing/          CorrelationIdContext
│   └── User/             CurrentUserService
├── GlobalUsings.cs
└── ServiceRegistration.cs
```

---

### Options/

Each options record exposes a `SectionName`, so binding never uses a loose string. The folder layout
mirrors those sections, which makes the mapping obvious at a glance:

| Folder | Configuration section |
| :--- | :--- |
| `Options/Authentication/GoogleOptions.cs` | `Authentication:Google` |
| `Options/Tokens/JwtOptions.cs` | `Tokens:Jwt` |
| `Options/Storage/LocalStorageOptions.cs` | `Storage:Local` |
| `Options/Storage/SupabaseOptions.cs` | `Storage:Supabase` |

Settings the app cannot run without are checked once at startup with `ValidateOnStart`.
`JwtOptions.IsValid` requires both keys to be at least 32 characters, plus an issuer, an audience and
positive expiry values. Failing here stops the app from starting, which is deliberate: a short or
empty signing key produces tokens anyone can forge, and that would otherwise stay invisible until
someone noticed forged tokens being accepted.

Optional providers are checked when they are used, not in the constructor:

```csharp
// LocalStorageService: checked when a path is actually resolved
private void EnsureConfigured()
{
    if (string.IsNullOrWhiteSpace(_options.RootDirectory))
        throw ConfigurationMissingException.ForSection(LocalStorageOptions.SectionName, nameof(_options.RootDirectory));
}
```

Every provider and verifier is constructed when its resolver is built. If one threw because it was
unconfigured, it would take the whole feature down with it, and an unconfigured Google client would
break plain email and password login.

### Services/Auth/ — the three sign-in flows

All three return a `TokenModel`. The access token goes back in the response body; the refresh token
goes to the client through `IRefreshTokenDeliveryService` and is stored only as a hash.

**Login with email and password**

```csharp
var user = await _userManager.FindByEmailAsync(email);          // 1. find the account
if (user is null) throw InvalidCredentialsException.WithCode(); //    same error for every failure, so
                                                                //    nobody can probe which emails exist
if (await _userManager.IsLockedOutAsync(user)) throw InvalidCredentialsException.WithCode();

if (!await _userManager.CheckPasswordAsync(user, password))     // 2. verify
{
    await _userManager.AccessFailedAsync(user);                 //    count it, so guessing locks out
    throw InvalidCredentialsException.WithCode();
}

await _userManager.ResetAccessFailedCountAsync(user);           // 3. success clears the counter
return await IssueTokensAsync(user, AuthProviderType.Local, ct);// 4. issue and store the session
```

**Refresh, which rotates the token**

```csharp
var tokenHash = _tokenService.HashRefreshToken(refreshToken);              // 1. hash what was presented
var userId = await _userSessionService.FindUserIdByRefreshTokenHashAsync(tokenHash, ct);
if (userId is null)                                                       // 2. unknown, already used
    throw UnauthorizedException.WithCode(ErrorCodes.SessionExpired);      //    or expired

var user = await _userManager.FindByIdAsync(userId.Value.ToString());
return await IssueTokensAsync(user, (AuthProviderType)user.ActiveAuthProviderId, ct);
// 3. Issuing overwrites the stored hash, so the old token stops working after a single use.
```

**External sign-in with Google**

```csharp
var verifier = _verifierResolver.Resolve(authProvider);           // 1. Google verifier, or 409
var externalUser = await verifier.VerifyIdTokenAsync(idToken, ct);// 2. checks listed below

if (!externalUser.EmailVerified)                                  // 3. refuse unverified emails, or
    throw ConflictException.WithCode(ErrorCodes.ExternalAuthMissingClaims);
                                                                  //    somebody could claim an email
                                                                  //    they do not own and take over
                                                                  //    the matching local account
var user = await ResolveExternalUserAsync(externalUser, authProvider, ct); // 4. link or create
return await IssueTokensAsync(user, authProvider, ct);
```

`ResolveExternalUserAsync` matches on the provider's subject id, not the email, because a user can
change the email on their Google account:

1. Look for a `UserAuthProvider` row with this provider and subject. Found means that account.
2. Otherwise find a local account with the same email and link it.
3. Otherwise create an account with no password, so it can only sign in through the provider, and give
   it the `Customer` role.

### Services/Auth/Verifiers/ — what GoogleAuthVerifier checks

```text
idToken ──► fetch Google's OpenID metadata (cached by ConfigurationManager)
        ──► signature against Google's published RSA keys
        ──► issuer  == Authentication:Google:Issuer
        ──► audience == Authentication:Google:ClientId   stops a token minted for another app
        ──► not expired (2 minute clock skew)
        ──► required claims present: sub, email, name
        ──► returns ExternalUserModel
```

### Services/Auth/Tokens/

`JwtTokenService` signs access tokens carrying `sub`, `email`, `jti` and `role`. Those short claim
names are deliberate: `MapInboundClaims` is off in the JWT bearer setup, so `RoleClaimType` must be
`"role"` too. Pointing it at the long `ClaimTypes.Role` URI would mean `[Authorize(Roles = ...)]`
never matches anything.

```csharp
var tokenModel = _tokenService.CreateTokens(user.Id, user.Email, roles);

// The refresh token is 64 random bytes. Only its hash is stored, and the plain value goes to the
// client in an HttpOnly cookie, never in the response body.
var storedHash = _tokenService.HashRefreshToken(tokenModel.RefreshToken);
```

`HashRefreshToken` uses a keyed hash with `RefreshSecurityKey`, separate from the signing key, so a
leaked database still cannot be turned back into working tokens.

### Services/Auth/RefreshTokenCookieService.cs

Handlers never touch cookies. They call the abstraction, and this implementation writes the cookie:

```csharp
_refreshTokenDelivery.Issue(tokenModel.RefreshToken, tokenModel.RefreshTokenExpiration);  // login, refresh
var presented = _refreshTokenDelivery.Read();                                             // refresh
_refreshTokenDelivery.Revoke();                                                           // logout
```

Each flag earns its place:

```csharp
HttpOnly = true,                  // page scripts cannot read it, so XSS cannot steal it
Secure   = true,                  // HTTPS only
SameSite = SameSiteMode.Strict,   // not attached to requests started by another site, blocking CSRF
Path     = "/api/v1/auth"         // not sent with every API call
```

Swap this implementation for a header-based one when native mobile clients cannot hold
cookies. No handler changes are needed.

### Services/Resolvers/

Both resolvers index their implementations once by the enum value they handle, so a lookup is a
dictionary hit rather than a scan.

```csharp
var presignedUrl = await _storageHandler.CreatePresignedUploadUrlAsync(
    FileStorageType.Supabase, "public/products/sample.jpg", ct);
```

Adding a provider means writing one class and registering it. No existing code changes, and callers
never decide what an unsupported provider should mean.

### Services/Clock/

`DateTimeService` returns the real clock. Take the current time from `IDateTimeService` instead of
calling `DateTime.UtcNow` directly, so tests can swap in a fake clock and assert on things like token
expiry without waiting.

### Services/Tracing/

One correlation id per request, so every log line from that request can be found together. It is
scoped, seeded with a new value and overwritten by the inbound header when one is sent.

`CorrelationIdMiddleware` in the WebApi layer sets it. Anything that wants to tie its output to the
request reads it:

```csharp
// LoggingBehavior, already wired for every CQRS request
_logger.LogInformation("Processing {RequestName} [CorrelationId: {CorrelationId}]",
    requestName, _correlationIdContext.CorrelationId);

// An outbound client: forwarding the header follows one user action across two services
public sealed class InvoiceApiClient(HttpClient http, ICorrelationIdContext correlation)
{
    public async Task SendAsync(Invoice invoice, CancellationToken ct)
    {
        http.DefaultRequestHeaders.Add("X-Correlation-ID", correlation.CorrelationId);
        await http.PostAsJsonAsync("/invoices", invoice, ct);
    }
}
```

A client reports a failure and quotes the `X-Correlation-ID` from the response headers. Searching logs
for that value returns only that request:

```text
[10:31:02 INF] Processing CQRS Request DeleteFileCommand [CorrelationId: 9f2c... ]
[10:31:03 WRN] Slow Request Detected: DeleteFileCommand (812 ms) [CorrelationId: 9f2c... ]
[10:31:03 ERR] Unhandled System Crash at DELETE /api/v1/files/... [CorrelationId: 9f2c... ]
```

### Services/User/

`CurrentUserService` reads the signed-in identity from `IHttpContextAccessor`. It uses the short claim
names the token is written with, and falls back to the long `ClaimTypes.*` URIs in case claim mapping
is ever turned back on.

### Services/Localization/ and caching

`SupportedCultureProvider` reads the active rows from the `SupportedCultures` table and keeps the
result in `IMemoryCache` for ten minutes, because cultures are read on nearly every request but change
rarely. It shows the pattern to copy: read often, changes rarely, cheap to rebuild.

```csharp
private const string CacheKey = "supported-cultures:active";
private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

public async Task<IReadOnlyList<string>> GetActiveCodesAsync(CancellationToken ct = default)
{
    if (_cache.TryGetValue(CacheKey, out IReadOnlyList<string>? cached) && cached is not null)
        return cached;                                    // hit: no database round trip

    var cultures = await _cultureReadRepository.GetAllAsync(ct);
    IReadOnlyList<string> codes = [.. cultures.Select(c => c.CultureCode)];

    _cache.Set(CacheKey, codes, CacheDuration);           // miss: read once, reuse for 10 minutes
    return codes;
}

public void RefreshCache() => _cache.Remove(CacheKey);    // call after adding or disabling a culture
```

Good candidates to cache next are the lookup tables (`FileStatuses`, `FileStorages`, `AuthProviders`,
`FilePurposes`), which are read constantly and change only when new reference data is deployed.

Do not cache anything user-specific without putting the user id in the key. A key that mixes two
users' data is a data leak, not a performance win:

```csharp
var cacheKey = $"user-permissions:{userId}";
var cacheKey = $"product-list:{cultureCode}:{pageNumber}";
```

`IMemoryCache` lives in one process. Run two instances and each keeps its own copy, so `RefreshCache()`
on one does not clear the other. Move to `IDistributedCache` with Redis once more than one
instance runs and stale data would matter.

`LocalizationService<T>` resolves `.resx` entries through `IStringLocalizer<T>`. The resource files
live in the **Application** project next to the marker classes they are named after, and
`AddLocalization` is registered in the WebApi layer.

---

## Configuration

Non-secret values live in `appsettings.json`. Secrets do not. Set them with `dotnet user-secrets` in
development and environment variables in production, using `__` for nesting
(`Tokens__Jwt__JwtSecurityKey`). The keys below ship empty on purpose, and startup fails with a clear
message when a required one is still empty or still holds a placeholder.

```json
{
  "Tokens": {
    "Jwt": {
      "JwtSecurityKey": "",
      "RefreshSecurityKey": "",
      "Issuer": "MeydanCleanApi.Template",
      "Audience": "MeydanCleanApi.Template.Clients",
      "AccessTokenExpiryMinutes": 15,
      "RefreshTokenExpirationDays": 7
    }
  },
  "Authentication": {
    "Google": {
      "ClientId": "",
      "Issuer": "https://accounts.google.com",
      "MetaData": "https://accounts.google.com/.well-known/openid-configuration"
    }
  },
  "Storage": {
    "Local": {
      "RootDirectory": "App_Data/uploads",
      "BaseUrl": "/api/v1/files"
    },
    "Supabase": {
      "Url": "",
      "ApiKey": "",
      "Bucket": "uploads",
      "DownloadUrlExpiryMinute": 60
    }
  }
}
```

Three settings are easy to get wrong:

- `AccessTokenExpiryMinutes` is short (15) on purpose. Logging out deletes the stored refresh token
  immediately, but an access token already handed out stays valid until it expires. A small window
  limits that gap without a database read on every request.
- `Storage:Local:RootDirectory` must stay outside `wwwroot`. Anything under `wwwroot` is served by
  `UseStaticFiles` with no authorization check, which would publish every private upload.
- Google settings are optional. Email and password login works without them.

---

## Rules worth knowing before extending this layer

**Refresh tokens never appear in a response body.** They travel in an HttpOnly, Secure,
SameSite=Strict cookie scoped to `/api/v1/auth`. Only a hash is stored, and each refresh replaces the
token, so a stolen copy stops working the moment it is used once.

**One active session per user.** Signing in on a second device ends the first. Supporting several
devices at once means moving the session columns off `AppUser` into a table of their own.

**Uploads are never stored under `wwwroot`.** Anything under `wwwroot` is served by `UseStaticFiles`
with no authorization check, which would publish every private upload. Local files go to
`Storage:Local:RootDirectory`, outside the served folder.

**File paths never come from the client.** The container is checked against a whitelist and the
stored name is generated, so a crafted file name cannot escape the storage folder.

**A file belongs to nobody until it is attached.** Permission is decided by the entity it hangs off,
which is why `FileEntity` carries no owner column.

---

## Service registration

```csharp
builder.Services.AddInfrastructureServices(builder.Configuration);
```

Three lifetime decisions matter when extending this layer:

- **The storage providers and their resolver are all `Singleton`.** `FileStorageHandlerResolver` reads
  every `IFileStorageService` into a dictionary once. Registering the providers as `Scoped` while the
  resolver stays `Singleton` is a captive dependency, and the application refuses to start in
  Development where the DI container validates scopes. Keep these lifetimes in step when adding a
  provider.
- **`ISupportedCultureProvider` is `Scoped`**, because it reads the database through the scoped
  `DbContext`. The cache in front of it is what keeps that cheap.
- **`IUserSessionService` is registered by Persistence, not here**, because storing sessions needs the
  `DbContext`.
