FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# 1. Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
ENV DOTNET_BUILD_CONFIGURATION=$BUILD_CONFIGURATION
WORKDIR /src

COPY ["src/AdvancedEfCore.Api/AdvancedEfCore.Api.csproj", "src/AdvancedEfCore.Api/"]
RUN dotnet restore "./src/AdvancedEfCore.Api/AdvancedEfCore.Api.csproj"

COPY . .
WORKDIR "/src/src/AdvancedEfCore.Api"
RUN dotnet build "./AdvancedEfCore.Api.csproj" -c $DOTNET_BUILD_CONFIGURATION -o /app/build

# 2. Publish stage
FROM build AS publish
RUN dotnet publish "./AdvancedEfCore.Api.csproj" -c $DOTNET_BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# 3. Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AdvancedEfCore.Api.dll"]
