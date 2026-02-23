# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files
COPY src/TodoApi/TodoApi.csproj src/TodoApi/

# Restore dependencies
RUN dotnet restore src/TodoApi/TodoApi.csproj

# Copy source code
COPY src/TodoApi/ src/TodoApi/

# Build the application
WORKDIR /src/src/TodoApi
RUN dotnet build -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TodoApi.dll"]


