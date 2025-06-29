# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy solution and project files
COPY *.sln .
COPY src/Gargar.Common.API/*.csproj ./src/Gargar.Common.API/
COPY src/Gargar.Common.Application/*.csproj ./src/Gargar.Common.Application/
COPY src/Gargar.Common.Domain/*.csproj ./src/Gargar.Common.Domain/
COPY src/Gargar.Common.Infrastructure/*.csproj ./src/Gargar.Common.Infrastructure/
COPY src/Gargar.Common.Persistance/*.csproj ./src/Gargar.Common.Persistance/

# Copy test projects
COPY tests/Gargar.Common.TestHelpers/*.csproj ./tests/Gargar.Common.TestHelpers/
COPY tests/UnitTests/Gargar.Common.Api.UnitTests/*.csproj ./tests/UnitTests/Gargar.Common.Api.UnitTests/
COPY tests/UnitTests/Gargar.Common.Application.UnitTests/*.csproj ./tests/UnitTests/Gargar.Common.Application.UnitTests/
COPY tests/UnitTests/Gargar.Common.Domain.UnitTests/*.csproj ./tests/UnitTests/Gargar.Common.Domain.UnitTests/
COPY tests/UnitTests/Gargar.Common.Infrastructure.UnitTests/*.csproj ./tests/UnitTests/Gargar.Common.Infrastructure.UnitTests/
COPY tests/UnitTests/Gargar.Common.Persistance.UnitTests/*.csproj ./tests/UnitTests/Gargar.Common.Persistance.UnitTests/
COPY tests/Integrationtests/Gargar.Common.IntegrationTests/*.csproj ./tests/Integrationtests/Gargar.Common.IntegrationTests/

# Restore dependencies
RUN dotnet restore

# Copy source code
COPY src/. ./src/
COPY tests/. ./tests/

# Run tests (optional - you can remove this line to speed up builds)
#RUN dotnet test

# Build and publish
RUN dotnet publish -c Release -o out src/Gargar.Common.API/Gargar.Common.API.csproj

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./

# Create directory for uploads
RUN mkdir -p /app/uploads && chmod 777 /app/uploads

# Coolify handles HTTPS, so only expose HTTP
ENV ASPNETCORE_URLS="http://+:8080"
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

ENTRYPOINT ["dotnet", "Gargar.Common.API.dll"]