FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /repo

COPY ["src/NotificationHub.Domain/NotificationHub.Domain.csproj", "src/NotificationHub.Domain/"]
COPY ["src/NotificationHub.Application/NotificationHub.Application.csproj", "src/NotificationHub.Application/"]
COPY ["src/NotificationHub.Infrastructure/NotificationHub.Infrastructure.csproj", "src/NotificationHub.Infrastructure/"]
COPY ["src/Workers/NotificationHub.Worker.Sms/NotificationHub.Worker.Sms.csproj", "src/Workers/NotificationHub.Worker.Sms/"]
RUN dotnet restore "src/Workers/NotificationHub.Worker.Sms/NotificationHub.Worker.Sms.csproj"

COPY . .
RUN dotnet publish "src/Workers/NotificationHub.Worker.Sms/NotificationHub.Worker.Sms.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "NotificationHub.Worker.Sms.dll"]
