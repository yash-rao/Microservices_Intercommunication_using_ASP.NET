# 1. Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy all project folders (we fixed the context in docker-compose already)
COPY . ./

# Optional restore (faster builds)
RUN dotnet restore ./ProductService.API/ProductService.API.csproj

# Publish from correct path
RUN dotnet publish ./ProductService.API/ProductService.API.csproj -c Release -o /out

# 2. Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /out .

ENTRYPOINT ["dotnet", "ProductService.API.dll"]
