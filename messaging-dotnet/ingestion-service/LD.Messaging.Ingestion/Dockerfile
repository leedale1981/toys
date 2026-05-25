FROM mcr.microsoft.com/dotnet/nightly/runtime:11.0-preview AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/nightly/sdk:11.0-preview AS build
WORKDIR /src

COPY ["ingestion-service/LD.Messaging.Ingestion/LD.Messaging.Ingestion.csproj", "ingestion-service/LD.Messaging.Ingestion/"]
COPY ["domain/LD.Messaging.Domain/LD.Messaging.Domain.csproj", "domain/LD.Messaging.Domain/"]
COPY ["data-interfaces/LD.Messaging.Data.Interfaces/LD.Messaging.Data.Interfaces.csproj", "data-interfaces/LD.Messaging.Data.Interfaces/"]
COPY ["infrastructure/LD.Messaging.Infrastructure.Persistence/LD.Messaging.Infrastructure.Persistence.csproj", "infrastructure/LD.Messaging.Infrastructure.Persistence/"]
RUN dotnet restore "ingestion-service/LD.Messaging.Ingestion/LD.Messaging.Ingestion.csproj"

COPY . .

WORKDIR "/src/ingestion-service/LD.Messaging.Ingestion"
RUN dotnet build "LD.Messaging.Ingestion.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LD.Messaging.Ingestion.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LD.Messaging.Ingestion.dll"]

