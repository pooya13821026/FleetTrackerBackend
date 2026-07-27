FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# اولrestoreپکیجها
COPY src/FleetTracker.Domain/FleetTracker.Domain.csproj src/FleetTracker.Domain/
COPY src/FleetTracker.Application/FleetTracker.Application.csproj src/FleetTracker.Application/
COPY src/FleetTracker.Infrastructure/FleetTracker.Infrastructure.csproj src/FleetTracker.Infrastructure/
COPY src/FleetTracker.Api/FleetTracker.Api.csproj src/FleetTracker.Api/
RUN dotnet restore src/FleetTracker.Api/FleetTracker.Api.csproj

# بعد کپی کل سورس و build
COPY src/ src/
RUN dotnet publish src/FleetTracker.Api -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
ENTRYPOINT ["dotnet", "FleetTracker.Api.dll"]
