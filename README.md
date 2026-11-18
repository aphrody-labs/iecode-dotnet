# iecode-dotnet

IECODE for .NET: a toolkit and CLI that read Inazuma Eleven Victory Road (IEVR) and sibling Level-5
games in place, without dumping or republishing any asset. It was extracted with its history from
`apps/iecode-dotnet` of the Rose Griffon monorepo (last tree there: `a8ed41ad`).

This repository contains no game data. Everything is read from an installation you own.

## Layout

| Path | What |
| --- | --- |
| `Core/` | `IECODE.Core` library: CRIWARE (CPK, UTF, ADX, HCA, USM), Level-5 formats (G4MG, G4MD, G4TX, G4PK, CfgBin, MEVBIN), NDS formats, Lua host, Steam depot download, on-demand CDN services, converters |
| `Cli/` | `IECODE.Cli` command line (`dump`, `extract`, `convert`, `cdn`, `vfs`, `search`, `steam`, ...) |
| `Ffi/` | `IECODE.Ffi` adapter for the native FFI and Node API hosts |
| `Core.Tests/` | xUnit tests (synthetic fixtures only) |
| `Gui/` | Avalonia desktop shell and absorbed tools; not part of releases |
| `Tests/Bench/` | benchmark program |

## Build

The SDK version is pinned by `global.json` (`net10.0` target).

```sh
dotnet restore IECODE.sln --locked-mode
dotnet build Core.Tests -c Release --no-restore
dotnet test Core.Tests -c Release --no-build
dotnet build Cli -c Release --no-restore
```

`IECODE.Gui` builds with the full solution (`dotnet build IECODE.sln`) and is built in CI on Windows.

## Run

```sh
dotnet run --project Cli -- --help
dotnet run --project Cli -- cdn ls face --game "/path/to/IEVR" -n 20
dotnet run --project Cli -- cdn get data/common/... --game "/path/to/IEVR" -o out.bin
dotnet run --project Cli -- cdn export ./menu-png --game "/path/to/IEVR" --filter face
dotnet run --project Cli -- cdn export-glb ./glb --game "/path/to/IEVR" --textures
```

The `cdn` command resolves any logical game path through the `cpk_list` index and extracts only the
requested file from a warm CPK reader pool. It is a command line tool, not an HTTP server: the
sibling `iecode-cpp` repository has the HTTP `serve` command.

### Configuration

| Variable | Meaning |
| --- | --- |
| `--game <path>` / `NIE_GAME_DIR` | game installation (default: detected Steam install) |
| `IECODE_CDN_CACHE` | in-memory file cache, MiB (default 256) |
| `IECODE_CDN_CACHE_MAX_ITEM` | largest cached file, MiB (default 8) |
| `IECODE_CDN_POOL` | warm CPK readers, 1 to 512 (default 48) |
| `IECODE_DUMP_MEM_BUDGET`, `IECODE_DUMP_CPK_PARALLELISM`, `IECODE_PARALLELISM`, `IECODE_IO_BUFFER` | dump and I/O tuning |
| `STEAM_USER`, `STEAM_PASSWORD`, `STEAM_GUARD_CODE` | Steam depot download (environment only, never committed) |

## Container image

```sh
docker run --rm -v "/path/to/IEVR:/game:ro" ghcr.io/aphrody-labs/iecode-dotnet:latest cdn ls --game /game -n 20
```

The image runs as a non-root user, has the CLI as entrypoint and contains no game data.

## Release

Push a tag `vX.Y.Z`. The `release` workflow publishes self-contained single-file builds for
`linux-x64` and `win-x64`, attaches the archives and `SHA256SUMS` to the GitHub Release, and pushes
`ghcr.io/aphrody-labs/iecode-dotnet:X.Y.Z` and `:latest`.

## License

MIT, see `LICENSE`. Bundled third-party sources keep their own terms and notices
(`LICENSE.upstream`, `Gui/*/LICENSE*`).
