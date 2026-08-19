# Domain Layer

## What this layer is responsible for

The Domain layer holds the business model: entities, enums, domain exceptions, and the contracts that
describe what an entity is. It is the innermost layer, so everything else depends on it.

**It depends on nothing.** No Entity Framework, no ASP.NET Core, no database. The one exception is
`Microsoft.Extensions.Identity.Stores`, which supplies the `IdentityUser` and `IdentityRole` base
classes for `AppUser` and `AppRole`. That is a deliberate trade to avoid rebuilding identity from
scratch.

---

## Folder breakdown

```text
MeydanCleanApi.Template.Domain/
├── Constants/            Error codes, roles, policies, validation limits
├── Entities/
│   ├── Auths/            AuthProvider, UserAuthProvider
│   ├── Base/             BaseEntity<TKey>, GuidEntity, IntEntity, BasicEntity, BaseFileAttachment
│   ├── Culture/          SupportedCulture
│   ├── Files/            FileEntity, FileStatus, FileStorage, FilePurpose
│   ├── Identity/         AppUser, AppRole
│   └── Samples/          Reference entities, deleted when the real domain starts
├── Enums/                AuthProviderType, FileStatusType, FileStorageType, FilePurposeType, SupportedCultureType
├── Exceptions/
│   └── Base/             AppException, the parent of every domain exception
├── Interfaces/
│   └── Entities/         IBaseEntity, ISoftDeletable, ISortable, ITranslatable
└── Models/               Error, ErrorResponse
```

---

### Constants/

Stable string codes and numeric limits shared across layers.

`ErrorCodes` and `ValidationCodes` are locale-independent keys such as `ERR_ID_NOT_FOUND` and
`VAL_NAME_REQUIRED`. Code throws the key; the `.resx` files in the Application layer turn it into text
for the caller's language. Because the code never changes, clients can branch on it safely.

`ValidationConstants` holds the numbers (`MaxNameLength = 200`, `MinPrice = 0.01m`). Entity
configurations and FluentValidation validators both read from here, so a length limit is defined once.

`RoleConstants` also exposes two sets that carry rules:

- `AdminPanelRoles` backs `CurrentUserExtensions.IsAdmin()` in the Application layer.
- `AssignableRoles` excludes `SuperAdmin`, so `UserAdminService` cannot hand that role out.

### Entities/Base/

`BaseEntity<TKey>` gives every entity an `Id` plus `CreatedAt` and `UpdatedAt`, which the DbContext
stamps automatically. `GuidEntity` and `IntEntity` are shorthands for the two key types used here.
`BasicEntity` is for lookup tables that only need an `Id` and a `Name`.

`BaseFileAttachment` is covered under Entities/Files below, since it only makes sense alongside
`FileEntity`.

### Entities/Files/

Files are modelled in two separate pieces, and keeping them apart is what stops the table count from
growing every time a new kind of document appears.

| Piece | Answers | Type |
| :--- | :--- | :--- |
| The file | Where are the bytes? Size, MIME type, path, upload status | `FileEntity` |
| The attachment | Whose is it, what role does it play, in what order | `SampleProductFile` and friends |

`FileEntity` says nothing about owners or purpose. Those are facts about a relationship, so they live
on the row that expresses the relationship.

```csharp
// The shared base: everything every attachment needs, and nothing else.
public abstract class BaseFileAttachment : GuidEntity, ISortable
{
    public override Guid Id { get => FileEntityId; set => FileEntityId = value; }

    public required Guid FileEntityId { get; set; }
    public required int FilePurposeId { get; set; }   // Gallery, Thumbnail, Datasheet, Manual
    public required int SortOrder { get; set; }

    public FileEntity? FileEntity { get; set; }
    public FilePurpose? FilePurpose { get; set; }
}

// An aggregate's attachment table: the owner key, plus whatever that domain needs.
public class SampleProductFile : BaseFileAttachment
{
    public required Guid SampleProductId { get; set; }
    public SampleProduct? SampleProduct { get; set; }
}
```

One attachment table per aggregate root, not per file kind. Product photos and the product's PDF
manual are rows in the same table, told apart by `FilePurposeId`.

`Id` is aliased to `FileEntityId`, so an attachment row and its file share one identifier and moving
between them needs no join. The trade-off is that a file can be attached exactly once. If two owners
each need a copy, upload it twice.

**To add a new kind of file:** add a member to `FilePurposeType`, add the matching row in
`LookupDataSeederService`. No new table, no change to existing code.

**To add files to another entity:** three steps, none of which touch existing files.

```csharp
// 1. The entity
public class AppUserFile : BaseFileAttachment
{
    public required Guid AppUserId { get; set; }
    public AppUser? AppUser { get; set; }
}

// 2. The mapping — the base class supplies the key, both relationships and the ordering index
public sealed class AppUserFileConfiguration : BaseFileAttachmentConfiguration<AppUserFile>
{
    protected override void ConfigureAttachment(EntityTypeBuilder<AppUserFile> builder)
    {
        builder.ToTable(nameof(ApplicationDbContext.AppUserFiles));
        builder.HasOne(x => x.AppUser).WithMany()
            .HasForeignKey(x => x.AppUserId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => new { x.AppUserId, x.FilePurposeId, x.SortOrder });
    }
}

// 3. Add one DbSet to ApplicationDbContext.Tables.cs, then create a migration.
```

**To add descriptive fields**, extend the concrete attachment entity rather than the base. Real typed
columns stay queryable and validated:

```csharp
public class InvoiceFile : BaseFileAttachment
{
    public required Guid InvoiceId { get; set; }
    public int? PageCount { get; set; }
    public string? AltText { get; set; }
}
```

**Why not one table for everything?** Two alternatives exist and neither is the default here.

A polymorphic table (`OwnerType` + `OwnerId` columns) never grows, but no foreign key can be placed on
a polymorphic column. That means no referential integrity, no cascade rules, and orphan rows nobody
notices.

EF Core Table-Per-Hierarchy is the better alternative. Keep the same C# classes and fold them into one
table with a discriminator:

```csharp
builder.ToTable("FileAttachments")
       .HasDiscriminator<string>("OwnerType")
       .HasValue<SampleProductFile>("SampleProduct")
       .HasValue<AppUserFile>("AppUser");
```

Each owner key stays a real foreign key, nullable only because a row is one owner type or the other,
so integrity and cascade survive. The cost is that the table gains a nullable column per owner type.
That is comfortable at five to ten owner types and unpleasant past thirty. It suits a domain where many
entities carry files, or where every attachment is often needed in one query.

### Entities/Identity/

`AppUser` extends `IdentityUser<Guid>` and adds three session columns: `RefreshTokenHash`,
`RefreshTokenExpiration` and `CurrentJti`. These give one active session per user. Signing in on a
second device replaces the first. To support several devices, move those three columns into their own
table with one row per device.

### Entities/Samples/

Reference entities showing multi-language content, soft delete, sorting and file attachments working
together. Delete this folder once the real domain starts.

`ITranslatable<TTranslation>` marks the master entity that owns translations. `ITranslatable` marks
the translation row itself and forces it to carry a `CultureCode`. Core fields such as price and stock
stay on the master; only text is duplicated per language.

```csharp
public class SampleProduct : GuidEntity, ITranslatable<SampleProductTranslation>, ISoftDeletable
{
    public required string Name { get; set; }        // culture-invariant internal name
    public decimal Price { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<SampleProductTranslation> Translations { get; set; } = [];
}

public class SampleProductTranslation : BaseEntity<Guid>, ITranslatable
{
    public Guid SampleProductId { get; set; }
    public required string CultureCode { get; set; }  // "tr-TR", "en-US"
    public required string Title { get; set; }
    public required string Slug { get; set; }
}
```

### Interfaces/Entities/

`ISoftDeletable` adds an `IsActive` flag. Setting it to `false` hides the row instead of deleting it.
The Persistence layer applies a global query filter to every entity implementing this interface, so
inactive rows disappear from reads without each query having to remember.

`ISortable` adds `SortOrder` for user-defined display order, such as arranging gallery images.

### Exceptions/

Every domain exception derives from `AppException`, which carries an `Error` and an HTTP status code.
The global exception handler in the WebApi layer reads both, so throwing the right exception is all a
handler has to do to produce the right HTTP response.

Each exception exposes a static factory instead of a public constructor, which keeps the error code
consistent at every call site.

| Exception | How it is thrown | Status |
| :--- | :--- | :--- |
| `IdNotFoundException` | `IdNotFoundException.For<SampleProduct>()` | 404 |
| `ValidationException` | `ValidationException.From(errors)` | 400 |
| `UnauthorizedException` | `UnauthorizedException.WithCode()` | 401 |
| `InvalidCredentialsException` | `InvalidCredentialsException.WithCode()` | 401 |
| `ForbiddenException` | `ForbiddenException.WithCode()` | 403 |
| `ConflictException` | `ConflictException.WithCode(ErrorCodes.EmailInUse)` | 409 |
| `InternalServerException` | `InternalServerException.WithCode()` | 500 |
| `ConfigurationMissingException` | `ConfigurationMissingException.ForSection("Tokens:Jwt", "JwtSecurityKey")` | 500 |

### Models/

`Error` pairs an error code with optional format arguments. `ErrorResponse` is the JSON shape every
failed request returns:

```json
{
  "status": 404,
  "errorCode": "ERR_ID_NOT_FOUND",
  "message": "SampleProduct was not found.",
  "errors": null
}
```

`errors` is only populated for validation failures, where it maps each field name to its messages.
