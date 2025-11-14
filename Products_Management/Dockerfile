# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy NuGet.config để specify NuGet source (nếu có)
COPY nuget.config* ./

# Copy solution file nếu có
COPY *.sln ./

# Copy toàn bộ source code
COPY . .

# Xóa thư mục bin và obj để tránh xung đột với build cũ
RUN find . -type d -name bin -prune -exec rm -rf {} \; 2>/dev/null || true && \
    find . -type d -name obj -prune -exec rm -rf {} \; 2>/dev/null || true

# Restore packages trước (đảm bảo PayOS được restore)
RUN dotnet restore "Products_Management/Products_Management.csproj" \
    --source https://api.nuget.org/v3/index.json \
    --no-cache \
    --verbosity detailed || \
    (echo "Restore failed, trying to add PayOS package explicitly..." && \
     dotnet add "Products_Management/Products_Management.csproj" package payOS --version 2.0.1 --source https://api.nuget.org/v3/index.json && \
     dotnet restore "Products_Management/Products_Management.csproj" \
     --source https://api.nuget.org/v3/index.json \
     --no-cache \
     --verbosity detailed)

# Verify PayOS package được restore (list packages để check)
RUN dotnet list "Products_Management/Products_Management.csproj" package | grep -i payos || echo "Warning: PayOS package not found in list"

# Build project để kiểm tra compile errors trước
RUN dotnet build "Products_Management/Products_Management.csproj" -c Release \
    --no-restore \
    --verbosity detailed || \
    (echo "Build failed, restoring again..." && \
     dotnet restore "Products_Management/Products_Management.csproj" \
     --source https://api.nuget.org/v3/index.json \
     --no-cache \
     --verbosity detailed && \
     dotnet build "Products_Management/Products_Management.csproj" -c Release \
     --verbosity detailed)

# Publish (sẽ tự động build với packages đã restore)
RUN dotnet publish "Products_Management/Products_Management.csproj" -c Release -o /app/publish \
    --no-restore \
    --verbosity detailed

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

EXPOSE 80
EXPOSE 443

ENV ASPNETCORE_URLS=http://+:80

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Products_Management.dll"]
