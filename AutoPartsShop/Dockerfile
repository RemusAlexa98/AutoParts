# ================= BUILD STAGE =================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# copiem proiectul și restaurăm pachetele
COPY *.csproj ./
RUN dotnet restore

# copiem tot restul codului
COPY . ./
RUN dotnet publish -c Release -o /app/publish

# ================= RUNTIME STAGE =================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# copiem output-ul
COPY --from=build /app/publish .

# forțăm portul intern
ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000

ENTRYPOINT ["dotnet", "AutoPartsShop.dll"]
