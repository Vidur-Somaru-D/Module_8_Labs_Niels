# =============================================================================
# Derivco PlayerApi — container image
#
# You are NOT asked to write this file. You are asked to READ it, understand
# what every instruction does, and answer the questions in README.md task 3.
#
# One line in this file breaks a rule covered in the module. Find it.
# =============================================================================


# -----------------------------------------------------------------------------
# Stage 1 — build
#
# This stage uses the full .NET SDK image: compilers, NuGet, MSBuild, the lot.
# It is roughly 800 MB. We need all of that to compile the app — but none of it
# to *run* the app, which is why there is a second stage below.
# -----------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy ONLY the project file first, then restore.
#
# Docker caches each instruction as a layer and reuses the cache until an input
# changes. Because .csproj changes far less often than your C# code, restoring
# in its own layer means editing PlayersController.cs does not re-download
# every NuGet package. Copy everything first and you lose that — every source
# edit invalidates the restore.
COPY Derivco.PlayerApi/Derivco.PlayerApi.csproj Derivco.PlayerApi/
RUN dotnet restore Derivco.PlayerApi/Derivco.PlayerApi.csproj

# Now copy the source and publish. --no-restore because the layer above did it.
COPY Derivco.PlayerApi/ Derivco.PlayerApi/
RUN dotnet publish Derivco.PlayerApi/Derivco.PlayerApi.csproj \
    -c Release \
    -o /app/publish \
    --no-restore


# -----------------------------------------------------------------------------
# Stage 2 — runtime
#
# A brand new image, starting from the ASP.NET *runtime* (~220 MB, no SDK).
# Nothing from stage 1 carries over except what we explicitly COPY --from=build.
# The result: no compilers, no NuGet cache, no source code in the shipped image.
# Smaller to pull, faster to start, and a much smaller attack surface.
# -----------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Kestrel listens on 8080. Note it is plain HTTP — inside the cluster, TLS is
# terminated at the ingress/load balancer, so the container never sees HTTPS.
ENV ASPNETCORE_URLS=http://+:8080

# Production disables Swagger UI and the developer exception page — see Program.cs.
ENV ASPNETCORE_ENVIRONMENT=Production

# CHANGE: Removed the baked-in Database__ConnectionString because configuration and credentials
# must be injected at runtime; anyone able to pull the image could otherwise inspect the secret.

# Take only the published output from stage 1 — not the source, not the SDK.
COPY --from=build /app/publish .

# Documents the port for humans and tooling. It does NOT publish the port —
# that is `docker run -p 8080:8080` or the `ports:` block in docker-compose.yml.
EXPOSE 8080

ENTRYPOINT ["dotnet", "Derivco.PlayerApi.dll"]