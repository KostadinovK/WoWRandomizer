# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.
# NOTE: the build context is the repository root (see DockerfileContext in the csproj), e.g.
#   docker build -f WoWRandomizer.API/Dockerfile .

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["WoWRandomizer.API/WoWRandomizer.API.csproj", "WoWRandomizer.API/"]
COPY ["WoWRandomizer.Domain/WoWRandomizer.Domain.csproj", "WoWRandomizer.Domain/"]
COPY ["WoWRandomizer.Infrastructure/WoWRandomizer.Infrastructure.csproj", "WoWRandomizer.Infrastructure/"]
COPY ["WoWRandomizer.Shared/WoWRandomizer.Shared.csproj", "WoWRandomizer.Shared/"]
RUN dotnet restore "./WoWRandomizer.API/WoWRandomizer.API.csproj"
COPY . .
WORKDIR "/src/WoWRandomizer.API"
RUN dotnet build "./WoWRandomizer.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./WoWRandomizer.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WoWRandomizer.API.dll"]
