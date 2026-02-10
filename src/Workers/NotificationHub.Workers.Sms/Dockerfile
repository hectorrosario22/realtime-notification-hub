# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["Workers/NotificationHub.Workers.Sms/NotificationHub.Workers.Sms.csproj", "Workers/NotificationHub.Workers.Sms/"]
RUN dotnet restore "Workers/NotificationHub.Workers.Sms/NotificationHub.Workers.Sms.csproj"

# Copy source code and build
COPY ["Workers/NotificationHub.Workers.Sms/", "Workers/NotificationHub.Workers.Sms/"]
WORKDIR "/src/Workers/NotificationHub.Workers.Sms"
RUN dotnet build "NotificationHub.Workers.Sms.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "NotificationHub.Workers.Sms.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/runtime:10.0-alpine AS final
WORKDIR /app

# Create non-root user
RUN addgroup -g 1000 appuser && adduser -u 1000 -G appuser -s /bin/sh -D appuser
USER appuser

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "NotificationHub.Workers.Sms.dll"]
