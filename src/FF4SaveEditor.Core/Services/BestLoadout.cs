using FF4SaveEditor.Core.Models;

namespace FF4SaveEditor.Core.Services;

/// <summary>
/// Returns the best-in-slot equipment for each character, aligned with
/// community strategy guides for the SNES version.
///
/// Key principles:
///   - Physical fighters: highest ATK weapon
///   - Mages (Rydia, Palom, Tellah, FuSoYa): Stardust Rod for INT+5 (magic damage)
///   - Ranged/healers (Rosa, Porom, Edward): Artemis Bow + Arrow
///   - Helmet: Ribbon for most; Tiara (INT+5) for Rydia child; Crystal Helm for Cecil
///   - Armor: Adamant (DEF 100, all stats +5) universally
///   - Accessory: Crystal Ring (DEF 20, MDef 12, AGI+5) universally
/// </summary>
public static class BestLoadout
{
    public record struct Equipment(byte RightHand, byte LeftHand, byte Helmet, byte Armor, byte Accessory);

    // --- Weapons ---
    private const byte CrystalSword = 0x3F;   // ATK 200, 2H — Ragnarok in later localizations (Cecil Paladin)
    private const byte WhiteSpear = 0x28;      // ATK 109, 2H (Kain)
    private const byte RuneAxe = 0x48;         // ATK 100, 2H (Cid)
    private const byte ArtemisBow = 0x53;      // ATK  80, bow (Rosa, Porom, Edward)
    private const byte ArtemisArrow = 0x5F;    // ATK  75, arrow
    private const byte Masamune = 0x30;        // ATK  65, 2H (Edge)
    private const byte Murasame = 0x2F;        // ATK  55, 2H (Edge dual-wield)
    private const byte StardustRod = 0x0D;     // ATK  45, INT+5 — best for mages (magic damage)
    private const byte BlackSword = 0x18;      // ATK  30, 2H (Cecil DK)
    private const byte CatClaw = 0x06;         // HIT  99, STR+5 AGI+5 (Yang)

    // --- Helmets ---
    private const byte Ribbon = 0x7C;          // DEF  9, MDef 12, Eva 12, MEva 12 (universal)
    private const byte Tiara = 0x7B;           // DEF  7, MDef 10, INT+5 (Rydia/Rosa/Porom/CecilPal)
    private const byte CrystalHelm = 0x76;     // DEF 12, MDef  8, SPI+5 (Cecil Paladin only)
    private const byte BlackHelm = 0x70;       // DEF  6, MDef  1 (Cecil DK only)

    // --- Armor ---
    private const byte AdamantArmor = 0x9A;    // DEF 100, MDef 20, all stats +5 (universal)
    private const byte BlackArmor = 0x84;      // DEF   9, MDef  3 (Cecil DK only)

    // --- Accessories ---
    private const byte CrystalRing = 0xAC;     // DEF 20, MDef 12, AGI+5 (universal)
    private const byte BlackGauntlet = 0x9F;   // DEF  4 (Cecil DK only)

    private const byte None = 0x00;

    /// <summary>
    /// Returns the best-in-slot equipment for the given character.
    /// Handles rejoined character variants (e.g. Kain2, Rosa2) automatically.
    /// </summary>
    public static Equipment Get(CharacterId id) => id switch
    {
        // === Physical fighters ===

        // Cecil DK: limited to Black equipment set; all weapons 2H
        CharacterId.CecilDarkKnight => new(BlackSword, None, BlackHelm, BlackArmor, BlackGauntlet),

        // Cecil Paladin: Crystal sword (ATK 200, 2H); Crystal Helm (exclusive)
        CharacterId.CecilPaladin => new(CrystalSword, None, CrystalHelm, AdamantArmor, CrystalRing),

        // Kain: White spear (ATK 109, 2H) — best spear
        CharacterId.Kain or CharacterId.Kain2 or CharacterId.Kain3
            => new(WhiteSpear, None, Ribbon, AdamantArmor, CrystalRing),

        // Yang: CatClaw in both hands (HIT 99, STR+5 AGI+5; claws have ATK 0, damage from stats)
        CharacterId.Yang or CharacterId.Yang2
            => new(CatClaw, CatClaw, Ribbon, AdamantArmor, CrystalRing),

        // Edge: dual-wield Masamune (ATK 65) + Murasame (ATK 55)
        CharacterId.Edge => new(Masamune, Murasame, Ribbon, AdamantArmor, CrystalRing),

        // Cid: RuneAxe (ATK 100, 2H)
        CharacterId.Cid => new(RuneAxe, None, Ribbon, AdamantArmor, CrystalRing),

        // === Mages: Stardust Rod (INT+5) for magic damage ===

        // Rydia child: Stardust Rod + Tiara (INT+5); child Rydia CAN equip Tiara
        CharacterId.RydiaChild => new(StardustRod, None, Tiara, AdamantArmor, CrystalRing),

        // Rydia adult: Stardust Rod + Ribbon (adult Rydia cannot equip Tiara per equip mask)
        CharacterId.RydiaAdult => new(StardustRod, None, Ribbon, AdamantArmor, CrystalRing),

        // Tellah: Stardust Rod for offensive magic (Meteo)
        CharacterId.Tellah or CharacterId.Tellah2 or CharacterId.Tellah3
            => new(StardustRod, None, Ribbon, AdamantArmor, CrystalRing),

        // Palom: Stardust Rod — black mage, INT+5 boosts all offensive spells
        CharacterId.Palom => new(StardustRod, None, Ribbon, AdamantArmor, CrystalRing),

        // FuSoYa: Stardust Rod — versatile mage
        CharacterId.FuSoYa => new(StardustRod, None, Ribbon, AdamantArmor, CrystalRing),

        // Golbez: uses Tellah's equip class
        CharacterId.Golbez => new(StardustRod, None, Ribbon, AdamantArmor, CrystalRing),

        // === Ranged / Healers: Artemis Bow + Arrow ===

        // Rosa: primary healer but Artemis Bow (ATK 80+75) for back-row versatility
        CharacterId.Rosa or CharacterId.Rosa2
            => new(ArtemisBow, ArtemisArrow, Ribbon, AdamantArmor, CrystalRing),

        // Porom: white mage, Artemis Bow for same reason as Rosa
        CharacterId.Porom => new(ArtemisBow, ArtemisArrow, Ribbon, AdamantArmor, CrystalRing),

        // Edward: Artemis Bow + Arrow (harps are much weaker)
        CharacterId.Edward => new(ArtemisBow, ArtemisArrow, Ribbon, AdamantArmor, CrystalRing),

        _ => default,
    };
}
