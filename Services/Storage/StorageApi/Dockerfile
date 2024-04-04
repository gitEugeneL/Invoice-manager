FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Services/Storage/StorageApi/StorageApi.csproj", "Services/Storage/StorageApi/"]
COPY ["Services/Contracts/Contracts.csproj", "Services/Contracts/"]
RUN dotnet restore "./Services/Storage/StorageApi/StorageApi.csproj"

COPY . .
WORKDIR "/src/Services/Storage/StorageApi"
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS runtime
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "StorageApi.dll"]