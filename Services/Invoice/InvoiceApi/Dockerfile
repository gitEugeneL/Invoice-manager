﻿FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Services/Invoice/InvoiceApi/InvoiceApi.csproj", "Services/Invoice/InvoiceApi/"]
COPY ["Services/Contracts/Contracts.csproj", "Services/Contracts/"]
RUN dotnet restore "./Services/Invoice/InvoiceApi/InvoiceApi.csproj"

COPY . .
WORKDIR "/src/Services/Invoice/InvoiceApi"
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS runtime
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "InvoiceApi.dll"]