FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props ./
COPY src/Northstar.RetailCatalog.Domain/Northstar.RetailCatalog.Domain.csproj src/Northstar.RetailCatalog.Domain/
COPY src/Northstar.RetailCatalog.Application/Northstar.RetailCatalog.Application.csproj src/Northstar.RetailCatalog.Application/
COPY src/Northstar.RetailCatalog.Infrastructure/Northstar.RetailCatalog.Infrastructure.csproj src/Northstar.RetailCatalog.Infrastructure/
COPY src/Northstar.RetailCatalog.Api/Northstar.RetailCatalog.Api.csproj src/Northstar.RetailCatalog.Api/
RUN dotnet restore src/Northstar.RetailCatalog.Api/Northstar.RetailCatalog.Api.csproj

COPY src/ src/
RUN dotnet publish src/Northstar.RetailCatalog.Api/Northstar.RetailCatalog.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "Northstar.RetailCatalog.Api.dll"]
