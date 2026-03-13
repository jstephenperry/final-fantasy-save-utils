# Final Fantasy Save Utils

Cross-platform save editor for retro Final Fantasy SNES titles. Built with .NET 10 and Avalonia.

## Supported Games

- **Final Fantasy IV** (FF2 US) — SNES SRAM save editor
- **Final Fantasy VI** (FF3 US) — SNES SRAM save editor

## Features

- Edit character stats (HP, MP, level, attributes, experience)
- Change equipped weapons, armor, and accessories
- Modify inventory items and quantities
- Edit gil, game time, and step count
- Sort and consolidate inventory
- View story progress milestones (FF4)
- Apply best-in-slot equipment loadouts (FF4)
- Max all stats with one click (FF4)
- Checksum calculation verified against ROM disassembly for both games

## Screenshots

The app uses a dark Fluent theme with a game selector dropdown, slot tabs, character editor, and expandable inventory/story sections.

## Architecture

A single unified Avalonia app with a pluggable game plugin system:

- **SaveEditor.Shell** — Shared UI shell (MainWindow, menus, file I/O, game selector)
- **SaveEditor.Shell.Abstractions** — `IGamePlugin` and `ISlotViewModel` interfaces
- **FF4SaveEditor.Core** — FF4 save format parsing, models, game data, services
- **FF4SaveEditor.Plugin** — FF4-specific views and viewmodels
- **FF6SaveEditor.Core** — FF6 save format parsing, models, game data
- **FF6SaveEditor.Plugin** — FF6-specific views and viewmodels

Adding a new game requires only a Core project (save format) and a Plugin project (UI), plus three lines of registration in the shell.

## Build & Run

Requires [.NET 10 SDK](https://dotnet.microsoft.com/download).

```sh
dotnet build FinalFantasySaveUtils.slnx
dotnet run --project src/SaveEditor.Shell
```

## Test

```sh
dotnet test FinalFantasySaveUtils.slnx
```

## Usage with OpenEmu

1. Locate your save file at `~/Library/Application Support/OpenEmu/SNES9x/Battery Saves/`
2. The `.sav` filename must match your ROM name exactly
3. Quit OpenEmu completely before editing
4. Open the `.sav` file in the editor, make changes, and save
5. Delete the auto save state at `~/Library/Application Support/OpenEmu/Save States/SuperNES/<game>/Auto Save State.oesavestate` — it overrides battery saves

## License

MIT
