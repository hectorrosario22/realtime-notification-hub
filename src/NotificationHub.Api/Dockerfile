# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["NotificationHub.Api/NotificationHub.Api.csproj", "NotificationHub.Api/"]
RUN dotnet restore "NotificationHub.Api/NotificationHub.Api.csproj"

# Copy source code and build
COPY ["NotificationHub.Api/", "NotificationHub.Api/"]
WORKDIR "/src/NotificationHub.Api"
RUN dotnet build "NotificationHub.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "NotificationHub.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS final
WORKDIR /app

# Create non-root user
RUN addgroup -g 1000 appuser && adduser -u 1000 -G appuser -s /bin/sh -D appuser
USER appuser

COPY --from=publish /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "NotificationHub.Api.dll"]
