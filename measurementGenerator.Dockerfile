FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["MeasurementStreamingService/MeasurementStreamingService.csproj", "MeasurementStreamingService/"]
COPY ["MeasurementGenerator/MeasurementGenerator.csproj", "MeasurementGenerator/"]
COPY ["DataAccess/DataAccess.csproj", "DataAccess/"]
COPY ["Models/Models.csproj", "Models/"]
RUN dotnet restore "./MeasurementStreamingService/MeasurementStreamingService.csproj"
COPY . .
WORKDIR "/src/MeasurementStreamingService"
RUN dotnet build "./MeasurementStreamingService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./MeasurementStreamingService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MeasurementStreamingService.dll"]