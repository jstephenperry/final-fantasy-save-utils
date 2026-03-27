# Final Fantasy Save Utils

Multi-game save editor for retro Final Fantasy titles. Unified Avalonia app with pluggable game-specific components.

## Repository Structure

```
final-fantasy-save-utils/
  FinalFantasySaveUtils.slnx          # Single solution for all projects
  src/
    SaveEditor.Shell/                  # Shared Avalonia app (the single executable)
    SaveEditor.Shell.Abstractions/     # IGamePlugin + ISlotViewModel interfaces
    FF1SaveEditor.Core/                # FF1 models, IO, GameData (no UI)
    FF1SaveEditor.Plugin/              # FF1 views, viewmodels, FF1GamePlugin
    FF1SaveEditor.Tests/               # FF1 xUnit tests
    FF2SaveEditor.Core/                # FF2 models, IO, GameData (no UI)
    FF2SaveEditor.Plugin/              # FF2 views, viewmodels, FF2GamePlugin
    FF2SaveEditor.Tests/               # FF2 xUnit tests
    FF3SaveEditor.Core/                # FF3 models, IO, GameData (no UI)
    FF3SaveEditor.Plugin/              # FF3 views, viewmodels, FF3GamePlugin
    FF3SaveEditor.Tests/               # FF3 xUnit tests
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

## FF1-Specific Details

- **SRAM format**: 8192 bytes total. Working copy at $0000-$03FF, validated copy at $0400-$07FF (1 slot)
- **Validation**: Magic bytes $55 and $AA, plus 8-bit ADC checksum
- **Checksum**: 8-bit sum with carry propagation over 4 pages (256 bytes each): CLC once, then for X=0..255: ADC page0,X; ADC page1,X; ADC page2,X; ADC page3,X. Total must equal $FF. Checksum byte at offset $FD. (bank_0F.asm, VerifyChecksum at $C888)
- **Character data**: 4 chars x 64 bytes at slot offset $100
- **Character layout (64 bytes)**: ClassId(1), Status(1), Name(4, FF1 text), unused(1), EXP(3, 24-bit LE), CurrentHP(2), MaxHP(2), unused(2), Str/Agi/Int/Vit/Luck(5), unused(3), Weapons(4, high bit=equipped), Armor(4, high bit=equipped), Damage/Hit%/Absorb/Evade%(4), unused(2), Level(1)
- **Gil**: 24-bit LE at slot offset $1C, max 999,999
- **Key items**: Individual byte flags at offsets $20-$31 (Lute, Crown, Crystal, Herb, MysticKey, TNT, Adamant, Slab, Ruby, Rod, Floater, Chime, Tail, Cube, Bottle, Oxyale, FireOrb, WaterOrb)
- **Magic data**: At slot offset $300, 3 spells per level x 8 levels x 4 chars + MP current/max
- **Text encoding**: $80-$89=0-9, $8A-$A3=A-Z, $A4-$BD=a-z, $FF=space/terminator
- **Classes**: Fighter(0), Thief(1), BlackBelt(2), RedMage(3), WhiteMage(4), BlackMage(5), Knight(6), Ninja(7), Master(8), RedWizard(9), WhiteWizard(10), BlackWizard(11)
- **Reference disassembly**: https://github.com/Entroper/FF1Disassembly (Disch), https://github.com/BenWenger/FinalFantasyDisassembly
- **SRAM editor reference**: https://github.com/jdratlif/ffse

## FF2-Specific Details

- **SRAM format**: 8192 bytes = working area ($0000-$02FF) + 4 slots x 768 bytes at $0300/$0600/$0900/$0C00
- **Validation**: $5A at slot offset $FE, checksum at $FF
- **Checksum**: 8-bit byte sum (CLC before each ADC, no carry propagation), EOR $FF. Total of all 768 bytes must equal $FF. (0F/DA8F in disassembly)
- **Character data**: 4 chars x 64 bytes, split into two blocks (Properties 1 at $100, Properties 2 at $200)
- **Properties 1** (64 bytes): CharID/Guest(1), Status(1), Name(6), HP(2+2), MP(2+2), BaseStats(6: Str/Agi/Sta/Int/Spi/Mag), AttackStats(3), Equipment(5: Helmet/Armor/Gloves/RHand/LHand), Items(2), StatMods(6), OffhandStats(3), Defense(1), Evade(2+2), Elements(1), SpellPenalty(1), Spells(16)
- **Properties 2** (64 bytes): WeaponProficiency(16: 8 types x 2 bytes), SpellProficiency(32: 16 spells x 2 bytes), EvadeLvl/Exp(2+2), Presence/Row(1)
- **Gil**: 24-bit LE at slot offset $1C
- **Inventory**: 32 item IDs at $60 (no quantities — FF2 doesn't stack items)
- **Keywords**: 16 bytes at $80 (FF2's Ask/Learn/Memorize system)
- **Key items**: 16-bit bitmask at $1A-$1B (Canoe, Ring, Pass, Mythril, Snowcraft, Goddess' Bell, etc.)
- **Skill-based progression**: No XP/levels. Stats grow through use (weapon skills, spell proficiency, HP/MP growth)
- **Fan translations**: Demiforce (1998) and ChaosRush translations do NOT change save format
- **Reference disassembly**: https://github.com/everything8215/ff2 (ram-map.txt)

## FF3-Specific Details

- **SRAM format**: 8192 bytes = working area ($0000-$03FF) + 3 slots x 1024 bytes at $0400/$0800/$0C00
- **Validation**: $5A at slot offset $19, checksum at $1A. Also $55/$AA at $7F38/$7F39 for save count
- **Checksum**: 8-bit byte sum (CLC before each ADC, no carry propagation), EOR $FF. Total of all 1024 bytes must equal $FF. (3D/AE5C in disassembly)
- **Character data**: 4 chars x 64 bytes, split into two blocks (Block A at $100, Block B at $200)
- **Block A** (64 bytes): JobId(1), Level(1), Status(1), EXP(3, 24-bit LE), Name(6), CurrentHP(2), MaxHP(2), Int/Spi/Str/Agi/Vit(5), ModifiedStats(5), Elements/Defense(4), AttackStats(10: R+L hand), AbsorbedElements/DefenseStats(4), StatusImmunity(1), unused(1), MP per level(16: 8 levels x current/max)
- **Block B** (64 bytes): Equipment(7: Helmet/Armor/Gloves/RHand/RArrowQty/LHand/LArrowQty), Spells(8), Row/Flags(1), JobLevels(44: 22 jobs x 2 bytes level/exp)
- **Gil**: 24-bit LE at slot offset $1C, max 9,999,999
- **Inventory**: 32 IDs at $C0 + 32 quantities at $E0 (separate arrays)
- **Capacity Points**: At slot offset $1B
- **Crystal Level**: At slot offset $21 (0x00=locked, 0x01=Wind, 0x03=Fire, 0x07=Water, 0x0F=Earth, 0x1F=Eureka)
- **Fat Chocobo**: 256 bytes of item storage at $300-$3FF
- **Jobs** (22 total): OnionKid(0), Fighter(1), Monk(2), WhiteMage(3), BlackMage(4), RedMage(5), Hunter(6), Knight(7), Thief(8), Scholar(9), Geomancer(10), Dragoon(11), Viking(12), Karateka(13), MagicKnight(14), Conjurer(15), Bard(16), Warlock(17), Shaman(18), Summoner(19), Sage(20), Ninja(21)
- **Fan translation**: Neill Corlett/A.W. Jackson/SoM2Freak (1999) does NOT change save format
- **Reference disassembly**: https://github.com/everything8215/ff3 (field-ram.txt)
- **SRAM editor reference**: https://github.com/Binarynova/FF3jSRAMEditor

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
- OpenEmu NES saves use `.sav` extension in `~/Library/Application Support/OpenEmu/FCEUX/Battery Saves/`
- Filename must match ROM name exactly (e.g., `Final Fantasy II (USA) (Rev 1).sav`, `Final Fantasy (USA).sav`)
- **Auto save state overrides battery saves** — must delete the auto save state at `~/Library/Application Support/OpenEmu/Save States/<system>/<game>/Auto Save State.oesavestate` for a modified `.sav` to take effect
- OpenEmu must be fully quit before replacing save files
- Rev 0 and Rev 1 ROMs use the same save format
- NES fan translation patches (Demiforce FF2, Neill Corlett FF3) do not change the SRAM save format — the editor works with both Japanese and English-patched ROMs

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
