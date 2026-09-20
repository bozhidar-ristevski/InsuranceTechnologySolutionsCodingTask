# Claims API

.NET 9 ASP.NET Core API for marine insurance **covers** and **claims**. Create covers, compute premium, attach claims, and delete records. Create and delete actions are audited in the background.

Solution: `Claims.sln`.

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- Docker Desktop (or another Docker daemon)

The API starts **SQL Server** and **MongoDB** with Testcontainers. Docker must be running before you start the app or the API tests.

## Run

```bash
dotnet run --project src/Claims.Api
```

Or open `Claims.sln` and run **Claims.Api**.

| | URL |
|---|---|
| HTTPS | https://localhost:7052 |
| HTTP | http://localhost:5052 |
| Swagger (Development) | https://localhost:7052/swagger |
| Hangfire dashboard | https://localhost:7052/hangfire (local requests only) |

## Solution layout

| Project | Role |
|---|---|
| `Claims.Api` | HTTP controllers, Swagger, ProblemDetails middleware |
| `Claims.Application` | Use cases, validation, DTOs |
| `Claims.Domain` | Cover / Claim entities and premium rules |
| `Claims.Infrastructure` | MongoDB, SQL Server, Hangfire |
| `Claims.Tests` | Unit and API tests |

**MongoDB** stores covers and claims. **SQL Server** stores audit rows and Hangfire jobs. **Hangfire** writes audits after the HTTP response path, so INSERT/DELETE of audit records does not block the request.

## API

JSON uses string enums (`Yacht`, `Collision`, …).

### Covers

| Method | Path | Description |
|---|---|---|
| `POST` | `/Covers/compute?startDate=&endDate=&coverType=` | Return premium; do not persist |
| `GET` | `/Covers` | List covers |
| `GET` | `/Covers/{id}` | Get one cover |
| `POST` | `/Covers` | Create cover (premium is computed and stored) |
| `DELETE` | `/Covers/{id}` | Delete cover |

```json
{
  "startDate": "2026-09-20",
  "endDate": "2026-10-20",
  "type": "Yacht"
}
```

`CoverType`: `Yacht`, `PassengerShip`, `ContainerShip`, `BulkCarrier`, `Tanker`.

### Claims

| Method | Path | Description |
|---|---|---|
| `GET` | `/Claims` | List claims |
| `GET` | `/Claims/{id}` | Get one claim |
| `POST` | `/Claims` | Create claim |
| `DELETE` | `/Claims/{id}` | Delete claim |

```json
{
  "coverId": "<existing-cover-id>",
  "created": "2026-09-25",
  "name": "Collision",
  "type": "Collision",
  "damageCost": 2500
}
```

`ClaimType`: `Collision`, `Grounding`, `BadWeather`, `Fire`.

## Validation

**Cover**

- `StartDate` cannot be in the past (UTC calendar date).
- `EndDate` must be after `StartDate`.
- Period cannot exceed one year.

**Claim**

- Cover must exist.
- `DamageCost` must be greater than 0 and at most 100,000.
- `Created` must fall within the cover period (inclusive dates).

## Premium

Base daily rate: **1250**.

| Cover type | Rate |
|---|---|
| Yacht | +10% |
| Passenger ship | +20% |
| Tanker | +50% |
| Other types | +30% |

Days are priced in bands:

1. First 30 days: full daily rate
2. Next 150 days: 5% off for Yacht, 2% off otherwise
3. Remaining days: extra 3% off for Yacht, extra 1% off otherwise

`POST /Covers` stores the result. `POST /Covers/compute` returns the same value without saving a cover.

## Errors

Responses use RFC 7807 Problem Details (`application/problem+json`).

| Situation | Status |
|---|---|
| Validation failed | 400 (`ValidationProblemDetails`) |
| Missing cover or claim | 404 |
| Unexpected failure | 500 (generic detail) |

```json
{
  "title": "Not Found",
  "status": 404,
  "detail": "Claim 'missing' was not found.",
  "instance": "/Claims/missing"
}
```

## Auditing

Create and delete of covers and claims enqueue Hangfire jobs. Each job inserts a `CoverAudit` or `ClaimAudit` (entity id, HTTP verb, timestamp) into SQL Server. Job status is on `/hangfire`.

## Tests

```bash
dotnet test Claims.sln
```

Includes premium calculation, cover/claim services with in-memory fakes, and HTTP tests against the hosted API.
