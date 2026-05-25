FROM mcr.microsoft.com/dotnet/nightly/aspnet:11.0-preview AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/nightly/sdk:11.0-preview AS build
WORKDIR /src

# Copy csproj files and restore (layer-cached)
COPY ["api/LD.Messaging.Api/LD.Messaging.Api.csproj", "api/LD.Messaging.Api/"]
COPY ["domain/LD.Messaging.Domain/LD.Messaging.Domain.csproj", "domain/LD.Messaging.Domain/"]
COPY ["data-interfaces/LD.Messaging.Data.Interfaces/LD.Messaging.Data.Interfaces.csproj", "data-interfaces/LD.Messaging.Data.Interfaces/"]
COPY ["infrastructure/LD.Messaging.Infrastructure.Persistence/LD.Messaging.Infrastructure.Persistence.csproj", "infrastructure/LD.Messaging.Infrastructure.Persistence/"]
RUN dotnet restore "api/LD.Messaging.Api/LD.Messaging.Api.csproj"

COPY . .

WORKDIR "/src/api/LD.Messaging.Api"
RUN dotnet build "LD.Messaging.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LD.Messaging.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LD.Messaging.Api.dll"]

