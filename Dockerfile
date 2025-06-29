# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy solution and project files
COPY *.sln .
# Main projects
COPY src/Gargar.Common.API/*.csproj ./src/Gargar.Common.API/
COPY src/Gargar.Common.Application/*.csproj ./src/Gargar.Common.Application/
COPY src/Gargar.Common.Domain/*.csproj ./src/Gargar.Common.Domain/
COPY src/Gargar.Common.Infrastructure/*.csproj ./src/Gargar.Common.Infrastructure/
COPY src/Gargar.Common.Persistance/*.csproj ./src/Gargar.Common.Persistance/

# Test projects
COPY tests/Gargar.Common.TestHelpers/*.csproj ./tests/Gargar.Common.TestHelpers/
COPY tests/UnitTests/Gargar.Common.Api.UnitTests/*.csproj ./tests/UnitTests/Gargar.Common.Api.UnitTests/
COPY tests/UnitTests/Gargar.Common.Application.UnitTests/*.csproj ./tests/UnitTests/Gargar.Common.Application.UnitTests/
COPY tests/UnitTests/Gargar.Common.Domain.UnitTests/*.csproj ./tests/UnitTests/Gargar.Common.Domain.UnitTests/
COPY tests/UnitTests/Gargar.Common.Infrastructure.UnitTests/*.csproj ./tests/UnitTests/Gargar.Common.Infrastructure.UnitTests/
COPY tests/UnitTests/Gargar.Common.Persistance.UnitTests/*.csproj ./tests/UnitTests/Gargar.Common.Persistance.UnitTests/
COPY tests/Integrationtests/Gargar.Common.IntegrationTests/*.csproj ./tests/Integrationtests/Gargar.Common.IntegrationTests/

# Restore dependencies
RUN dotnet restore

# Copy all the source code and test code
COPY src/. ./src/
COPY tests/. ./tests/

# Run tests (optional, but recommended)
RUN dotnet test

# Build and publish
RUN dotnet publish -c Release -o out src/Gargar.Common.API/Gargar.Common.API.csproj

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./

# Create directory for file uploads if needed
RUN mkdir -p /app/uploads && chmod 777 /app/uploads

# Environment variables
ENV ASPNETCORE_URLS=https://+:443;http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

# Expose port
EXPOSE 80
EXPOSE 443
# Run the application
ENTRYPOINT ["dotnet", "Gargar.Common.API.dll"]