# Final Fantasy Save Utils

Multi-game save editor for retro Final Fantasy titles. Unified Avalonia app with pluggable game-specific components.

## Repository Structure

```
final-fantasy-save-utils/
  FinalFantasySaveUtils.slnx          # Single solution for all projects
  src/
    SaveEditor.Shell/                  # Shared Avalonia app (the single executable)
    SaveEditor.Shell.Abstractions/     # IGamePlugin + ISlotViewModel interfaces
    FF4SaveEditor.Core/                # FF4 models, IO, GameData, Services (no UI)
    FF4SaveEditor.Plugin/              # FF4 views, viewmodels, FF4GamePlugin
    FF4SaveEditor.Tests/               # FF4 xUnit tests
    FF6SaveEditor.Core/                # FF6 models, IO, GameData (no UI)
    FF6SaveEditor.Plugin/              # FF6 views, viewmodels, FF6GamePlugin
    FF6SaveEditor.Tests/               # FF6 xUnit tests
```

## Tech Stack

- **.NET 10** (net10.0)
- **Avalonia 11.2.7** with Fluent dark theme for cross-platform desktop UI
- **CommunityToolkit.Mvvm** for MVVM pattern (ObservableProperty, RelayCommand)
- **xUnit** for testing
- Solution files use `.slnx` format

## Architecture Conventions

- **Shell** (`SaveEditor.Shell`): Shared Avalonia executable with MainWindow, menu, status bar, game selector, file I/O. Delegates to `IGamePlugin` for game-specific logic.
- **Shell.Abstractions**: Minimal interface layer — `IGamePlugin` (Load/Save/GameName) and `ISlotViewModel` (Header/IsValid). The only contract between shell and plugins.
- **Plugin** projects: Game-specific views, viewmodels, and `IGamePlugin` implementation. Avalonia DataTemplates in `App.axaml` map each plugin's `SlotViewModel` to its `SlotView`.
- **Core** projects: Pure library (no UI dependency) — models, IO, game data, services
- **Tests** projects reference Core only
- Game data (item databases, etc.) are embedded JSON resources in Core
- Save file IO reads/writes raw bytes; models expose typed properties over a raw byte buffer for round-trip fidelity
- Checksums and validation must match the actual game's algorithm exactly (verified against ROM disassembly)

## FF4-Specific Details

- **SRAM format**: 8192 bytes = 4 slots x 2048 bytes
- **Checksum**: 65816-style ADC with carry propagation over overlapping 16-bit LE words, 0x7FA iterations (save.asm:121-140). Carry flag is cleared once before the loop, NOT per iteration.
- **Validation**: 0x1BE4 at slot offset 0x7FE, load flag 0x01 at 0x7FB
- **Character data**: 5 chars x 64 bytes at offset 0x000 within each slot
- **Character byte 0x00**: bits 7-6 = handedness (bit7=right-handed, bit6=left-handed), bits 5-0 = character ID. Handedness only affects bow damage (20% penalty when bow is in off-hand)
- **Equipment**: 7 bytes at character offset 0x30-0x36 (helmet, armor, accessory, R.hand, R.hand qty, L.hand, L.hand qty)
- **Equip bitmask**: 14-bit mask — CecilDK=0, Kain=1, RydiaChild=2, Tellah=3, Edward=4, Rosa=5, Yang=6, Palom=7, Porom=8, CecilPaladin=9, Cid=10, RydiaAdult=11, Edge=12, FuSoYa=13
- **Inventory**: 48 slots x 2 bytes (itemId, quantity) at offset 0x440
- **Spell lists**: 13 lists x 24 bytes at offset 0x560 (not currently edited)
- **Gil**: 24-bit LE at offset 0x6A0
- **Item IDs**: 0x00=None, 0xFE/0xFF=Empty sentinels
- **Key items**: BaronKey=0xEF, EarthCrystal=0xF1, MagmaKey=0xF2, LucaKey=0xF3, TwinHarp=0xF4, DarkCrystal=0xF5
- **Character IDs** (byte 0x00 bits 5-0): None=0x00, CecilDK=0x01, Kain=0x02, RydiaChild=0x03, Tellah=0x04, Edward=0x05, Rosa=0x06, Yang=0x07, Palom=0x08, Porom=0x09, Tellah2=0x0A, CecilPaladin=0x0B, Tellah3=0x0C, Yang2=0x0D, Cid=0x0E, Kain2=0x0F, Rosa2=0x10, RydiaAdult=0x11, Edge=0x12, FuSoYa=0x13, Kain3=0x14, Golbez=0x15
- **Characters that rejoin get new IDs**: Tellah(0x04,0x0A,0x0C), Yang(0x07,0x0D), Kain(0x02,0x0F,0x14), Rosa(0x06,0x10)
- **Event switches**: 32 bytes (256 bits) at slot offset 0x280-0x29F; primary mechanism for story progress tracking. Some are permanent event flags, others are state toggles (e.g., switch 48 "Passage Underground Open" is cleared when Cid seals the hole)
- **World byte**: offset 0x701 — 0=overworld, 1=underground, 2=moon
- **Reference disassembly**: https://github.com/everything8215/ff4 (save.asm, ram-map, field/event.asm)

## FF6-Specific Details

- **SRAM format**: 8192 bytes = 3 slots x 2560 bytes at offsets $0000, $0A00, $1400
- **SRAM validity**: 4x `0xE41B` at file offsets $1FF8/$1FFA/$1FFC/$1FFE
- **Checksum**: Byte-by-byte ADC into 16-bit accumulator (lo/hi bytes), `CPX #$09FE` clears carry each iteration (no cross-iteration propagation), stored at slot+$09FE
- **Character data**: 16 chars x 37 bytes at slot+$0000
- **Character layout (37 bytes)**: ActorId(1), GraphicIndex(1), Name(6, FF6 text), Level(1), CurrentHP(2), MaxHP(2, 0x3FFF mask), CurrentMP(2), MaxMP(2), EXP(3, 24-bit LE), Status(2), Commands(4), Vigor/Speed/Stamina/MagicPower(4), EquippedEsper(1), Equipment(6: weapon/shield/helmet/armor/relic1/relic2)
- **Gil**: 24-bit LE at slot+$0260, max 9,999,999
- **Game time**: H/M/S at slot+$0263, **Steps**: 24-bit LE at slot+$0266
- **Inventory**: 256 IDs at slot+$0269 + 256 qtys at slot+$0369 (separate arrays, 0xFF=empty)
- **Espers**: 4-byte bitmask at slot+$0469
- **Text encoding**: 0x80=A..0x99=Z, 0x9A=a..0xB3=z, 0xB4=0..0xBD=9, 0xFF=terminator
- **Equip bitmask**: 14-bit mask — Terra=0, Locke=1, Cyan=2, Shadow=3, Edgar=4, Sabin=5, Celes=6, Strago=7, Relm=8, Setzer=9, Mog=10, Gau=11, Gogo=12, Umaro=13
- **Reference disassembly**: https://github.com/everything8215/ff6

## OpenEmu Integration Notes

- OpenEmu SNES9x saves use `.sav` extension in `~/Library/Application Support/OpenEmu/SNES9x/Battery Saves/`
- Filename must match ROM name exactly (e.g., `Final Fantasy II (USA) (Rev 1).sav`)
- **Auto save state overrides battery saves** — must delete the auto save state at `~/Library/Application Support/OpenEmu/Save States/SuperNES/<game>/Auto Save State.oesavestate` for a modified `.sav` to take effect
- OpenEmu must be fully quit before replacing save files
- Rev 0 and Rev 1 ROMs use the same save format

## Build & Test

```sh
dotnet build FinalFantasySaveUtils.slnx
dotnet test FinalFantasySaveUtils.slnx
dotnet run --project src/SaveEditor.Shell
```

## Adding a New Game

1. Create `src/FFxSaveEditor.Core/` with models, IO, checksum, game data
2. Create `src/FFxSaveEditor.Plugin/` with ViewModels, Views, and `FFxGamePlugin : IGamePlugin`
3. Register plugin in `src/SaveEditor.Shell/App.axaml.cs` (one line)
4. Add DataTemplate in `src/SaveEditor.Shell/App.axaml` (one line)
5. Add project references to `SaveEditor.Shell.csproj`
6. Verify checksum algorithm against ROM disassembly — do not assume standard sum-of-bytes
7. Embedded JSON for game data (items, spells, etc.)
8. Preserve raw bytes in models for round-trip fidelity
