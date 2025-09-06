# Use the official .NET 8.0 SDK image as a build environment
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["SmartFlowBackend.sln", "."]
COPY "SmartFlowBackend.Application/SmartFlowBackend.Application.csproj" "SmartFlowBackend.Application/"
COPY "SmartFlowBackend.Domain/SmartFlowBackend.Domain.csproj" "SmartFlowBackend.Domain/"
COPY "SmartFlowBackend.Infrastructure/SmartFlowBackend.Infrastructure.csproj" "SmartFlowBackend.Infrastructure/"
RUN dotnet restore "SmartFlowBackend.sln"
COPY . .
WORKDIR "/src/SmartFlowBackend.Application"
# Use dotnet publish for a smaller, optimized output
RUN dotnet publish "SmartFlowBackend.Application.csproj" -c Release -o /app/publish /p:UseAppHost=false

# The final runtime image, using the lightweight ASP.NET runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final
WORKDIR /app
# Copy the published output from the build stage
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
# Correct the entrypoint to point to the dll in the /app directory
ENTRYPOINT ["dotnet", "SmartFlowBackend.Application.dll"]
