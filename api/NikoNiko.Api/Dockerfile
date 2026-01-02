# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copie le fichier solution
COPY api/nikoniko.slnx .

# Crée le répertoire 'api' et ses sous-répertoires pour correspondre à la structure de la solution
RUN mkdir -p api/NikoNiko.Api \
           api/NikoNiko.Core \
           api/NikoNiko.Services \
           api/NikoNiko.Data \
           api/NikoNiko.Data.Sqlite \
           api/NikoNiko.Data.PostgreSql \
           api/NikoNiko.Notifications \
           api/NikoNiko.Notifications.IntegrationTests

# Copie les fichiers .csproj vers leurs emplacements corrects
COPY api/NikoNiko.Api/*.csproj ./api/NikoNiko.Api/
COPY api/NikoNiko.Core/*.csproj ./api/NikoNiko.Core/
COPY api/NikoNiko.Services/*.csproj ./api/NikoNiko.Services/
COPY api/NikoNiko.Data/*.csproj ./api/NikoNiko.Data/
COPY api/NikoNiko.Data.Sqlite/*.csproj ./api/NikoNiko.Data.Sqlite/
COPY api/NikoNiko.Data.PostgreSql/*.csproj ./api/NikoNiko.Data.PostgreSql/
COPY api/NikoNiko.Notifications/*.csproj ./api/NikoNiko.Notifications/
COPY api/NikoNiko.Notifications.IntegrationTests/*.csproj ./api/NikoNiko.Notifications.IntegrationTests/


# Restaure toutes les dépendances
RUN dotnet restore "nikoniko.slnx"

# Copie le reste des fichiers source
COPY api ./api

# Construit le projet NikoNiko.Api
RUN dotnet build "api/NikoNiko.Api/NikoNiko.Api.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "api/NikoNiko.Api/NikoNiko.Api.csproj" -c Release -o /app/publish

# Stage 3: Final
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "NikoNiko.Api.dll"]
