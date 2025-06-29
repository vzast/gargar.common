# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy solution and project files
COPY *.sln .
COPY src/Gargar.Common.API/*.csproj ./src/Gargar.Common.API/
COPY src/Gargar.Common.Application/*.csproj ./src/Gargar.Common.Application/
COPY src/Gargar.Common.Domain/*.csproj ./src/Gargar.Common.Domain/
COPY src/Gargar.Common.Infrastructure/*.csproj ./src/Gargar.Common.Infrastructure/
COPY src/Gargar.Common.Persistance/*.csproj ./src/Gargar.Common.Persistance/

# Restore dependencies
RUN dotnet restore

# Copy all the source code
COPY src/. ./src/

# Build and publish
RUN dotnet publish -c Release -o out src/Gargar.Common.API/Gargar.Common.API.csproj

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./

# Create directory for file uploads if needed
RUN mkdir -p /app/uploads && chmod 777 /app/uploads

# Environment variables
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

# Expose port
EXPOSE 80

# Run the application
ENTRYPOINT ["dotnet", "Gargar.Common.API.dll"]