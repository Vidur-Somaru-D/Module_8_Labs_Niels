# Module 8 Lab — Production Reality: Tooling, Containers, and AI

## The starting point

This is the fully working API from Modules 5–7, with a passing test suite. Nothing is broken. Your job is to make it production-ready and extend it using AI assistance.

## Ground rules

Comment every change you make. For the AI-generated endpoint, your comments should include what you found when you reviewed the output and what you changed before committing.

## Prerequisites

Same setup as Module 6 — SQL Server runs in a container, no local install needed - just Rancher desktop.

Clone this repo to your local machine. The directory you clone the repo to will be your working directory for this lab.

* Open the `before` folder in your favourite editor
* Open a terminal window in the same folder (or run from the editor's terminal)

**1. Start SQL Server:**

In the `before` folder you see a file: `docker-compose.yml`, containing s SQL Server container. Spin up SQL:

```bash
docker compose up -d sqlserver
```

Wait about 20–30 seconds for it to initialise (`docker compose logs -f sqlserver` until you see "SQL Server is now ready").

**2. Create the database, tables, and seed data:**

```bash
# create the database and the tables
docker compose exec sqlserver /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P "Derivco@Training1" \
  -C \
  -i /scripts/schema.sql

# ingest data into the tables
docker compose exec sqlserver /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P "Derivco@Training1" \
  -C -d DerivcoTraining -i /scripts/seed-data.sql
```

The connection string in `appsettings.json` is already configured for this container.

**3. Confirm the starting point still works:**

```bash
dotnet build Derivco.PlayerApi.sln   # confirm everything still passes before you start
dotnet test
dotnet run --project Derivco.PlayerApi/Derivco.PlayerApi.csproj
# make note of the portnumber when the project spins up
# you use that below
```


`GET http://localhost:<port>/api/players` should return the six seeded players.

When you have completed the lab (see below), commit and push to GH.

---

## What to build

### 1 — Structured logging with Serilog

Add structured logging so that every request produces a JSON log entry on stdout. The packages you need are `Serilog.AspNetCore` and `Serilog.Sinks.Console`. Configure it in `Program.cs` — stdout only, no file sink. Add a `Serilog` section to `appsettings.json` to control log levels.

When done, `dotnet run` should produce JSON log output in the terminal for every request.

### 2 — Connection string via environment variable

The connection string should not live in a config file checked into source control. Move it so that the application reads it from an environment variable at startup.

ASP.NET Core maps environment variables to configuration automatically. The separator convention for nested keys on Linux is a double underscore: `Database__ConnectionString` maps to `Database:ConnectionString` in config.

Verify the change works in both directions — with the variable set and without it.

### 3 — Read, run, and fix the container setup

You are **not** writing the `Dockerfile` — it is already in this folder, and so is `docker-compose.yml`. Writing them from scratch is D2-level work. Your job here is to understand what you are about to depend on, which is the same standard the module applies to AI-generated code: if you cannot explain it, you do not ship it.

**a. Read `Dockerfile` top to bottom** and be ready to answer, out loud, in your own words:

- Why are there two `FROM` lines? What is in the final image, and what is *not*?
- Why is `Derivco.PlayerApi.csproj` copied on its own before the rest of the source?
- What is the difference between `EXPOSE 8080` and `-p 8080:8080`?
- Why does the app listen on plain HTTP rather than HTTPS?

**b. Find the rule violation.** One `ENV` line in the `Dockerfile` breaks a rule from this module. Run the AI review checklist over the file as though a colleague had written it — the same checklist you will use in task 4. When you find it, delete the line and write a comment in your commit message explaining *why* it was wrong, not just that you removed it.

> Hint if you are stuck: re-read the "Config Must Not Live in the Image" slide. Ask yourself who can pull an image out of a container registry.

**c. Build and run the whole thing:**

```bash
docker compose up --build
```

This builds the API image and starts it alongside SQL Server. Note that removing the `ENV` line in step (b) does **not** break anything — `docker-compose.yml` injects `Database__ConnectionString` at runtime instead. That is the point: the image ships inert, and the environment supplies the values.

**d. Verify it:**

```bash
curl http://localhost:8080/api/players
docker compose logs api            # your Serilog JSON, on stdout
```

Swagger is not available here — the container sets `ASPNETCORE_ENVIRONMENT=Production`, and `Program.cs` only maps Swagger in Development. That is deliberate, and it is why `curl` is the tool for this step.

You will also see a `Failed to determine the https port for redirect` warning from `UseHttpsRedirection`. That is expected in a container: there is no HTTPS port, because TLS terminates at the ingress. Requests still pass through.

Tear down with `docker compose down` (add `-v` to also drop the database volume).

### 4 — AI-assisted endpoint with critical review

Use Claude Code to generate a `GET /api/players/{id}/transactions` endpoint. Before committing anything, apply the Derivco AI review checklist:

- Is money stored as `decimal`, not `float` or `double`?
- Are all SQL parameters parameterised — no string interpolation?
- Are the correct HTTP status codes returned (200, 404)?
- Is there a null check when the player is not found?
- Does the query name specific columns, or does it use `SELECT *`?

Find and fix at least one issue. Write a test for the new endpoint. Then commit with a message that describes what you generated, what you found, and what you fixed.

## You're done when

- `dotnet test` still passes after all changes
- `docker compose up --build` starts both services and the API answers on port 8080
- `docker compose logs api` shows structured JSON log lines
- The baked-in `ENV Database__ConnectionString` line is gone from the `Dockerfile`, and the API still works
- You can explain every line of the `Dockerfile` without reading the comments
- The transactions endpoint returns the correct player's transactions and 404 for an unknown player

---

## Hints

Only read these if you are genuinely stuck.

**Serilog setup**

*Hint:* Call `builder.Host.UseSerilog(...)` before `builder.Build()`. Use `ReadFrom.Configuration(builder.Configuration)` to pick up log levels from `appsettings.json`. Use `WriteTo.Console(new JsonFormatter())` to get structured output.

**Setting the environment variable locally for task 2**

*Hint:* In zsh/bash, `export Database__ConnectionString="Server=localhost,1433;..."` before `dotnet run`. Confirm the negative case too: unset it and check the app fails in a way you understand.

**The API container cannot reach SQL Server**

*Hint:* Inside the compose network, services address each other by service name, not `localhost`. From the API container, SQL Server is at `sqlserver,1433`. `localhost` inside a container means *that* container.

**AI review — what to look for first**

*Hint:* Generated data access code most commonly gets wrong: `float`/`double` for money, `SELECT *` instead of named columns, and missing null checks for the not-found case. Check these three before anything else.
