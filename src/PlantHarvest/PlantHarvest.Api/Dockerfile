#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER app
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/PlantHarvest/PlantHarvest.Api/PlantHarvest.Api.csproj", "src/PlantHarvest/PlantHarvest.Api/"]
COPY ["src/GardenLog.SharedInfrastructure/GardenLog.SharedInfrastructure.csproj", "src/GardenLog.SharedInfrastructure/"]
COPY ["src/GardenLog.SharedKernel/GardenLog.SharedKernel.csproj", "src/GardenLog.SharedKernel/"]
COPY ["src/PlantHarvest/PlantHarvest.Contract/PlantHarvest.Contract.csproj", "src/PlantHarvest/PlantHarvest.Contract/"]
COPY ["src/PlantHarvest/PlantHarvest.Domain/PlantHarvest.Domain.csproj", "src/PlantHarvest/PlantHarvest.Domain/"]
COPY ["src/PlantHarvest/PlantHarvest.Infrastructure/PlantHarvest.Infrastructure.csproj", "src/PlantHarvest/PlantHarvest.Infrastructure/"]
COPY ["src/PlantCatalog/PlantCatalog.Contract/PlantCatalog.Contract.csproj", "src/PlantCatalog/PlantCatalog.Contract/"]
COPY ["src/UserManagement/UserManagement.Contract/UserManagement.Contract.csproj", "src/UserManagement/UserManagement.Contract/"]
RUN dotnet restore "./src/PlantHarvest/PlantHarvest.Api/./PlantHarvest.Api.csproj"
COPY . .
WORKDIR "/src/src/PlantHarvest/PlantHarvest.Api"
RUN dotnet build "./PlantHarvest.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./PlantHarvest.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PlantHarvest.Api.dll"]