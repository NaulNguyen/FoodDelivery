FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
# Copy project files indicating the directory structure
COPY ["FoodDelivery.API/FoodDelivery.API.csproj", "FoodDelivery.API/"]
COPY ["FoodDelivery.Application/FoodDelivery.Application.csproj", "FoodDelivery.Application/"]
COPY ["FoodDelivery.Domain/FoodDelivery.Domain.csproj", "FoodDelivery.Domain/"]
COPY ["FoodDelivery.Infrastructure/FoodDelivery.Infrastructure.csproj", "FoodDelivery.Infrastructure/"]

# Restore NuGet packages
RUN dotnet restore "FoodDelivery.API/FoodDelivery.API.csproj"

# Copy the remaining source code
COPY . .
WORKDIR "/src/FoodDelivery.API"

# Build the application
RUN dotnet build "FoodDelivery.API.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "FoodDelivery.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Generate final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set ENTRYPOINT
ENTRYPOINT ["dotnet", "FoodDelivery.API.dll"]
