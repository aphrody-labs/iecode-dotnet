# syntax=docker/dockerfile:1
# Base images are pinned by index digest (mcr.microsoft.com, tag 10.0). Refresh both together.
FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/sdk:10.0@sha256:bef136a371963aaa46ec97882845ef6f54dec15728bb4b128b1c2a400ffbc9b4 AS build
WORKDIR /src
COPY global.json NuGet.config Directory.Build.props Directory.Packages.props ./
COPY Core/ Core/
COPY Cli/ Cli/
RUN dotnet publish Cli/IECODE.Cli.csproj -c Release -r linux-x64 --self-contained false \
    -p:UseAppHost=false -o /out

FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/runtime:10.0@sha256:64a538092439382d055c87c0f49092efb8bca883c736c54ae523da7fd8e02d18
# The runtime image ships no ICU.
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1 \
    DOTNET_EnableDiagnostics=0
WORKDIR /app
COPY --from=build /out/ ./
# Game data is never baked in: mount the installation read-only and pass --game /game
# (or set NIE_GAME_DIR for the commands that read it).
VOLUME ["/game"]
USER $APP_UID
ENTRYPOINT ["dotnet", "IECODE.Cli.dll"]
CMD ["cdn", "--help"]
