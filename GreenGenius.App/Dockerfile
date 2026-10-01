FROM mcr.microsoft.com/dotnet/aspnet:10.0.12 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0.401 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Build.props", "./"]
COPY ["GreenGenius.App/GreenGenius.App.csproj", "GreenGenius.App/"]
COPY ["GreenGenius.Api/GreenGenius.Api.csproj", "GreenGenius.Api/"]
COPY ["GreenGenius.Common.Data/GreenGenius.Common.Data.csproj", "GreenGenius.Common.Data/"]
COPY ["GreenGenius.Infra.Hosting/GreenGenius.Infra.Hosting.csproj", "GreenGenius.Infra.Hosting/"]
RUN dotnet restore "GreenGenius.App/GreenGenius.App.csproj"
COPY . .
RUN dotnet publish "GreenGenius.App/GreenGenius.App.csproj" -c "$BUILD_CONFIGURATION" --no-restore -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "GreenGenius.App.dll"]
