# Application Layer

## What this layer is responsible for

The Application layer holds the use cases. Every action the API can perform is a command or a query
here, with its own handler. It also declares the interfaces for everything it needs from the outside
world: database access, file storage, tokens, localization.

**It depends only on Domain.** No Entity Framework, no ASP.NET Core, no `HttpContext`, no
`IConfiguration`. When a handler needs the database it asks for `IReadRepository<T, TKey>`, and the
Persistence layer supplies the implementation at startup. That is what keeps the business rules
testable and free of infrastructure detail.

---

## Folder breakdown

```text
MeydanCleanApi.Template.Application/
├── Abstractions/         Interfaces only, no implementations
│   ├── Auth/             IAuthService, ICurrentUserService, ITokenService, IUserSessionService,
│   │                     IUserAdminService, IExternalAuthVerifier, IExternalAuthVerifierResolver,
│   │                     IRefreshTokenDeliveryService
│   ├── Clock/            IDateTimeService
│   ├── Localization/     ILocalizationService<T>, ISupportedCultureProvider
│   ├── Messaging/        IMediator, IRequest<T>, IRequestHandler<T,R>, IPipelineBehavior<T,R>
│   ├── Options/          IOptionSection
│   ├── Repositories/     IReadRepository<T,TKey>, IWriteRepository<T,TKey>, IUnitOfWork
│   ├── Storage/          IFileStorageCoordinator, IFileStorageHandler,
│   │                     IFileStorageHandlerResolver, IFileStorageService
│   └── Tracing/          ICorrelationIdContext
├── Behaviors/            LoggingBehavior, PerformanceBehavior, ValidationBehavior
├── Common/
│   ├── Constants/        PaginationConstants
│   ├── Extensions/       SampleProductMappingExtensions
│   ├── Models/
│   │   ├── (root)        BaseResponse<T>
│   │   ├── Auth/         ExternalUserModel
│   │   ├── FileStorage/  FileDto, FileUploadModel
│   │   ├── Pagination/   PagedRequest, PagedResponse<T>
│   │   └── Token/        TokenModel, AccessTokenModel
│   └── Security/         CurrentUserExtensions
├── Constants/
│   └── FileStorage/      FileStorageContainers
├── DTOs/
│   └── Samples/          SampleProductDto, SampleProductFileDto
├── Features/             One folder per use case
│   ├── Auth/             Login, RefreshToken, Logout, ExternalLogin
│   ├── Cultures/         GetSupportedCultures
│   ├── Files/            CreatePendingFile, ConfirmFileUpload, DeleteFile, GetFilesPaged
│   └── Samples/          CreateSampleProduct, SoftDeleteSampleProduct, AttachSampleProductFile,
│                         DetachSampleProductFile, GetSampleProductsPaged
├── Localization/         Marker classes with their .resx translations beside them
├── Services/
│   ├── FileStorage/      FileStorageCoordinator
│   └── Messaging/        Mediator
├── GlobalUsings.cs
└── ServiceRegistration.cs
```

---

### Abstractions/

Interfaces only. Concrete classes live in `Services/` here, or in Infrastructure and Persistence.
`IUserSessionService` is declared here but implemented in **Persistence**, because storing sessions
needs the `DbContext` and Infrastructure must not reference EF Core.

Two of these deserve a note:

`IRefreshTokenDeliveryService` hands the refresh token to the client and reads it back. Handlers call
it instead of returning the token in the response body, so the secret never appears in JSON. The
shipped implementation uses an HttpOnly cookie.

`IExternalAuthVerifierResolver` returns the verifier for a sign-in provider. It mirrors
`IFileStorageHandlerResolver`, so adding Apple sign-in means writing one verifier and registering it,
with no change to `AuthService`.

### Behaviors/

Every request passes through three behaviors before reaching its handler, in this order:

1. `LoggingBehavior` writes a line when the request starts and finishes, tagged with the correlation ID.
2. `PerformanceBehavior` warns when a request takes longer than 500 ms.
3. `ValidationBehavior` runs the FluentValidation rules registered for that request.

Validation failures become the Domain's own `ValidationException`, which the global exception handler
turns into an HTTP 400 with a field-by-field list. Letting FluentValidation's own exception escape
would produce a 500 with no detail.

### Common/Models/

`BaseResponse<T>` is the envelope every handler returns, so all successful responses share one shape:

```csharp
public class BaseResponse<T>
{
    public bool IsSucceed { get; init; } = true;
    public T? Data { get; init; }
    public string? Message { get; init; }
}
```

`TokenModel` carries the full token pair inside the server. `AccessTokenModel` is the trimmed version
that goes into a response body: the access token and its expiry, and deliberately no refresh token.

### Common/Security/

Being signed in is not the same as being allowed. Any handler that loads a record by id must also
check the record belongs to the caller, or anyone can reach another user's data by guessing ids.

```csharp
var order = await _orderReadRepository.GetByIdOrThrowAsync(request.Id, ct: ct);

// 401 when nobody is signed in, 403 when signed in but not the owner. Admins pass.
_currentUserService.EnsureCanAccess(order.CustomerUserId);
```

For list queries, apply the same rule inside the predicate so other users' rows never leave the
database:

```csharp
var isAdmin = _currentUserService.IsAdmin();
Expression<Func<Order, bool>> filter = order => isAdmin || order.CustomerUserId == userId;
```

Files are the exception. They carry no owner of their own, so permission is decided by the entity they
are attached to. See the upload flow below.

### Features/

Each feature area splits into `Commands/` and `Queries/`, and inside those is one folder per use case
holding the request, its handler, its response and a validator:

```text
Features/Samples/
├── Commands/
│   ├── CreateSampleProduct/        Command, Handler, Response, Validator
│   ├── SoftDeleteSampleProduct/    Command, Handler, Response
│   ├── AttachSampleProductFile/    Command, Handler, Response
│   └── DetachSampleProductFile/    Command, Handler, Response
└── Queries/
    └── GetSampleProductsPaged/     Query, Handler, Response, Validator
```

Every request returns `BaseResponse<TResponse>`, and every response is its own record in its own file.

```csharp
// Dispatching a query. Paging values are flat, so it binds from ?pageNumber=1&pageSize=20
var query = new GetSampleProductsPagedQuery(pageNumber: 1, pageSize: 20, searchTerm: "phone");
BaseResponse<GetSampleProductsPagedQueryResponse> response = await Mediator.Send(query, ct);
```

### Services/Messaging/

`Mediator` is a small in-process dispatcher built on `IServiceProvider`, so the template carries no
MediatR licence obligation. It finds the handler registered for a request type, wraps it in the
pipeline behaviors, and runs the chain.

### Services/FileStorage/ and the upload flow

File bytes never pass through the API. The client asks for a signed URL and uploads straight to
storage, which keeps large uploads off the web server.

```text
[ Client ]                          [ API ]                        [ Storage ]
    │── 1. POST /files/upload-url ────►│                                 │
    │                                  │── 2. save Pending FileEntity    │
    │                                  │── 3. ask for signed URL ───────►│
    │◄─ 4. { fileId, uploadUrl } ──────│◄─────────────────────────────────│
    │── 5. PUT bytes straight to storage ────────────────────────────────►│
    │── 6. PUT /files/{id}/confirm ───►│── 7. does the object exist? ────►│
    │                                  │   yes, status becomes Uploaded   │
    │── 8. POST /sampleproducts/{id}/files   permission is checked HERE   │
```

Steps worth understanding:

- **Validation runs first.** `CreatePendingFileCommandValidator` checks the size ceiling, the
  extension against an allow list, and the container against `FileStorageContainers.All`. The
  container comes from the client, so an unchecked value such as `../../` would let a caller write
  outside the storage folder.
- **The path is generated, never taken from the client.** The stored name is a new id plus the
  extension, so a crafted file name cannot influence where the object lands.
- **The record is saved before the URL is issued**, so a failed save never hands out a usable URL.
- **Status does not change on its own.** The confirm call asks the provider whether the object really
  arrived. Records left in `Pending` are abandoned uploads a background job can sweep up.
- **Permission belongs to the owner, not the file.** A `FileEntity` on its own belongs to nobody, which
  is why it has no user column. Attaching is where the check happens.

Removing a file goes through the owner too, so the link, the metadata row and the stored object are
removed together:

```text
DELETE /api/v1/sampleproducts/{id}/files/{fileId}
```

Four services share the work, each with one job:

| Service | Layer | Its one job |
| :--- | :--- | :--- |
| `IFileStorageCoordinator` | Application | Validate the container, build the path, stage the row, ask for URLs. Knows no cloud SDK. |
| `IFileStorageHandler` | Infrastructure | Take a `FileStorageType` and forward the call. A router, no logic. |
| `IFileStorageHandlerResolver` | Infrastructure | Return the provider registered for that type. |
| `IFileStorageService` | Infrastructure | Talk to one backend: local disk or Supabase. |

Handlers only ever touch the coordinator. Injecting `IFileStorageService` into a handler means the
layering has slipped. Adding AWS S3 is one new `IFileStorageService` plus one DI line, and nothing
above it changes.

### Abstractions/Repositories/ and transactions

`IUnitOfWork` has two members, and choosing between them is short:

| Goal | Use |
| :--- | :--- |
| Any number of changes saved in one go | `SaveChangesAsync`, already atomic on its own |
| Several saves, or a save plus an outside effect | `ExecuteInTransactionAsync` |

```csharp
await _unitOfWork.ExecuteInTransactionAsync(async token =>
{
    _fileWriteRepository.HardDelete(fileEntity);
    await _unitOfWork.SaveChangesAsync(token);

    await _coordinator.DeleteFromStorageAsync(fileEntity, token);   // irreversible, so it goes last
}, ct);
```

There is no separate Begin/Commit/Rollback trio, so a caller cannot forget to roll back. Worked
scenarios, including the cases where a transaction is the wrong answer, are in the
[Persistence README](../MeydanCleanApi.Template.Persistence/README.md).

Reading related data is opt-in, and every read method takes the same `includes` argument. If a
response DTO reads a collection that was never included, the API silently returns an empty list:

```csharp
var paged = await _productReadRepository.GetPagedAsync(
    request.Page,
    predicate: filter,
    includes: [product => product.Files],   // without this, Files comes back empty
    ct: ct);

var product = await _productReadRepository.GetByIdOrThrowAsync(
    id,
    includes: [p => p.Files, p => p.Translations],
    ct: ct);
```

Pass the cancellation token by name. `includes` sits before `ct`, so a positional `GetAllAsync(ct)`
no longer compiles.

Paging is always ordered. With no sort supplied the repository falls back to newest first, because
`Skip`/`Take` without an `ORDER BY` has no defined result in PostgreSQL and the same row can appear on
two pages.

### Localization/ and Resources/

The three classes in `Localization/` hold no code. They exist so `ILocalizationService<ErrorMessages>`
can point at one set of translations rather than another.

Each `.resx` sits directly beside the marker class it belongs to, and nests under it in Solution
Explorer:

```text
Localization/
├── ApiMessages.cs              MSG_* success messages
│   ├── ApiMessages.resx        neutral, English
│   └── ApiMessages.tr.resx     Turkish
├── ErrorMessages.cs            ERR_* failures
│   ├── ErrorMessages.resx
│   └── ErrorMessages.tr.resx
└── ValidationMessages.cs       VAL_* field rules
    ├── ValidationMessages.resx
    └── ValidationMessages.tr.resx
```

Two rules make this work, and breaking either one fails silently by returning the raw code instead of
a message:

- **The files must stay in this project.** `IStringLocalizer<T>` resolves resources from the assembly
  that owns the marker type, so moving them to WebApi would leave every lookup unresolved.
- **`AddLocalization()` is registered with no `ResourcesPath`.** That makes the lookup name match the
  marker's full type name, which is what lets the files sit next to the class. Setting a path would
  send it looking in a separate folder tree.

The Turkish files are `.tr.resx`, not `.tr-TR.resx`. A neutral language file matches every Turkish
region, so both `tr-TR` and `tr-CY` resolve to it. The file with no suffix is the final fallback and
holds English.

**To add a language**, copy a file and change the suffix, for example `ErrorMessages.de.resx`. Then add
the culture to the `SupportedCultures` table so request localization will select it.

Handlers use codes, never literal text:

```csharp
var entityName = _localizer.Get(nameof(SampleProduct));            // localized entity name
var message = _localizer.Get(ResponseCodes.Created, entityName);   // "{0} created successfully."
```

`nameof` means renaming the entity in the IDE updates the lookup key too.

---

## Service registration

```csharp
builder.Services.AddApplicationServices();
```

Registers the mediator, the three pipeline behaviors, `FileStorageCoordinator`, every
`IRequestHandler` found by assembly scan, and every FluentValidation validator.

---

## Removing the sample feature

The `SampleProduct` feature exists to demonstrate the full CQRS flow end to end. Once it is no longer
needed, delete these folders and files, then regenerate the migration:

- `Features/Samples` and `DTOs/Samples` in this project
- `Entities/Samples` in Domain
- `Configurations/Samples` in Persistence, plus `SampleProductSeederService`
- `Controllers/Samples` in WebApi
- `Common/Extensions/SampleProductMappingExtensions.cs`

Creating the project with `--IncludeSamples false` leaves all of it out from the start.
