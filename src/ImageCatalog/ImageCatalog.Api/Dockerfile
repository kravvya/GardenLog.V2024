#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/ImageCatalog/ImageCatalog.Api/ImageCatalog.Api.csproj", "src/ImageCatalog/ImageCatalog.Api/"]
COPY ["src/GardenLog.SharedInfrastructure/GardenLog.SharedInfrastructure.csproj", "src/GardenLog.SharedInfrastructure/"]
COPY ["src/GardenLog.SharedKernel/GardenLog.SharedKernel.csproj", "src/GardenLog.SharedKernel/"]
COPY ["src/ImageCatalog/ImageCatalog.Contract/ImageCatalog.Contract.csproj", "src/ImageCatalog/ImageCatalog.Contract/"]
RUN dotnet restore "./src/ImageCatalog/ImageCatalog.Api/./ImageCatalog.Api.csproj"
COPY . .
WORKDIR "/src/src/ImageCatalog/ImageCatalog.Api"
RUN dotnet build "./ImageCatalog.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ImageCatalog.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ImageCatalog.Api.dll"]