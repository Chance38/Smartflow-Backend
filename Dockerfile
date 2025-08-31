FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /app

# 複製 solution 與 nuget config
COPY ["SmartFlowBackend.sln", "nuget.config", "./"]
# 複製 csproj 到正確目錄
COPY ["SmartFlowBackend/SmartFlowBackend.csproj", "SmartFlowBackend/"]

RUN dotnet restore "SmartFlowBackend.sln"

# 複製其餘原始碼
COPY . .

WORKDIR /app/SmartFlowBackend
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "SmartFlowBackend.dll"]
