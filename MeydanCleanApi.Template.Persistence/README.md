# Persistence Layer

## What this layer is responsible for

Persistence owns the database: the EF Core `DbContext`, the entity mappings, the generic repositories,
transaction handling, and the seeders that insert reference data.

**It depends on Application and Domain only.** It must not reference the WebApi or Infrastructure
layers. It implements interfaces the Application layer declares, such as `IReadRepository<T, TKey>`
and `IUserSessionService`, and dependency injection connects the two at startup.

---

## Folder breakdown

```text
MeydanCleanApi.Template.Persistence/
├── Abstractions/         ISeederService, ISeederService<T>
├── Configurations/       EF Core IEntityTypeConfiguration<T> mappings
│   ├── Auths/            AuthProviderConfiguration, UserAuthProviderConfiguration
│   ├── Base/             BaseFileAttachmentConfiguration<T>, shared by every attachment table
│   ├── Culture/          SupportedCultureConfiguration
│   ├── Files/            FileEntityConfiguration, FilePurposeConfiguration,
│   │                     FileStatusConfiguration, FileStorageConfiguration
│   ├── Identity/         AppUserConfiguration, AppRoleConfiguration
│   └── Samples/          SampleProduct and its translations, attributes and files
├── Contexts/
│   ├── ApplicationDbContext.cs         Audit stamping and global query filters
│   └── ApplicationDbContext.Tables.cs  DbSet<T> declarations
├── Migrations/           InitialCreate, shipped so a clone runs immediately
├── Repositories/
│   ├── ReadRepository.cs   Read side, AsNoTracking by default, soft-deleted rows excluded
│   ├── WriteRepository.cs  Add, update, soft delete, hard delete
│   └── UnitOfWork.cs       SaveChanges and transactions
├── Services/
│   └── UserSessionService.cs   Stores, finds and revokes refresh token sessions
├── Seeding/
│   ├── Base/                       BaseSeederService<T>
│   ├── AdminUserSeederService.cs   System roles and the bootstrap SuperAdmin account
│   ├── LookupDataSeederService.cs  File statuses, purposes, storage providers, auth providers, cultures
│   ├── SampleProductSeederService.cs
│   └── DatabaseSeederExtensions.cs Runs all seeders in order at startup
├── DesignTimeDbContextFactory.cs
├── GlobalUsings.cs
└── ServiceRegistration.cs
```

This template ships with **no migration files**, on purpose. Entities get renamed to suit the real
domain, so the first migration should be generated from that model rather than inherited. The API
applies pending migrations at startup and warns with the exact command to run when none exist. The
repository ships an `InitialCreate` migration so a clone runs immediately; a `dotnet new` project
does not, and generates its own.

---

### Configurations/

Mappings live in their own classes rather than as attributes on entities, which keeps EF Core out of
the Domain layer. `ApplicationDbContext` finds them all with `ApplyConfigurationsFromAssembly`.

Lookup tables use `ValueGeneratedNever()`. Their primary keys come from enum values in code, so the
database must not generate its own:

```csharp
public void Configure(EntityTypeBuilder<AuthProvider> builder)
{
    builder.HasKey(x => x.Id);
    builder.Property(x => x.Id).ValueGeneratedNever();   // Id comes from AuthProviderType
    builder.Property(x => x.Name).IsRequired().HasMaxLength(ValidationConstants.MaxShortNameLength);
}
```

Delete behaviour is set on every relationship rather than left to convention, because the EF Core
default for a required foreign key is `Cascade`, which deletes more than intended:

| Behaviour | Effect | Where it is used |
| :--- | :--- | :--- |
| `Cascade` | Deleting the parent deletes the children | `SampleProduct` to its translations |
| `Restrict` | The parent cannot be deleted while children exist | `SampleProductCategory` to its products |

### Configurations/Base/

`BaseFileAttachmentConfiguration<T>` holds the mapping shared by every attachment table: the shared
primary key, the relationships to `FileEntity` and `FilePurpose`, and the ordering index. A new
attachment table only configures what is specific to it:

```csharp
public sealed class SampleProductFileConfiguration : BaseFileAttachmentConfiguration<SampleProductFile>
{
    protected override void ConfigureAttachment(EntityTypeBuilder<SampleProductFile> builder)
    {
        builder.ToTable(nameof(ApplicationDbContext.SampleProductFiles));

        builder.HasOne(x => x.SampleProduct).WithMany(p => p.Files)
            .HasForeignKey(x => x.SampleProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.SampleProductId, x.FilePurposeId, x.SortOrder });
        builder.HasQueryFilter(x => x.SampleProduct!.IsActive);
    }
}
```

### Contexts/

`ApplicationDbContext` extends `IdentityDbContext<AppUser, AppRole, Guid>`, so identity tables and
domain tables share one context and one transaction.

It does three things automatically:

- **Audit stamping.** `SaveChangesAsync` sets `CreatedAt` and `UpdatedAt` on anything implementing
  `IBaseEntity`, and protects `CreatedAt` from being changed on update.
- **Soft delete filtering.** Entities implementing `ISoftDeletable` get a global query filter, so
  inactive rows disappear from reads without every query remembering.
- **Matching filters on children.** Translation, attribute and file entities carry a filter following
  their parent's `IsActive`. Without it EF Core warns that a required relationship points at a
  filtered entity, and a deactivated product's translations would still appear.

`FileEntity` also maps PostgreSQL's hidden `xmin` column as a row version. If two requests read the
same file record and both try to change its status, the second save fails instead of silently
overwriting the first.

### Repositories/

`ReadRepository` uses `AsNoTracking` by default, with tracking as an opt-in for rows that will be
changed. Three behaviours are worth knowing first:

- **Paging is always ordered.** `GetPagedAsync` falls back to `CreatedAt DESC, Id ASC` when no sort is
  supplied, because `Skip`/`Take` without an `ORDER BY` has no defined result in PostgreSQL. The same
  row can show up on two pages, or on none.
- **Related data is only loaded on request.** Every method that returns entities takes the same
  `includes` argument (`AnyAsync` and `CountAsync` do not, since they return a bool and a count):
  `includes: [product => product.Files]`. Forget it and the navigation comes back empty with no error,
  so the API quietly reports no related records.
- **`SoftDelete` throws for entities that are not `ISoftDeletable`.** It used to fall back to a
  permanent delete, which is a surprising thing for a method with that name to do. Call `HardDelete`
  when it is meant. The failure is an `InternalServerException` carrying
  `ERR_SOFT_DELETE_NOT_SUPPORTED` and the entity name, so it is localized like every other error.

### Soft delete

Two pieces work together:

1. **`ISoftDeletable`** on the entity adds the `IsActive` flag.
2. **The global query filter** in `ApplicationDbContext` hides inactive rows from every read.

Deactivating a row therefore removes it from the API without deleting anything. Its history, its
related records and its files all stay in the database.

```csharp
// SoftDeleteSampleProductCommandHandler
var product = await _productReadRepository.GetByIdOrThrowAsync(request.Id, enableTracking: true, ct: ct);

_productWriteRepository.SoftDelete(product);   // sets IsActive = false
await _unitOfWork.SaveChangesAsync(ct);
```

After this the product is gone from every query, including the one that just found it. Calling the
delete endpoint a second time returns 404, because the filter hides the row from the lookup too.

The repositories deliberately provide **no way to read a deactivated row back**. A "deleted items"
screen or a restore feature needs one, and the right place is the single aggregate that needs it,
rather than a hole in the shared repository. Every method on `ReadRepository` is `virtual`, so a subclass
is the natural place:

```csharp
// Persistence/Repositories/SampleProductReadRepository.cs
public sealed class SampleProductReadRepository(ApplicationDbContext context)
    : ReadRepository<SampleProduct, Guid>(context)
{
    public async Task<List<SampleProduct>> GetDeletedAsync(CancellationToken ct = default)
        => await context.SampleProducts
            .AsNoTracking()
            .IgnoreQueryFilters()          // removes EVERY global filter, so keep the method narrow
            .Where(product => !product.IsActive)
            .ToListAsync(ct);
}
```

Register it as its own type and inject it only where it is genuinely needed.

`GetByIdAsync` builds a predicate on the `Id` property, which works for every entity except
`UserAuthProvider`, whose key is a composite. Use `FirstOrDefaultAsync` for composite-key entities.

### Repositories/UnitOfWork.cs

A single `SaveChangesAsync` is already atomic, because EF Core wraps it in its own transaction. Reach
for `ExecuteInTransactionAsync` only when several steps must succeed or fail together.

`SaveChangesAsync` also turns database constraint failures into meaningful HTTP results instead of an
unexplained 500:

| Database condition | Thrown as | HTTP |
| :--- | :--- | :--- |
| Unique, foreign key or check violation | `ConflictException` | 409 |
| Row changed by someone else since it was read | `ConflictException` | 409 |

**Scenario 1 — several rows, one save.** No transaction needed. Adding a product with its files is
still one `SaveChangesAsync`.

```csharp
await _productWriteRepository.AddAsync(product, ct);
await _productFileWriteRepository.AddAsync(productFile, ct);

await _unitOfWork.SaveChangesAsync(ct);   // all rows commit together, or none do
```

**Scenario 2 — a database change plus an irreversible outside effect.** Deleting a stored object
cannot be undone, so the database row goes first and the file last. If the row will not delete because
something still references it, the file is never touched.

```csharp
await _unitOfWork.ExecuteInTransactionAsync(async token =>
{
    _fileWriteRepository.HardDelete(fileEntity);
    await _unitOfWork.SaveChangesAsync(token);

    await _coordinator.DeleteFromStorageAsync(fileEntity, token);
}, ct);
```

**Scenario 3 — two saves that must agree.** Creating an account and linking its sign-in provider are
separate saves through different services. A half-finished result leaves an account nobody can sign
in to.

```csharp
await _unitOfWork.ExecuteInTransactionAsync(async token =>
{
    await _userAdminService.AssignRoleAsync(userId, RoleConstants.Customer, token);
    await _userAuthProviderWriteRepository.AddAsync(providerLink, token);
    await _unitOfWork.SaveChangesAsync(token);
}, ct);
```

**Scenario 4 — do not wrap a plain read.** Queries need no transaction. Reads already run against a
consistent snapshot, and opening one just holds a connection longer.

```csharp
// Wrong
await _unitOfWork.ExecuteInTransactionAsync(async token => { products = await _repo.GetPagedAsync(...); }, ct);

// Right
var products = await _productReadRepository.GetPagedAsync(request.Page, ct: ct);
```

**Scenario 5 — do not call a third-party API inside a transaction.** A payment call takes seconds and
cannot be rolled back. Holding a transaction open across it locks rows for the whole call, and a
failure afterwards cannot un-charge the customer. Save intent first, commit, then call out, then save
the result in a second short transaction.

**Scenario 6 — retries.** `ExecuteInTransactionAsync` runs inside the connection retry strategy, so a
dropped connection replays the whole block. Keep anything that must not happen twice, such as sending
an email, outside it.

There is no separate Begin, Commit and Rollback trio. Callers cannot forget to roll back, and manual
`BeginTransaction` is incompatible with `EnableRetryOnFailure`, which is switched on in
`AddPersistenceServices`.

### Seeding/

Seeders run at startup and are safe to repeat, because each checks whether its data already exists.
They run in dependency order: lookup data, then roles and the bootstrap administrator, then sample
data. Sample data is skipped outside Development.

The administrator is only created when `SeedData:SuperAdminEmail` and `SeedData:SuperAdminPassword`
are configured, and only when the database has no users at all. Supply the password through
user-secrets or an environment variable, never in `appsettings.json`.

Outside Development a seeding failure stops startup. Continuing would leave the database half filled
while the API reports itself healthy.

---

## Migrations

`DesignTimeDbContextFactory` lets the EF Core CLI build a `DbContext` without running the API. It
walks up to the WebApi folder, reads the configuration files and environment variables, and resolves
`ConnectionStrings:DefaultConnection`.

Every command needs two flags:

- `--project` is where migration files are written (Persistence).
- `--startup-project` is where configuration and the `Microsoft.EntityFrameworkCore.Design` package
  live (WebApi).

**Create the first migration**

```bash
dotnet ef migrations add InitialCreate --project MeydanCleanApi.Template.Persistence --startup-project MeydanCleanApi.Template.WebApi
```

**Apply pending migrations**

```bash
dotnet ef database update --project MeydanCleanApi.Template.Persistence --startup-project MeydanCleanApi.Template.WebApi
```

**Roll back to a specific migration**

```bash
dotnet ef database update TargetMigrationName --project MeydanCleanApi.Template.Persistence --startup-project MeydanCleanApi.Template.WebApi
```

**Remove the last migration, if it has not been applied yet**

```bash
dotnet ef migrations remove --project MeydanCleanApi.Template.Persistence --startup-project MeydanCleanApi.Template.WebApi
```

**Drop the database, for development only**

```bash
dotnet ef database drop --project MeydanCleanApi.Template.Persistence --startup-project MeydanCleanApi.Template.WebApi
```

---

## Service registration

```csharp
builder.Services.AddPersistenceServices(builder.Configuration);
```

Registers the `DbContext` on PostgreSQL with retry-on-failure, ASP.NET Core Identity with its password
policy, the generic repositories, `IUnitOfWork`, and `IUserSessionService`.
