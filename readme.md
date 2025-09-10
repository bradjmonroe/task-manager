# TaskTracker

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)


_A minimal .NET sample that demonstrates a clean application structure with JWT auth and EF Core with SQLite._

**Stack**: 
- .NET 9
-  ASP.NET Core Web API (Controllers)
-  Razor Pages Web UI • EF Core 9 (SQLite)
-  JWT Bearer
-  OpenAPI (built‑in) + Scalar UI

---

## Purpose of this project?

- To showcase **full‑stack** skills with a small, reviewable codebase
- Clean solution split: **Data** (DbContext + models + migrations), **API** (auth + endpoints), **Web** (Razor UI calling the API)
- Real‑world bits: **hashed passwords**, **JWT**, **per‑user data** (FK via `CreatedBy`), **migrations**, **CORS**

---

## Solution layout

```
TaskTracker.sln
TaskTracker.Data/        # EF Core models, TasksDb, migrations
TaskTracker.Api/         # Controllers, JWT auth, OpenAPI + Scalar, CORS
TaskTracker.Web/         # Razor Pages UI; typed HttpClient; session‑stored JWT
```

---

## Requirements

- .NET SDK **9.0**
- (Optional) SQLite tooling (DB file is created automatically)

---

## Quick start

```bash
# 1) Restore
 dotnet restore

# 2) Configure API secrets
 dotnet user-secrets init --project TaskTracker.Api
 dotnet user-secrets set "Jwt:Key" "<long-random-secret>" --project TaskTracker.Api

# 3) Ensure DB is up to date
 dotnet ef database update --project TaskTracker.Data --startup-project TaskTracker.Api

# 4) Run API & Web (separate terminals)
 dotnet run --project TaskTracker.Api
 dotnet run --project TaskTracker.Web
```

**Default URLs**:

- API: e.g. `http://localhost:5080`
- API Docs (Scalar): `http://localhost:5080/scalar`  
  OpenAPI JSON: `http://localhost:5080/openapi/v1.json`
- Web UI: typically `https://localhost:7193` (or `http://localhost:5193`)

> If CORS blocks requests, make sure the API `Cors:AllowedOrigins` contains your Web URL(s).

---

## Configuration

### API: `TaskTracker.Api/appsettings.json`

```json
{
  "ConnectionStrings": { "Default": "Data Source=tasktracker.db" },
  "Jwt": {
    "Issuer": "TaskTracker",
    "Audience": "TaskTracker"
    // Key is set via user-secrets: dotnet user-secrets set "Jwt:Key" "..."
  },
  "Cors": {
    "AllowedOrigins": [
      "https://localhost:7193",
      "http://localhost:5193"
    ]
  },
  "Logging": { 
    "LogLevel": { 
        "Default": "Information", 
        "Microsoft.AspNetCore": "Warning" 
        } 
    },
  "AllowedHosts": "*"
}
```

### Web: `TaskTracker.Web/appsettings.Development.json`

```json
{ "Api": { 
    "BaseUrl": "http://localhost:5080" 
    } 
}
```

---

## API overview

**Auth**

- `POST /api/auth/register` — Create account; returns `{ token, userId, email }`
- `POST /api/auth/login` — Returns `{ token, userId, email }`

**Tasks** (require `Authorization: Bearer <token>`)

- `GET /api/tasks` — List current user’s tasks
- `POST /api/tasks` — Body `{ "title": "string" }`
- `PUT /api/tasks/{id}/toggle` — Toggle completion

### Curl demo

```bash
# Register
curl -s -X POST http://localhost:5080/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email":"demo@example.com","password":"P@ssw0rd!"}'

# Login 
TOKEN=$(curl -s -X POST http://localhost:5080/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"demo@example.com","password":"P@ssw0rd!"}' | jq -r .token)

# Create a task
curl -s -X POST http://localhost:5080/api/tasks \
  -H "Authorization: Bearer $TOKEN" -H "Content-Type: application/json" \
  -d '{"title":"Ship demo"}'

# List tasks
curl -s http://localhost:5080/api/tasks -H "Authorization: Bearer $TOKEN"
```

---

## Data model

- `AppUser` — `Id (Guid)`, `Email (unique)`, `PasswordHash`, `CreatedOn`
- `TaskItem` — `Id`, `Title`, `IsDone`, `CreatedOn`, `CreatedBy (Guid FK -> AppUser)`

EF Core configuration lives in `TaskTracker.Data/TasksDb.cs`. Migrations are in `TaskTracker.Data/Migrations/` (and are committed).

---

## Design notes

- **Separation**: API does not reference Razor; Web does not reference EF or Data directly.
- **Auth**: JWT signed with HMAC; token carries `sub`/`email`/`nameidentifier` claims.
- **Scoping Per User**: API filters tasks by `CreatedBy` from token.
- **Migrations**: API applies `Database.Migrate()` at startup for easy local runs.
- **CORS**: Restricted to Web origin(s) via policy `web`.

---

## Development tips

- Rotate JWT key via user‑secrets:

  ```bash
  dotnet user-secrets set "Jwt:Key" "$(openssl rand -base64 48)" --project TaskTracker.Api
  ```

- Reset local DB: stop API, delete `tasktracker.db` (and `*.db-wal`, `*.db-shm`), rerun API.
- Add a new migration when models change:

  ```bash
  dotnet ef migrations add <Name> --project TaskTracker.Data --startup-project TaskTracker.Api
  dotnet ef database update --project TaskTracker.Data --startup-project TaskTracker.Api
  ```

---

## License

This project is licensed under the **MIT License**.  
See the [LICENSE](LICENSE) file for full text.

© 2025 Brad Monroe

---

## Credits

- ASP.NET Core & EF Core team docs
- Scalar UI for pretty OpenAPI docs

Test again