FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
# Copy file dự án để restore dependencies
COPY ["NotificationService/NotificationService.csproj", "NotificationService/"]
COPY ["PRN232_Seminar.Shared/PRN232_Seminar.Shared.csproj", "PRN232_Seminar.Shared/"]
RUN dotnet restore "NotificationService/NotificationService.csproj"

# Copy toàn bộ code và build
COPY NotificationService/ NotificationService/
COPY PRN232_Seminar.Shared/ PRN232_Seminar.Shared/
RUN dotnet publish "NotificationService/NotificationService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENTRYPOINT ["dotnet", "NotificationService.dll"]