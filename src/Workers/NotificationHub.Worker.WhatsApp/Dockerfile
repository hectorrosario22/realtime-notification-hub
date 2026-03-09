FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /repo

COPY ["src/NotificationHub.Domain/NotificationHub.Domain.csproj", "src/NotificationHub.Domain/"]
COPY ["src/NotificationHub.Application/NotificationHub.Application.csproj", "src/NotificationHub.Application/"]
COPY ["src/NotificationHub.Infrastructure/NotificationHub.Infrastructure.csproj", "src/NotificationHub.Infrastructure/"]
COPY ["src/Workers/NotificationHub.Worker.WhatsApp/NotificationHub.Worker.WhatsApp.csproj", "src/Workers/NotificationHub.Worker.WhatsApp/"]
RUN dotnet restore "src/Workers/NotificationHub.Worker.WhatsApp/NotificationHub.Worker.WhatsApp.csproj"

COPY . .
RUN dotnet publish "src/Workers/NotificationHub.Worker.WhatsApp/NotificationHub.Worker.WhatsApp.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "NotificationHub.Worker.WhatsApp.dll"]
