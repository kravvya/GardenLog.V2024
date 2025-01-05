#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER app
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/UserManagement/UserManagement.Api/UserManagement.Api.csproj", "src/UserManagement/UserManagement.Api/"]
COPY ["src/GardenLog.SharedInfrastructure/GardenLog.SharedInfrastructure.csproj", "src/GardenLog.SharedInfrastructure/"]
COPY ["src/GardenLog.SharedKernel/GardenLog.SharedKernel.csproj", "src/GardenLog.SharedKernel/"]
COPY ["src/PlantHarvest/PlantHarvest.Contract/PlantHarvest.Contract.csproj", "src/PlantHarvest/PlantHarvest.Contract/"]
COPY ["src/UserManagement/UserManagement.Contract/UserManagement.Contract.csproj", "src/UserManagement/UserManagement.Contract/"]
RUN dotnet restore "./src/UserManagement/UserManagement.Api/./UserManagement.Api.csproj"
COPY . .
WORKDIR "/src/src/UserManagement/UserManagement.Api"
RUN dotnet build "./UserManagement.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./UserManagement.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UserManagement.Api.dll"]