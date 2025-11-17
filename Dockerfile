# Use the official .NET 8.0 SDK image as a build environment
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["SmartFlowBackend.sln", "."]
COPY "Application/Application.csproj" "Application/"
COPY "Domain/Domain.csproj" "Domain/"
COPY "Infrastructure/Infrastructure.csproj" "Infrastructure/"
COPY "Presentation/Presentation.csproj" "Presentation/"
COPY "Test/SmartFlowBackend.Test.csproj" "Test/"
RUN dotnet restore "SmartFlowBackend.sln"
COPY . .
WORKDIR "/src/Presentation"
# Use dotnet publish for a smaller, optimized output
RUN dotnet publish "Presentation.csproj" -c Release -o /app/publish /p:UseAppHost=false

# The final runtime image, using the lightweight ASP.NET runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final
WORKDIR /app
# Copy the published output from the build stage
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80
# Correct the entrypoint to point to the dll in the /app directory
ENTRYPOINT ["dotnet", "Presentation.dll"]
