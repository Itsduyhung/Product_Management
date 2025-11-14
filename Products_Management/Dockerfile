# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy NuGet.config để specify NuGet source (nếu có)
COPY nuget.config* ./

# Copy solution file nếu có
COPY *.sln ./

# Copy toàn bộ source code (obj và bin đã được ignore bởi .dockerignore)
COPY . .

# Xóa thư mục bin và obj nếu có (để đảm bảo sạch sẽ)
RUN find . -type d \( -name bin -o -name obj \) -exec rm -rf {} + 2>/dev/null || true

# Xóa TẤT CẢ các file restore cache TRƯỚC TIÊN (phải xóa để force restore lại)
RUN echo "=== Deleting restore cache files ===" && \
    rm -rf Products_Management/obj/project.assets.json 2>/dev/null || true && \
    rm -rf Products_Management/obj/project.nuget.cache 2>/dev/null || true && \
    rm -rf Products_Management/obj/*.nuget.props 2>/dev/null || true && \
    rm -rf Products_Management/obj/*.nuget.targets 2>/dev/null || true && \
    find . -type f -name "project.assets.json" -delete 2>/dev/null || true && \
    find . -type f -name "project.nuget.cache" -delete 2>/dev/null || true && \
    find . -type f -name "*.nuget.props" -delete 2>/dev/null || true && \
    find . -type f -name "*.nuget.targets" -delete 2>/dev/null || true && \
    find . -type d -name ".nuget" -exec rm -rf {} + 2>/dev/null || true && \
    echo "✓ All restore cache files deleted"

# Xóa NuGet cache global để force restore lại tất cả packages
RUN dotnet nuget locals all --clear || true

# Verify đã xóa cache (không còn project.assets.json)
RUN echo "=== Verifying cache files are deleted ===" && \
    (find . -name "project.assets.json" 2>/dev/null | wc -l | xargs test 0 -eq) && \
    echo "✓ Cache files successfully deleted" || \
    echo "⚠ Warning: Some cache files may still exist"

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
