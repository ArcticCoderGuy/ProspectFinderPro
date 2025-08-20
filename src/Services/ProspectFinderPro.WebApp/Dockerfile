# ---------- Build ----------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore src/Services/ProspectFinderPro.WebApp/ProspectFinderPro.WebApp.csproj
RUN dotnet publish src/Services/ProspectFinderPro.WebApp/ProspectFinderPro.WebApp.csproj -c Release -o /app/publish /p:UseAppHost=false

# ---------- Runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet","ProspectFinderPro.WebApp.dll"]
