#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["NSSLServer/NSSLServer.Runtime.csproj", "NSSLServer/"]
COPY ["NSSLServer.Database/NSSLServer.Database.csproj", "NSSLServer.Database/"]
COPY ["NSSLServer.Core/NSSLServer.Core.csproj", "NSSLServer.Core/"]
COPY ["NSSLServer.Shared/NSSLServer.Shared.csproj", "NSSLServer.Shared/"]
COPY ["lib/querybuilder/src/Deviax.QueryBuilder.Postgres/Deviax.QueryBuilder.Postgres.csproj", "lib/querybuilder/src/Deviax.QueryBuilder.Postgres/"]
RUN dotnet restore "NSSLServer/NSSLServer.Runtime.csproj"
COPY . .
WORKDIR "/src/NSSLServer"
RUN dotnet build "NSSLServer.Runtime.csproj" -c Release -o /app/build /p:"IsDockerBuild=true"

FROM build AS publish
RUN dotnet publish "NSSLServer.Runtime.csproj" -c Release -o /app/publish /p:"IsDockerBuild=true"

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NSSLServer.dll", "--server.urls", "http://+:80"]