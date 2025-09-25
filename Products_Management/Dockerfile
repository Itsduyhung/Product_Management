# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj và restore dependency
COPY ["Products_Management/Products_Management.csproj", "Products_Management/"]
RUN dotnet restore "Products_Management/Products_Management.csproj"

# Copy toàn bộ source code
COPY . .

# Publish ra thư mục /app/publish
RUN dotnet publish "Products_Management/Products_Management.csproj" -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

EXPOSE 80
EXPOSE 443

ENV ASPNETCORE_URLS=http://+:80

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Products_Management.dll"]
