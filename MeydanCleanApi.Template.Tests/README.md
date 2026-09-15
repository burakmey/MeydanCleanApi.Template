# MeydanCleanApi.Template.Tests

Unit tests for the parts of the template where being wrong is expensive and being right is not
obvious from reading the code.

```bash
dotnet test MeydanCleanApi.Template.slnx
```

CI runs this automatically: the `Test` step in [ci.yml](../.github/workflows/ci.yml) looks for a
project matching `*Tests*.csproj` and runs it when one exists.

---

## What is covered

| Suite | What it pins down |
| :--- | :--- |
| `LocalStorageServiceTests` | Signed upload and download URLs: that a signature is tied to one operation, one path and one expiry, and that the traversal guard refuses a path leaving the storage root |
| `CorrelationIdMiddlewareTests` | Which inbound correlation ids are trusted. The value reaches the log file, so anything accepted here is something a caller can write into the log |
| `ProductionSecretGuardTests` | That the API refuses to start outside Development while a signing key published in this repository is still configured |
| `JwtTokenServiceTests` | Token expiry comes from the clock abstraction, claims survive a round trip, a foreign-signed token is refused, and the refresh hash is keyed |
| `CreatePendingFileCommandValidatorTests` | The extension allow list, size limit and container whitelist — the rules that decide what a client may put in storage |

Several of these are regression tests for defects the code once had. Those carry a comment saying
which, so a later change that reintroduces one fails with an explanation rather than a bare red.

---

## Conventions

**No database.** Everything here runs in memory or in a temporary directory. A test that needs real
SQL belongs in a separate integration project with a PostgreSQL container, so this suite stays fast
enough to run on every save.

**Real collaborators where they are cheap.** `CorrelationIdContext`, `JwtTokenService` and the
validators are constructed directly rather than mocked. Only the clock is faked, through
`FixedClock`, because waiting for an expiry is not a test.

**Names say the rule, not the method.** `UploadSignature_IsNotAcceptedAsADownloadSignature` survives
a rename of the method it exercises; `IsSignatureValid_ReturnsFalse` does not.

---

## What is deliberately not covered

- **Anything needing a database.** Audit timestamps, the soft-delete query filter and repository
  paging all need a real provider. `ReadRepository.GetPagedAsync` is the one worth doing first,
  because nothing calls it with a `configureQuery` hook today, so its ordering logic is untested and
  unused at the same time.
- **Login timing.** Rejecting a wrong password and rejecting an unknown address are meant to take the
  same time. Asserting on a duration is flaky on shared CI hardware, so this is verified by reading
  the code rather than by a test.
- **The HTTP pipeline end to end.** The Docker smoke test in CI covers that against a running API and
  a real database, which is a better place for it than a mocked host here.
