# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["Workers/NotificationHub.Workers.WhatsApp/NotificationHub.Workers.WhatsApp.csproj", "Workers/NotificationHub.Workers.WhatsApp/"]
RUN dotnet restore "Workers/NotificationHub.Workers.WhatsApp/NotificationHub.Workers.WhatsApp.csproj"

# Copy source code and build
COPY ["Workers/NotificationHub.Workers.WhatsApp/", "Workers/NotificationHub.Workers.WhatsApp/"]
WORKDIR "/src/Workers/NotificationHub.Workers.WhatsApp"
RUN dotnet build "NotificationHub.Workers.WhatsApp.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "NotificationHub.Workers.WhatsApp.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/runtime:10.0-alpine AS final
WORKDIR /app

# Create non-root user
RUN addgroup -g 1000 appuser && adduser -u 1000 -G appuser -s /bin/sh -D appuser
USER appuser

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "NotificationHub.Workers.WhatsApp.dll"]
