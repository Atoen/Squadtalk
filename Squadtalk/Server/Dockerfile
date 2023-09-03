FROM mcr.microsoft.com/dotnet/aspnet:7.0.10-jammy-amd64 AS base

ARG TARGETARCH
ARG TARGETOS

RUN arch=$TARGETARCH \
    && if [ "$arch" = "amd64" ]; then arch="x64"; fi \
    && echo $TARGETOS-$arch > /tmp/rid

WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0.400-jammy-amd64 AS build
WORKDIR /src
COPY ["Squadtalk/Server/Squadtalk.Server.csproj", "Squadtalk/Server/"]
COPY ["Squadtalk/Client/Squadtalk.Client.csproj", "Squadtalk/Client/"]
COPY ["Squadtalk/Shared/Squadtalk.Shared.csproj", "Squadtalk/Shared/"]

RUN dotnet restore "Squadtalk/Server/Squadtalk.Server.csproj" -r $(cat /tmp/rid)
COPY . .
WORKDIR "/src/Squadtalk/Server"
RUN dotnet build "Squadtalk.Server.csproj" -c Release -o /app/build -r $(cat /tmp/rid) --self-contained false --no-restore

FROM build AS publish
RUN dotnet publish "Squadtalk.Server.csproj" -c Release -o /app/publish /p:UseAppHost=false -r $(cat /tmp/rid) --self-contained false --no-restore

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Squadtalk.Server.dll"]
