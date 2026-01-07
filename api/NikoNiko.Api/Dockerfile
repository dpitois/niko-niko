# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy the entire api directory content to /src
COPY api/ .

# Restore dependencies using the solution file now at /src/nikoniko.slnx
RUN dotnet restore "nikoniko.slnx"

# Build the project
RUN dotnet build "NikoNiko.Api/NikoNiko.Api.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "NikoNiko.Api/NikoNiko.Api.csproj" -c Release -o /app/publish

# Stage 3: Final
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Install curl for healthcheck
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "NikoNiko.Api.dll"]
