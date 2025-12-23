FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["LR4_gRPC_Chat_Server/LR4_gRPC_Chat_Server.csproj", "LR4_gRPC_Chat_Server/"]
RUN dotnet restore "LR4_gRPC_Chat_Server/LR4_gRPC_Chat_Server.csproj"
COPY . .
WORKDIR "/src/LR4_gRPC_Chat_Server"
RUN dotnet build "LR4_gRPC_Chat_Server.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "LR4_gRPC_Chat_Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LR4_gRPC_Chat_Server.dll"]
