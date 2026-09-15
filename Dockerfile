# ==============================================================================
# STAGE 1: BASE (Runtime Environment)
# ==============================================================================
# FROM: the lightweight ASP.NET 10.0 runtime image. It has no SDK, no compilers.
# 'AS base' names this stage so later stages can build on it.
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base

# WORKDIR: every relative path after this line is resolved inside /app.
WORKDIR /app

# ENV: tells Kestrel which port to listen on. Hosting platforms usually expect 8080.
ENV ASPNETCORE_HTTP_PORTS=8080

# EXPOSE: documents the listening port for Docker and the hosting platform.
EXPOSE 8080


# ==============================================================================
# STAGE 2: BUILD (Restore & Compile)
# ==============================================================================
# FROM: the full SDK image, which has the compilers and CLI tools the runtime image lacks.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

# ARG: build-time only. Release turns on compiler optimisations.
ARG BUILD_CONFIGURATION=Release

WORKDIR /src

# COPY the .csproj files on their own, before any source code.
# Docker caches each layer. Source changes far more often than project files, so restoring
# first means an ordinary code change reuses the cached restore instead of re-downloading
# every NuGet package.
#
# Directory.Packages.props comes with them, and is not optional. Package versions live there
# rather than in the project files, so a restore that cannot see it fails on every
# PackageReference for having no version at all.
COPY ["Directory.Packages.props", "./"]
COPY ["MeydanCleanApi.Template.WebApi/MeydanCleanApi.Template.WebApi.csproj", "MeydanCleanApi.Template.WebApi/"]
COPY ["MeydanCleanApi.Template.Application/MeydanCleanApi.Template.Application.csproj", "MeydanCleanApi.Template.Application/"]
COPY ["MeydanCleanApi.Template.Domain/MeydanCleanApi.Template.Domain.csproj", "MeydanCleanApi.Template.Domain/"]
COPY ["MeydanCleanApi.Template.Infrastructure/MeydanCleanApi.Template.Infrastructure.csproj", "MeydanCleanApi.Template.Infrastructure/"]
COPY ["MeydanCleanApi.Template.Persistence/MeydanCleanApi.Template.Persistence.csproj", "MeydanCleanApi.Template.Persistence/"]

# RUN dotnet restore: resolves and downloads every NuGet dependency.
RUN dotnet restore "MeydanCleanApi.Template.WebApi/MeydanCleanApi.Template.WebApi.csproj"

# COPY the rest of the source. .dockerignore decides what is left out.
COPY . .

WORKDIR "/src/MeydanCleanApi.Template.WebApi"

# RUN dotnet publish: compiles and packages the app for deployment in one step.
# There is deliberately no separate 'dotnet build' stage. Publish compiles anyway,
# so building first would do the same work twice and make the image slower to produce.
# /p:UseAppHost=false drops the native executable wrapper, which the container does not need.
RUN dotnet publish "MeydanCleanApi.Template.WebApi.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false


# ==============================================================================
# STAGE 3: FINAL (Production Image)
# ==============================================================================
# FROM base: back to the runtime-only image. The SDK and all intermediate build output stay
# behind in the build stage, which keeps the shipped image around 200 MB instead of 800 MB+.
FROM base AS final

WORKDIR /app

# Run as a non-root user, because a break-out from a root process is a break-out as root.
# The image already ships one: user 'app', whose id is in the APP_UID environment variable.
# Do not try to create your own here — this image has no adduser or useradd, so a RUN adduser
# line fails the build with exit code 127.
COPY --from=build --chown=app:app /app/publish .

# Local file uploads land here (Storage:Local:RootDirectory). Mount a volume over it in
# production, otherwise uploads disappear when the container is replaced. The chown matters
# for that mount: Docker copies this directory's ownership into a new named volume, which is
# what lets the non-root process write to it.
RUN mkdir -p /app/App_Data/uploads && chown -R app:app /app/App_Data

USER $APP_UID

# ENTRYPOINT: the command that runs when the container starts.
# ASPNETCORE_ENVIRONMENT and every secret are supplied at runtime, never baked into the image.
ENTRYPOINT ["dotnet", "MeydanCleanApi.Template.WebApi.dll"]
