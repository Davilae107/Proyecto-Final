FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["SIPAD.API/SIPAD.API.csproj", "SIPAD.API/"]
COPY ["SIPAD.Application/SIPAD.Application.csproj", "SIPAD.Application/"]
RUN dotnet restore "SIPAD.API/SIPAD.API.csproj"
COPY . .
WORKDIR "/src/SIPAD.API"
RUN dotnet build "SIPAD.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SIPAD.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SIPAD.API.dll"]