FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["UserProfileService/UserProfileService.csproj", "UserProfileService/"]
RUN dotnet restore "UserProfileService/UserProfileService.csproj"

COPY . .
WORKDIR "/src/UserProfileService"
RUN dotnet publish "UserProfileService.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "UserProfileService.dll"]