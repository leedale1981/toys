# CQRS + Persistence + Seq/OpenTelemetry Setup

This repository now includes:

- `data-interfaces/LD.Messaging.Data.Interfaces` - shared CQRS interfaces and stock-record query/command contracts.
- `infrastructure/LD.Messaging.Infrastructure.Persistence` - PostgreSQL-backed implementations of those interfaces.
- API endpoint wiring for querying persisted stock records via CQRS.
- Serilog logging to Seq in API, ingestion, and file-service.
- OpenTelemetry trace/metric export to Seq over OTLP HTTP.
- Docker Compose stack for PostgreSQL, Seq, Kafka, API, ingestion service, and file service.

## Projects and contracts

- Query contract: `GetStockRecordsQuery`
- Query handler interface: `IQueryHandler<TQuery, TResult>`
- Command contract: `SaveStockRecordsCommand`
- Command handler interface: `ICommandHandler<TCommand>`

Persistence registrations are in:

- `infrastructure/LD.Messaging.Infrastructure.Persistence/ServiceCollectionExtensions.cs`

## Run locally (Docker)

```powershell
docker compose up --build -d
```

## Key URLs

- Seq UI: `http://localhost:5341`
- API: `http://localhost:8081/api/stockrecords`
- Kafka UI: `http://localhost:8080`

## Example API call

```powershell
curl "http://localhost:8081/api/stockrecords?exchange=NYSE&pageSize=50&pageOffset=0"
```

## Local verification (already run)

- `dotnet build LD.Messaging.slnx`
- `dotnet test LD.Messaging.slnx --no-build`

## Notes

- OTLP HTTP endpoints used for Seq:
  - Traces: `/ingest/otlp/v1/traces`
  - Metrics: `/ingest/otlp/v1/metrics`
- Current OpenTelemetry package versions report known moderate CVEs during restore; update package versions when you are ready to remediate.

