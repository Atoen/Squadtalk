FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Squadtalk/Server/Squadtalk.Server.csproj", "Squadtalk/Server/"]
COPY ["Squadtalk/Client/Squadtalk.Client.csproj", "Squadtalk/Client/"]
COPY ["Squadtalk/Shared/Squadtalk.Shared.csproj", "Squadtalk/Shared/"]
RUN dotnet restore "Squadtalk/Server/Squadtalk.Server.csproj"
COPY . .
WORKDIR "/src/Squadtalk/Server"
RUN dotnet build "Squadtalk.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Squadtalk.Server.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Squadtalk.Server.dll"]
