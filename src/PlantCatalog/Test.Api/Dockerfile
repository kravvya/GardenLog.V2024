#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/PlantCatalog/Test.Api/Test.Api.csproj", "src/PlantCatalog/Test.Api/"]
RUN dotnet restore "./src/PlantCatalog/Test.Api/./Test.Api.csproj"
COPY . .
WORKDIR "/src/src/PlantCatalog/Test.Api"
RUN dotnet build "./Test.Api.csproj" -c Release  -o /app/build

FROM build AS publish
RUN dotnet publish "./Test.Api.csproj" -c Release  -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Test.Api.dll"]


#https://stackoverflow.com/questions/77781040/how-to-get-azure-container-apps-to-accept-the-new-net-8-default-http-port-8080
#https://stackoverflow.com/questions/77753907/socketexception-permission-denied-when-trying-to-bind-port-80-or-443-to-kestrel