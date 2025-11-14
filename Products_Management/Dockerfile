# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy NuGet.config để specify NuGet source (nếu có)
COPY nuget.config* ./

# Copy solution file nếu có
COPY *.sln ./

# Copy toàn bộ source code
COPY . .

# Xóa thư mục bin và obj để tránh xung đột với build cũ và cache restore
RUN rm -rf **/bin **/obj 2>/dev/null || true && \
    find . -type d -name bin -exec rm -rf {} + 2>/dev/null || true && \
    find . -type d -name obj -exec rm -rf {} + 2>/dev/null || true

# Xóa NuGet cache để force restore lại tất cả packages
RUN dotnet nuget locals all --clear || true

# Xóa các file cache restore cụ thể
RUN find . -name "project.assets.json" -delete 2>/dev/null || true && \
    find . -name "project.nuget.cache" -delete 2>/dev/null || true && \
    find . -name "*.nuget.props" -delete 2>/dev/null || true && \
    find . -name "*.nuget.targets" -delete 2>/dev/null || true

# Restore packages trước (đảm bảo PayOS được restore) - force restore không dùng cache
RUN dotnet restore "Products_Management/Products_Management.csproj" \
    --source https://api.nuget.org/v3/index.json \
    --force \
    --no-cache \
    --verbosity detailed

# Verify PayOS package được restore (list packages để check)
RUN echo "=== Checking packages ===" && \
    dotnet list "Products_Management/Products_Management.csproj" package || true && \
    echo "=== End package list ==="

# Nếu PayOS không được restore, thử add lại
RUN dotnet list "Products_Management/Products_Management.csproj" package | grep -i "payos" || \
    (echo "PayOS not found, adding package..." && \
     dotnet add "Products_Management/Products_Management.csproj" package payOS --version 2.0.1 --source https://api.nuget.org/v3/index.json && \
     dotnet restore "Products_Management/Products_Management.csproj" \
     --source https://api.nuget.org/v3/index.json \
     --force \
     --no-cache \
     --verbosity detailed)

# Verify PayOS DLL tồn tại sau restore
RUN find ~/.nuget/packages -name "*PayOS*.dll" -o -name "*payOS*.dll" 2>/dev/null | head -5 || echo "Warning: PayOS DLL not found in NuGet cache"

# Build project để kiểm tra compile errors trước
RUN dotnet build "Products_Management/Products_Management.csproj" -c Release \
    --no-restore \
    --verbosity detailed || \
    (echo "Build failed with --no-restore, trying build without --no-restore..." && \
     dotnet build "Products_Management/Products_Management.csproj" -c Release \
     --source https://api.nuget.org/v3/index.json \
     --verbosity detailed)

# Publish (sẽ tự động restore và build nếu cần)
RUN dotnet publish "Products_Management/Products_Management.csproj" -c Release -o /app/publish \
    --source https://api.nuget.org/v3/index.json \
    --verbosity detailed

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

EXPOSE 80
EXPOSE 443

ENV ASPNETCORE_URLS=http://+:80

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Products_Management.dll"]
