FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["UserBehaviorService/UserBehaviorService.csproj", "UserBehaviorService/"]
RUN dotnet restore "UserBehaviorService/UserBehaviorService.csproj"

COPY . .
WORKDIR "/src/UserBehaviorService"
RUN dotnet publish "UserBehaviorService.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "UserBehaviorService.dll"]