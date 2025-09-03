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
RUN dotnet build "SmartFlowBackend.Application.csproj" -c Release -o /app/build

FROM build AS publish
WORKDIR "/src/SmartFlowBackend.Application"
RUN dotnet publish "SmartFlowBackend.Application.csproj" -c Release -o /app/publish

# The final runtime image
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY . /src

ENV ASPNETCORE_URLS=http://+:8080
RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"
RUN chmod +x /src/entrypoint.sh
EXPOSE 8080
ENTRYPOINT ["/src/entrypoint.sh"]
CMD ["dotnet", "/app/SmartFlowBackend.Application.dll"]
