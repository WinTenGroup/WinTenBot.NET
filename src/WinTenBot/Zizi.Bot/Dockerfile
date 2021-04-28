#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.
FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
VOLUME ["C:/Users/azhe403/.nuget/packages","/root/.nuget/packages"]

WORKDIR /src
COPY ["Zizi.Bot/Zizi.Bot.csproj", "Zizi.Bot/"]
COPY ["Zizi.Core/Zizi.Core.csproj", "Zizi.Core/"]
RUN dotnet restore --verbosity n "Zizi.Bot/Zizi.Bot.csproj"
COPY . .
WORKDIR "/src/Zizi.Bot"
RUN dotnet build "Zizi.Bot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Zizi.Bot.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Zizi.Bot.dll"]