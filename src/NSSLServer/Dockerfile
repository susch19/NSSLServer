FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-stretch-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build

COPY ["NSSLServer/NSSLServer.csproj", "NSSLServer/"]
COPY ["NSSLServer/NuGet.Config", "NSSLServer/"]
COPY ["Firebase/Firebase.csproj", "Firebase/"]
COPY ["NSSLServer.Shared/NSSLServer.Shared.csproj", "NSSLServer.Shared/"]
COPY ["lib/querybuilder/src/Deviax.QueryBuilder.Postgres/Deviax.QueryBuilder.Postgres.csproj", "lib/querybuilder/Deviax.QueryBuilder.Postgres/"]
RUN dotnet restore "NSSLServer/NSSLServer.csproj"
COPY . .
WORKDIR "/NSSLServer"
RUN dotnet build "NSSLServer.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "NSSLServer.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "NSSLServer.dll"]