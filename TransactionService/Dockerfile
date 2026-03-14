FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["TransactionService/TransactionService.csproj", "TransactionService/"]
COPY ["PRN232_Seminar.Shared/PRN232_Seminar.Shared.csproj", "PRN232_Seminar.Shared/"]
RUN dotnet restore "TransactionService/TransactionService.csproj"

COPY TransactionService/ TransactionService/
COPY PRN232_Seminar.Shared/ PRN232_Seminar.Shared/
RUN dotnet publish "TransactionService/TransactionService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENTRYPOINT ["dotnet", "TransactionService.dll"]