# Собирается из папки lab6: docker compose задаёт её как context.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Сначала только проекты, чтобы restore кэшировался, пока не меняются зависимости.
COPY Lab6.Contracts/Lab6.Contracts.csproj Lab6.Contracts/
COPY Lab6.History/Lab6.History.csproj Lab6.History/
RUN dotnet restore Lab6.History/Lab6.History.csproj

COPY Lab6.Contracts/ Lab6.Contracts/
COPY Lab6.History/ Lab6.History/
RUN dotnet publish Lab6.History/Lab6.History.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Lab6.History.dll"]
