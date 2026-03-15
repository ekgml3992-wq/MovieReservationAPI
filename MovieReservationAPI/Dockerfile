FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore MovieReservationAPI.sln
RUN dotnet publish MovieReservationAPI/MovieReservationAPI.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:7082

EXPOSE 7082

ENTRYPOINT ["dotnet", "MovieReservationAPI.dll"]
