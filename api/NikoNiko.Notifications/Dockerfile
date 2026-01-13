# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

# Copy project files for restoration
COPY ["api/nikoniko.slnx", "api/"]
COPY ["api/NikoNiko.Api/NikoNiko.Api.csproj", "api/NikoNiko.Api/"]
COPY ["api/NikoNiko.Core/NikoNiko.Core.csproj", "api/NikoNiko.Core/"]
COPY ["api/NikoNiko.Data/NikoNiko.Data.csproj", "api/NikoNiko.Data/"]
COPY ["api/NikoNiko.Services/NikoNiko.Services.csproj", "api/NikoNiko.Services/"]
COPY ["api/NikoNiko.Data.PostgreSql/NikoNiko.Data.PostgreSql.csproj", "api/NikoNiko.Data.PostgreSql/"]
COPY ["api/NikoNiko.Data.Sqlite/NikoNiko.Data.Sqlite.csproj", "api/NikoNiko.Data.Sqlite/"]
COPY ["api/NikoNiko.Notifications/NikoNiko.Notifications.csproj", "api/NikoNiko.Notifications/"]
COPY ["api/NikoNiko.Api.IntegrationTests/NikoNiko.Api.IntegrationTests.csproj", "api/NikoNiko.Api.IntegrationTests/"]
COPY ["api/NikoNiko.Notifications.IntegrationTests/NikoNiko.Notifications.IntegrationTests.csproj", "api/NikoNiko.Notifications.IntegrationTests/"]

# Restore dependencies
RUN dotnet restore "api/nikoniko.slnx"

# Copy the rest of the source code
COPY api/ api/

# Build and publish the Notifications service
WORKDIR "/src/api/NikoNiko.Notifications"
RUN dotnet publish "NikoNiko.Notifications.csproj" -c Release -o /app/publish --no-restore

# Stage 2: Final
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS final
WORKDIR /app

# Install icu-libs for globalization support
RUN apk add --no-cache icu-libs curl
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "NikoNiko.Notifications.dll"]