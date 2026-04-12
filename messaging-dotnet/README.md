# LD.Messaging

An event-driven stock market data pipeline built with .NET, MassTransit, and Apache Kafka. The system demonstrates how to decouple file-based data ingestion from downstream processing using durable, replayable messaging — turning a brittle batch-file workflow into a resilient, independently scalable pipeline.

---

## The Problem It Solves

Many financial and enterprise systems still exchange data through flat files dropped into shared directories. The naive approach is to poll those directories and process each file synchronously, coupling the reader directly to every downstream consumer. This creates several problems:

- **Tight coupling** — adding a new consumer (e.g. a risk engine, a reporting service) requires modifying the reader.
- **Brittle failure modes** — if a downstream system is down, the reader either blocks or silently drops data.
- **No replay** — once a file is processed, the data is gone; there is no way to reprocess it for a new consumer that joins later.
- **Scaling bottlenecks** — the reader and all its consumers must scale together.

This project solves those problems by inserting an event bus between the file reader and all downstream consumers. The file service parses each incoming file and publishes a strongly typed domain event to a Kafka topic. Consumers subscribe independently, at their own pace, and Kafka retains the full message history for replay.

---

## Architecture

```
┌─────────────────────┐        writes files        ┌─────────────────────┐
│   StockGenerator    │ ─────────────────────────► │    FileService      │
│  (synthetic data)   │                             │  (FileMonitor)      │
└─────────────────────┘                             └────────┬────────────┘
                                                             │
                                               MassTransit Kafka Rider
                                               (ITopicProducer<T>)
                                                             │
                              ┌──────────────────────────────▼──────────────────────────────┐
                              │                     Apache Kafka                             │
                              │   topic: FTSE500   │   topic: NYSE   │   topic: NASDAQ      │
                              └──────────────────────────────┬──────────────────────────────┘
                                                             │
                                               MassTransit Kafka Rider
                                               (IConsumer<T> endpoints)
                                                             │
                                                  ┌──────────▼──────────┐
                                                  │  IngestionService   │
                                                  │  (3 consumers)      │
                                                  └─────────────────────┘
```

### Event-Driven Flow

1. **StockGenerator** writes synthetic stock data files (one per exchange per tick) into a watched directory. In production this role is played by a real market data feed or an upstream ETL job.
2. **FileService** watches the directory with a `FileSystemWatcher`. When a file appears it is parsed through a railway-oriented `Checked<T>` validation pipeline. A valid parse produces a `StockMarketData` domain object which is published to the appropriate Kafka topic via a MassTransit `ITopicProducer<T>`.
3. **Kafka** acts as the durable event bus. Three topics mirror the three exchanges: `FTSE500`, `NYSE`, `NASDAQ`. Kafka decouples producers from consumers in time — a consumer can restart and replay from any offset.
4. **IngestionService** subscribes to all three topics. Each exchange has a dedicated `IConsumer<T>` implementation, configured through the MassTransit Kafka Rider. The service currently logs received records; in a production system each consumer would persist to a database, update a pricing engine, or forward to an analytics stream.

### Why MassTransit?

MassTransit provides a technology-agnostic messaging abstraction over Kafka (and many other transports). Publishing a `Ftse500Data` message looks identical whether the transport is Kafka, RabbitMQ, or Azure Service Bus. The Kafka Rider adds topic-level routing, consumer-group management, and offset control while keeping application code free of Kafka SDK details.

### Validation Pipeline (`Checked<T>`)

Parsing is modelled as a railway-oriented pipeline of `Checked<T>` monadic checks. Each phase validates one concern (exchange, date, column headers, record values). The first failing phase short-circuits the chain and surfaces a precise error message — no exceptions are thrown for expected data quality problems.

---

## Projects

| Folder | Project | Role |
|---|---|---|
| `domain/` | `LD.Messaging.Domain` | Shared domain model: `StockMarketData`, `StockRecord`, `Exchange`, `Checked<T>` monad, message contracts (`Ftse500Data`, `NyseData`, `NasdaqData`) |
| `adapter/` | `LD.Messaging.Adapter` | File parsing: `StockFileParser` (validation pipeline), `StockFileAdapter` (file I/O wrapper) |
| `adapter-tests/` | `LD.Messaging.Adapter.Tests` | Unit tests for the parser and adapter |
| `file-service/` | `LD.Messaging.FileService` | `FileMonitorService` watches a directory; `StockPublisher` routes parsed data to Kafka via MassTransit |
| `ingestion-service/` | `LD.Messaging.Ingestion` | Kafka consumers (`Ftse500Consumer`, `NyseConsumer`, `NasdaqConsumer`) that receive and process stock data events |
| `stock-generator/` | `LD.Messaging.StockGenerator` | `FileGeneratorService` writes synthetic stock files on a configurable interval for local development and load testing |
| `file-service-benchmarks/` | `LD.Messaging.FileService.Benchmarks` | BenchmarkDotNet suite measuring the adapter parsing pipeline at small, medium, and large payloads, with and without disk I/O |

---

## Getting Started

### Prerequisites

- [.NET 11 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)

### 1. Start Kafka

```bash
docker compose up -d
```

This starts Zookeeper, a single-broker Kafka instance, auto-creates the three topics (`FTSE500`, `NYSE`, `NASDAQ`), and exposes the Kafka UI at [http://localhost:8080](http://localhost:8080).

### 2. Run the services

Open three terminals from the repo root:

```bash
# Terminal 1 — file service (watches for incoming files)
dotnet run --project file-service/LD.Messaging.FileService

# Terminal 2 — ingestion service (consumes from Kafka)
dotnet run --project ingestion-service/LD.Messaging.Ingestion

# Terminal 3 — stock generator (writes synthetic files)
dotnet run --project stock-generator/LD.Messaging.StockGenerator
```

The generator writes one file per exchange per second into `generated-data/`. The file service picks each file up, parses it, and publishes to Kafka. The ingestion service logs the received records.

Point both services at the same directory by aligning their config:

```jsonc
// file-service appsettings.json
{ "FileMonitor": { "WatchPath": "generated-data" } }

// stock-generator appsettings.json
{ "Generator": { "OutputPath": "generated-data", "IntervalMs": "1000", "RecordCount": "10" } }
```

### 3. Run the benchmarks

```bash
dotnet run -c Release --project file-service-benchmarks/LD.Messaging.FileService.Benchmarks
```

Results are written to `BenchmarkDotNet.Artifacts/` and include throughput, mean latency, and memory allocations for the parsing pipeline.

---

## File Format

```
EXCHANGE:FTSE500
DATE:2026-04-12
TIME:09:30:00
---
SYMBOL,NAME,OPEN,HIGH,LOW,CLOSE,VOLUME,CHANGE_PCT
BP.L,BP PLC,450.20,455.10,449.80,453.60,5234567,0.75
HSBA.L,HSBC Holdings,625.10,630.20,622.40,628.90,8765432,-0.32
```

The parser validates every field and produces a precise error message for each violation. Invalid files are logged and discarded; the pipeline continues processing subsequent files.
