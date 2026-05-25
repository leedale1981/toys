FROM mcr.microsoft.com/dotnet/nightly/runtime:11.0-preview AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/nightly/sdk:11.0-preview AS build
WORKDIR /src

COPY ["file-service/LD.Messaging.FileService/LD.Messaging.FileService.csproj", "file-service/LD.Messaging.FileService/"]
COPY ["domain/LD.Messaging.Domain/LD.Messaging.Domain.csproj", "domain/LD.Messaging.Domain/"]
COPY ["adapter/LD.Messaging.Adapter/LD.Messaging.Adapter.csproj", "adapter/LD.Messaging.Adapter/"]
RUN dotnet restore "file-service/LD.Messaging.FileService/LD.Messaging.FileService.csproj"

COPY . .

WORKDIR "/src/file-service/LD.Messaging.FileService"
RUN dotnet build "LD.Messaging.FileService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LD.Messaging.FileService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LD.Messaging.FileService.dll"]

