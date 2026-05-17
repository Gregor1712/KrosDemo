# syntax=docker/dockerfile:1.6
# Build context: repository root (so we can copy the whole src/ tree)

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/KrosDemo.Api/KrosDemo.Api.csproj                   src/KrosDemo.Api/
COPY src/KrosDemo.Application/KrosDemo.Application.csproj   src/KrosDemo.Application/
COPY src/KrosDemo.Domain/KrosDemo.Domain.csproj             src/KrosDemo.Domain/
COPY src/KrosDemo.Infrastructure/KrosDemo.Infrastructure.csproj src/KrosDemo.Infrastructure/
RUN dotnet restore src/KrosDemo.Api/KrosDemo.Api.csproj

COPY src/ src/
RUN dotnet publish src/KrosDemo.Api/KrosDemo.Api.csproj \
        -c Release \
        -o /app/publish \
        --no-restore \
        /p:SkipNSwag=true \
        /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true

EXPOSE 8080
ENTRYPOINT ["dotnet", "KrosDemo.Api.dll"]