#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["dapper-pubsub-test.csproj", ""]
RUN dotnet restore "./dapper-pubsub-test.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "dapper-pubsub-test.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "dapper-pubsub-test.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "dapper-pubsub-test.dll"]