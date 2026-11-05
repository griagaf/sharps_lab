# Собирается из папки lab5: docker compose задаёт её как context.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Сначала только проекты, чтобы restore кэшировался, пока не меняются зависимости.
COPY Lab5.Contracts/Lab5.Contracts.csproj Lab5.Contracts/
COPY Lab5.History/Lab5.History.csproj Lab5.History/
RUN dotnet restore Lab5.History/Lab5.History.csproj

COPY Lab5.Contracts/ Lab5.Contracts/
COPY Lab5.History/ Lab5.History/
RUN dotnet publish Lab5.History/Lab5.History.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Lab5.History.dll"]
