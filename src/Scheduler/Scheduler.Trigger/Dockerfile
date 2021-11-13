#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.
#See also https://devandchill.com/posts/2019/06/net-core-3.0-publish-single-file-binary-on-alpine-container/
# --------------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
FROM mcr.microsoft.com/dotnet/runtime-deps:5.0-alpine AS runtime

# --------------------------------------------------------------------------------
FROM build AS restore
COPY src/Scheduler/Scheduler.EventHandler/Scheduler.EventHandler.csproj /src/Scheduler/Scheduler.EventHandler/
COPY src/Scheduler/Scheduler.Contracts/Scheduler.Contracts.csproj /src/Scheduler/Scheduler.Contracts/
RUN dotnet restore /src/Scheduler/Scheduler.EventHandler/Scheduler.EventHandler.csproj -r linux-musl-x64
RUN dotnet restore /src/Scheduler/Scheduler.Contracts/Scheduler.Contracts.csproj -r linux-musl-x64

# --------------------------------------------------------------------------------
FROM restore AS publish
COPY src /src/
RUN dotnet publish /src/Scheduler/Scheduler.EventHandler/Scheduler.EventHandler.csproj --no-restore -o /app/ -c Release -r linux-musl-x64 --self-contained true -p:CrossGenDuringPublish=false

# --------------------------------------------------------------------------------
FROM runtime AS final
WORKDIR /app/
EXPOSE 80
COPY --from=publish /app/ .
ENTRYPOINT ./assistant.net.scheduler.eventhandler
