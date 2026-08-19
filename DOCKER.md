# Running this project with Docker

Docker runs the API and the database together. **Neither PostgreSQL nor the .NET SDK has to be
installed** — Docker carries its own copies of both. Docker Desktop is the only requirement.

All commands run from this folder, the one holding `docker-compose.yml`.

| Service   | What it is              | Address                | Starts by default?  |
| --------- | ----------------------- | ---------------------- | ------------------- |
| `db`      | PostgreSQL 18           | `localhost:5433`       | yes                 |
| `api`     | ASP.NET Core 10 Web API | http://localhost:5080  | yes                 |
| `pgadmin` | Database browser        | http://localhost:5050  | no, profile `tools` |

---

## Getting started

Create the settings file. `.env.example` holds placeholder values that work locally; `.env` is the
working copy and git never stores it:

```bash
cp .env.example .env
```

Start everything:

```bash
docker compose up -d --build
```

The API is then at http://localhost:5080/swagger. That is the whole setup. Tables, sample data and an
administrator account are created automatically on first run, using the `InitialCreate` migration
that ships with the repository.

The first build takes a few minutes while Docker downloads .NET and PostgreSQL. Later starts take
about 15 seconds.

Confirm both parts are healthy:

```bash
docker compose ps
```

Both should read `healthy`. For the API that means more than "the process started": Compose calls
`/health/ready`, which runs a real query against PostgreSQL.

---

## Docker words, in plain language

Every example below is from this project's `docker-compose.yml`.

**Image** — a frozen copy of a program, ready to start. Like an installer. It never changes and holds
no data. This project uses two:

- `postgres:18-alpine` is downloaded ready-made, and runs the database.
- `<project>-api` is built here from the `Dockerfile`, and runs the API.

**Container** — an image that is running, like an open program. `docker compose up` creates two:

- `<project>-db-1` started from the `postgres:18-alpine` image.
- `<project>-api-1` started from the `<project>-api` image.

The image is the recipe; the container is the meal. Deleting a container discards everything written
inside it, which is why data belongs in a volume instead.

**Volume** — a storage box that lives outside the container, so its contents survive. This project
uses two that matter:

- `<project>_pgdata` holds the PostgreSQL data files, every table and row.
- `<project>_uploads` holds uploaded files.

Because they sit outside, `docker compose down` can delete both containers without losing anything.

**Port mapping** — the line `"5080:8080"` in `docker-compose.yml`. Two different ports:

- **5080 is on the computer running Docker.** This is the one that goes in the browser:
  http://localhost:5080
- **8080 is inside the container.** The API listens there because the `Dockerfile` sets
  `ASPNETCORE_HTTP_PORTS=8080`. Docker did not choose it; this project did.

The two are independent. Changing the left number changes the browser address, while the right one
stays 8080 because that is where the app listens.

The database works the same way with `"5433:5432"`. PostgreSQL always listens on 5432 inside the
container. 5433 is used outside so that a PostgreSQL installed on the computer can keep 5432.

**Compose** — the tool that reads `docker-compose.yml` and starts several containers together, in the
right order. Without it each container would need a long `docker run` command.

**Building is not running.** This one causes the most confusion.

- `docker compose up` creates and starts **containers**. It builds an image only when none exists
  yet, which is why the very first run builds one.
- After that, `up` reuses the image already on disk. It does not look at the source files at all.
- `docker compose up --build` forces a new image from the current files first.

So anything copied into the image needs `--build` to take effect:

| Changed | Needs `--build`? |
| --- | --- |
| C# source code | yes |
| A NuGet package in a `.csproj` | yes |
| `appsettings.json` | yes |
| A `.resx` translation file | yes |
| The `Dockerfile` itself | yes |
| `.env` | no |

`.env` is the exception because those values are handed to the container when it starts, not baked
into the image. `docker compose up -d` is enough for those.

---

## What Docker creates for this project

Names below use `<project>` for the folder name in lower case with dots removed. A project in a
folder called `YourApi` gets `yourapi-api-1` and `yourapi_pgdata`. Two projects from this template
therefore never share anything.

### Images

```bash
docker images
```

| Image | Size | What it is for |
| --- | --- | --- |
| `<project>-api` | ~362 MB | The API, built locally from the `Dockerfile` |
| `postgres:18-alpine` | ~433 MB | Runs the database container |
| `mcr.microsoft.com/dotnet/aspnet:10.0` | ~340 MB | The .NET **runtime**. The API image is built on top of it |
| `mcr.microsoft.com/dotnet/sdk:10.0` | ~1.24 GB | The .NET **compiler**, used only while building |

**Why two .NET images.** Docker cannot use the .NET installed on the machine: a container is a
separate small Linux system and cannot see programs installed on Windows. So the `Dockerfile` uses
the large SDK image to compile, copies the result into the small runtime image, and discards the SDK.
The final API image is therefore ~362 MB instead of over 1.5 GB.

### Containers

```bash
docker compose ps
```

| Container | What it is |
| --- | --- |
| `<project>-api-1` | the running API |
| `<project>-db-1` | the running PostgreSQL |
| `<project>-pgadmin-1` | pgAdmin, only with `--profile tools` |

Containers hold nothing important. Deleting and recreating them is routine — `docker compose up -d
--build` does exactly that every time.

### Volumes

```bash
docker volume ls
```

| Volume | What it holds |
| --- | --- |
| `<project>_pgdata` | every table and row in the database |
| `<project>_uploads` | uploaded files |
| `<project>_dpkeys` | keys that protect cookies and links |
| `<project>_pgadmin` | pgAdmin's own settings |

All project data lives here, and only here. This is the only part worth protecting.

**About `dpkeys`.** ASP.NET Core Data Protection keys encrypt small values such as password reset
links, email confirmation links and login cookies. This template does not use them yet — login uses
JWT, signed with the `Tokens:Jwt` keys, which is a separate mechanism. The volume is in place because
those features usually arrive later, and without it new keys are generated whenever the container is
replaced, breaking every link already sent to a user.

---

## Daily commands

| Command | What it does |
| --- | --- |
| `docker compose up -d` | starts everything |
| `docker compose down` | stops everything, keeps the data |
| `docker compose ps` | shows what is running and whether it is healthy |
| `docker compose logs -f api` | follows the API log, `Ctrl+C` to stop |
| `docker compose restart api` | restarts the API without rebuilding |
| `docker compose --profile tools up -d` | also starts pgAdmin, which stays off otherwise |

---

## After a code or settings change

Docker does not pick up changes on its own.

| What changed | Command |
| --- | --- |
| C# code | `docker compose up -d --build api` |
| A new EF Core migration | `docker compose up -d --build api` |
| Only `.env` | `docker compose up -d` |

Settings are read when the container starts, so `.env` needs no rebuild. Migrations are applied by
the API itself at startup, so rebuilding is enough.

---

## Keeping Docker up to date

The base images (PostgreSQL, pgAdmin, .NET) receive security fixes and are worth updating every month
or two:

```bash
docker compose pull
```

```bash
docker compose up -d --build
```

`pull` fetches the newer downloaded images. `--build` is still needed because the API image is built
locally, and its .NET base layer only changes during a build.

A minor PostgreSQL update (18.1 to 18.2) keeps the data. A major one (18 to 19) does not: files are
stored per major version, so version 19 starts empty.

---

## Cleaning up disk space

Every build leaves files behind, and this can reach tens of gigabytes.

```bash
docker system df
```

**Build cache is almost always the biggest, and is safe to delete at any time.** It is never data,
and the only cost is one slower build afterwards:

```bash
docker builder prune -a
```

Two things to know before deleting anything else:

- `docker system prune` **never removes volumes**, and never removes an image that still has a name.
  That is deliberate, and it is why disk stays full after running it.
- `docker image prune -a` removes every image no running container uses, **including images from
  other projects**. Remove images by name instead: `docker rmi <name>:<tag>`.

Never delete the `_pgdata` or `_uploads` volumes of a project still in use. Wiping a project on
purpose is the next section.

---

## Starting the database from zero

```bash
docker compose down -v
```

The `-v` deletes the volumes: the database and every uploaded file are gone.

The next `docker compose up` does not restore them. It builds a **new** empty database, the API
recreates all tables from the migrations, and the seeder inserts the lookup data, the sample product
and the administrator account again. The result is a fresh install, not old tables with the rows
removed.

---

## Looking inside the database

The database container already includes `psql`, so no client needs installing. `\dt` lists tables,
`\q` exits:

```bash
docker compose exec db psql -U postgres -d meydanclean_db
```

Table names need double quotes, because PostgreSQL folds unquoted names to lower case:

```bash
docker compose exec db psql -U postgres -d meydanclean_db -c 'SELECT "Id","Name" FROM "SampleProducts";'
```

pgAdmin at http://localhost:5050 offers the same with a user interface. Registering the database
there needs host `db` and port `5432`, the address **inside** Docker, not `localhost:5433`.

---

## Two databases with the same name

Running this project in Visual Studio as well means two databases exist. Both can be called
`meydanclean_db`, and only the port separates them:

| Where | Port | Used by |
| --- | --- | --- |
| Installed on the machine | **5432** | Visual Studio, `dotnet run` |
| In Docker | **5433** | the container |

Missing data is usually the wrong port rather than lost rows. Both can run at once, because the ports
differ.

---

## EF Core commands against the Docker database

`dotnet ef` runs outside Docker, so it reaches the container through port `5433`:

```bash
dotnet ef migrations add YourMigrationName --project MeydanCleanApi.Template.Persistence --startup-project MeydanCleanApi.Template.WebApi -- --connection "Host=localhost;Port=5433;Database=meydanclean_db;Username=postgres;Password=postgres"
```

`dotnet ef database update` is rarely needed here, because the API applies pending migrations itself
at startup.

---

## Settings

Every setting reaches the container as an environment variable. `.env.example` documents all of them
and is the file to read.

The naming rule is mechanical — two underscores mean one level deeper:

| Environment variable | Setting in the app |
| --- | --- |
| `ConnectionStrings__DefaultConnection` | `ConnectionStrings:DefaultConnection` |
| `Tokens__Jwt__JwtSecurityKey` | `Tokens:Jwt:JwtSecurityKey` |
| `CorsOrigins__0` | first entry of `CorsOrigins` |

`appsettings.Development.json` is deliberately excluded from the image, so the container is configured
the way a deployed server would be. A missing setting therefore fails here first, not after
deployment.

`API_JWT_ACCESS_KEY` and `API_JWT_REFRESH_KEY` are required, and Compose refuses to start without
them. Both need at least 32 characters:

```bash
openssl rand -base64 48
```

---

## Where uploaded files are saved

This section describes **local disk storage**, which is the default and needs no account. Supabase
works differently and is covered at the end.

Two settings control it, both in `docker-compose.yml`:

| Setting | Value | Meaning |
| --- | --- | --- |
| `Storage__Local__RootDirectory` | `App_Data/uploads` | where files are written, relative to `/app` |
| `Storage__Local__BaseUrl` | `/api/v1/files` | the address clients download through |

So inside the container files land under `/app/App_Data/uploads`, and the `<project>_uploads` volume
is mounted exactly there, which is what makes them survive a restart. A stored file looks like this:

```text
/app/App_Data/uploads/public/products/8f3c1e90-....png
```

Listing them:

```bash
MSYS_NO_PATHCONV=1 docker compose exec api ls -R /app/App_Data/uploads
```

### Why not wwwroot

`wwwroot` is the folder ASP.NET Core publishes to the internet through `UseStaticFiles`. Anything
placed there is downloadable by **anyone who knows the file name**, with no login and no permission
check.

Uploads are therefore kept out of it on purpose. `App_Data` is not served by anything, so the only
way to reach a file is through `/api/v1/files`, which checks permission first. Putting uploads in
`wwwroot` would silently publish every private document in the system.

### Using Supabase instead

Filling in `API_SUPABASE_URL` and `API_SUPABASE_API_KEY` in `.env`, then running
`docker compose up -d`, switches storage to a Supabase bucket. Files then live in the cloud rather
than on disk, and the `<project>_uploads` volume is no longer used.

---

## Building the image without Compose

The `Dockerfile` also stands on its own:

```bash
docker build -t yourapi .
```

No secret is ever baked into the image. Every value is passed at run time:

```bash
docker run -p 8080:8080 -e ASPNETCORE_ENVIRONMENT=Production -e ConnectionStrings__DefaultConnection="Host=host.docker.internal;Port=5432;Database=meydanclean_db;Username=postgres;Password=YOUR_PASSWORD" -e Tokens__Jwt__JwtSecurityKey="replace-with-32-or-more-random-characters" -e Tokens__Jwt__RefreshSecurityKey="replace-with-a-different-32-plus-character-string" yourapi
```

`.dockerignore` excludes `appsettings.Development.json` and `.env`, because `COPY . .` ignores
`.gitignore` and would otherwise bake real credentials into the image. `App_Data/` and `Logs/` are
excluded too, so stale data never ships. A volume must be mounted over `/app/App_Data` in production,
or uploads disappear when the container is replaced.

---

## When something goes wrong

**The API stays at `starting`, or keeps restarting.** The log almost always names the cause:

```bash
docker compose logs api
```

**Cannot connect to the database.** Usually an old database container left from a different password.
This fixes it, at the cost of the data:

```bash
docker compose down -v && docker compose up -d
```

**A port is already in use.** Change `COMPOSE_API_PORT`, `COMPOSE_DB_PORT` or
`COMPOSE_PGADMIN_PORT` in `.env`, then run `docker compose up -d` again.

**Uploading a file fails with a permission error.** The app runs as a non-root user, and an old
volume can carry the wrong owner. Removing that volume deletes uploaded files only, not the database:

```bash
docker compose down && docker volume rm <project>_uploads && docker compose up -d
```

**A command containing a path fails in Git Bash.** Git Bash rewrites paths starting with `/` into
Windows paths before Docker sees them:

```bash
MSYS_NO_PATHCONV=1 docker compose exec api ls -l /app/App_Data/uploads
```
