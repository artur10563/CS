FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["LR6_Api/LR6_Api.csproj", "LR6_Api/"]
COPY ["LR6_Data/LR6_Data.csproj", "LR6_Data/"]
RUN dotnet restore "LR6_Api/LR6_Api.csproj"
COPY . .
WORKDIR "/src/LR6_Api"
RUN dotnet build "LR6_Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "LR6_Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LR6_Api.dll"]
