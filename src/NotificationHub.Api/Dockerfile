FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /repo

COPY ["src/NotificationHub.Domain/NotificationHub.Domain.csproj", "src/NotificationHub.Domain/"]
COPY ["src/NotificationHub.Application/NotificationHub.Application.csproj", "src/NotificationHub.Application/"]
COPY ["src/NotificationHub.Infrastructure/NotificationHub.Infrastructure.csproj", "src/NotificationHub.Infrastructure/"]
COPY ["src/NotificationHub.Api/NotificationHub.Api.csproj", "src/NotificationHub.Api/"]
RUN dotnet restore "src/NotificationHub.Api/NotificationHub.Api.csproj"

COPY . .
RUN dotnet publish "src/NotificationHub.Api/NotificationHub.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "NotificationHub.Api.dll"]
