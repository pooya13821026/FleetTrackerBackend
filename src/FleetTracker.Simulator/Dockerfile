FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/FleetTracker.Domain/FleetTracker.Domain.csproj src/FleetTracker.Domain/
COPY src/FleetTracker.Application/FleetTracker.Application.csproj src/FleetTracker.Application/
COPY src/FleetTracker.Simulator/FleetTracker.Simulator.csproj src/FleetTracker.Simulator/
RUN dotnet restore src/FleetTracker.Simulator/FleetTracker.Simulator.csproj

COPY src/ src/
RUN dotnet publish src/FleetTracker.Simulator -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "FleetTracker.Simulator.dll"]
