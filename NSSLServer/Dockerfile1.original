#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /src
COPY ["NSSLServer/NSSLServer.csproj", "NSSLServer/"]
COPY ["NSSLServer/NuGet.Config", "NSSLServer/"]
COPY ["NSSLServer.Shared/NSSLServer.Shared.csproj", "NSSLServer.Shared/"]
COPY ["lib/querybuilder/src/Deviax.QueryBuilder.Postgres/Deviax.QueryBuilder.Postgres.csproj", "lib/querybuilder/src/Deviax.QueryBuilder.Postgres/"]
RUN dotnet restore "NSSLServer/NSSLServer.csproj"
COPY . .
WORKDIR "/src/NSSLServer"
RUN dotnet build "NSSLServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NSSLServer.csproj" -c Release -o /app/publish

ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80/tcp

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NSSLServer.dll", "--server.urls", "http://+:80"]