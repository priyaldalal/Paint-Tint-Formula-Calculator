# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy csproj and restore dependencies
COPY ["src/PaintTint.Core/PaintTint.Core.csproj", "src/PaintTint.Core/"]
COPY ["src/PaintTint.Infrastructure/PaintTint.Infrastructure.csproj", "src/PaintTint.Infrastructure/"]
COPY ["src/PaintTint.Api/PaintTint.Api.csproj", "src/PaintTint.Api/"]
RUN dotnet restore "src/PaintTint.Api/PaintTint.Api.csproj"

# Copy source and publish
COPY . .
WORKDIR "/app/src/PaintTint.Api"
RUN dotnet publish "PaintTint.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000
ENTRYPOINT ["dotnet", "PaintTint.Api.dll"]
