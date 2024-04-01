FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Services/FileGenerator/FileGeneratorApi/FileGeneratorApi.csproj", "Services/FileGenerator/FileGeneratorApi/"]
COPY ["Services/Contracts/Contracts.csproj", "Services/Contracts/"]
RUN dotnet restore "./Services/FileGenerator/FileGeneratorApi/FileGeneratorApi.csproj"

COPY . .
WORKDIR "/src/Services/FileGenerator/FileGeneratorApi"
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS runtime
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FileGeneratorApi.dll"]
