# iecode-dotnet: agent contract

IECODE for .NET: the C# toolkit that reads Inazuma Eleven Victory Road (IEVR) and sibling Level-5
game data in place: CRIWARE CPK/UTF/ADX/HCA/USM, Level-5 formats (G4MG, G4MD, G4TX, G4PK, CfgBin,
MEVBIN), Lua, Steam depot download, and the on-demand `cdn` command. It was extracted with its
history from `apps/iecode-dotnet` of the Rose Griffon monorepo (last tree there: `a8ed41ad`).

## Rules

- .NET SDK version comes from `global.json`; target is `net10.0`. NuGet versions live only in
  `Directory.Packages.props`; lock files are committed and CI restores in locked mode
  (`dotnet restore --locked-mode`; refresh with `dotnet restore --force-evaluate`).
- Never commit, copy or bake a game asset into an image or fixture. Game data is read in place from
  the user's installation (`--game <path>`, `NIE_GAME_DIR`). Tests build synthetic fixtures.
- `IECODE.Gui` (Avalonia) is not part of the release or the image; the CLI and the Core library are.
- Secrets (`STEAM_USER`, `STEAM_PASSWORD`, `STEAM_GUARD_CODE`) come from the environment only; never
  print, log or commit them. Run `gitleaks detect` before every push.
- Heavy builds run in GitHub Actions or on the build host (`ssh fr1`), not on the workstation.
- Commit by path on `main`, no branches, no AI attribution. Check `gh run list -R aphrody-labs/iecode-dotnet`
  after pushing. Releases are tags `v*`; the workflow publishes archives and the ghcr image.
- English, no emoji, in code, docs and commits. Original comments in French stay until the file is
  touched for another reason.
- Third-party notices stay with their code (`LICENSE.upstream`, `Gui/*/LICENSE*`). Do not delete them.
