#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/PlantCatalog/PlantCatalog.Api/PlantCatalog.Api.csproj", "src/PlantCatalog/PlantCatalog.Api/"]
COPY ["src/GardenLog.SharedInfrastructure/GardenLog.SharedInfrastructure.csproj", "src/GardenLog.SharedInfrastructure/"]
COPY ["src/GardenLog.SharedKernel/GardenLog.SharedKernel.csproj", "src/GardenLog.SharedKernel/"]
COPY ["src/PlantCatalog/PlantCatalog.Contract/PlantCatalog.Contract.csproj", "src/PlantCatalog/PlantCatalog.Contract/"]
COPY ["src/PlantCatalog/PlantCatalog.Infrustructure/PlantCatalog.Infrustructure.csproj", "src/PlantCatalog/PlantCatalog.Infrustructure/"]
COPY ["src/PlantCatalog/PlantCatalog.Domain/PlantCatalog.Domain.csproj", "src/PlantCatalog/PlantCatalog.Domain/"]
RUN dotnet restore "./src/PlantCatalog/PlantCatalog.Api/./PlantCatalog.Api.csproj"
COPY . .
WORKDIR "/src/src/PlantCatalog/PlantCatalog.Api"
RUN dotnet build "./PlantCatalog.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./PlantCatalog.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PlantCatalog.Api.dll"]