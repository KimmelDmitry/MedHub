FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app

EXPOSE 80

ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_HTTP_PORTS=80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["src/MedHub.Api/MedHub.Api.csproj", "src/MedHub.Api/"]
COPY ["src/MedHub.Application/MedHub.Application.csproj", "src/MedHub.Application/"]
COPY ["src/MedHub.Domain/MedHub.Domain.csproj", "src/MedHub.Domain/"]
COPY ["src/MedHub.Infrastructure/MedHub.Infrastructure.csproj", "src/MedHub.Infrastructure/"]

RUN dotnet restore "./src/MedHub.Api/./MedHub.Api.csproj"

COPY . .
WORKDIR "/src/src/MedHub.Api"
RUN dotnet build "./MedHub.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./MedHub.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "MedHub.Api.dll"]