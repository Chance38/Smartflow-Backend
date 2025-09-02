FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /app

# 複製 solution 與 nuget config
COPY ["SmartFlowBackend.sln", "nuget.config", "./"]

# 複製所有專案的 .csproj 檔案
COPY ["SmartFlowBackend.Application/SmartFlowBackend.Application.csproj", "SmartFlowBackend.Application/"]
COPY ["SmartFlowBackend.Domain/SmartFlowBackend.Domain.csproj", "SmartFlowBackend.Domain/"]
COPY ["SmartFlowBackend.Infrastructure/SmartFlowBackend.Infrastructure.csproj", "SmartFlowBackend.Infrastructure/"]

# 還原所有專案的依賴
RUN dotnet restore "SmartFlowBackend.sln"

# 複製其餘所有原始碼
COPY . .

WORKDIR /app/SmartFlowBackend.Application
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "SmartFlowBackend.Application.dll"]
