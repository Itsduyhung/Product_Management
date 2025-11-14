# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy NuGet.config để specify NuGet source (nếu có)
COPY nuget.config* ./

# Copy solution file nếu có
COPY *.sln ./

# Copy toàn bộ source code
COPY . .

# Restore packages trước (đảm bảo PayOS được restore)
RUN dotnet restore "Products_Management/Products_Management.csproj" \
    --source https://api.nuget.org/v3/index.json \
    --no-cache \
    --verbosity detailed

# Publish (publish sẽ tự động restore nếu cần)
RUN dotnet publish "Products_Management/Products_Management.csproj" -c Release -o /app/publish \
    --source https://api.nuget.org/v3/index.json \
    --no-cache \
    --verbosity detailed

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

EXPOSE 80
EXPOSE 443

ENV ASPNETCORE_URLS=http://+:80

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Products_Management.dll"]
