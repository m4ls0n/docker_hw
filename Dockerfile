FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["AchievementsConsoleApp/AchievementsConsoleApp.csproj", "AchievementsConsoleApp/"]
COPY ["AchievementsLibrary/AchievementsLibrary.csproj", "AchievementsLibrary/"]
RUN dotnet restore "AchievementsConsoleApp/AchievementsConsoleApp.csproj"
COPY . .
WORKDIR "/src/AchievementsConsoleApp"
RUN dotnet build "AchievementsConsoleApp.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "AchievementsConsoleApp.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AchievementsConsoleApp.dll"]
