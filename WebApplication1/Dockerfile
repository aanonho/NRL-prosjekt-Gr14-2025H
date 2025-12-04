# Norsk oversikt: Dockerfile beskriver hvordan appen bygges og kjøres likt i utvikling, test og produksjon.
# Dette stadiet brukes av Visual Studio i rask debug-modus for å starte API-et fort med minste mulig image.
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Byggesteg som gjenbruker SDK-bildet for å restore avhengigheter, kompilere og forberede artefakter.
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["WebApplication1/WebApplication1.csproj", "WebApplication1/"]
RUN dotnet restore "./WebApplication1/WebApplication1.csproj"
COPY . .
WORKDIR "/src/WebApplication1"
# Tar med appsettings slik at byggeprosessen har riktige konfigurasjoner tilgjengelig under kompilering.
COPY appsettings*.json ./
RUN dotnet build "./WebApplication1.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publiserer ferdig kompilert kode til en mappe som kan kopieres til det slanke runtime-bildet.
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./WebApplication1.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Endelig runtime-bilde som kjører appen i produksjon eller vanlig modus uten SDK-overhead.
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WebApplication1.dll"]