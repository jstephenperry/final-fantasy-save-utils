using FF4SaveEditor.Core.Models;

namespace FF4SaveEditor.Core.Services;

public record StoryMilestone(string Name, string Description, bool Completed);

/// <summary>
/// Determines story progression from save slot data using the game's event switch flags
/// (32 bytes / 256 bits at slot offset $0280-$029F), the world byte ($0701), and
/// party composition as a fallback for late-game milestones.
///
/// Event switch IDs are from the ff4 disassembly (https://github.com/everything8215/ff4).
/// Some switches are state flags (toggled on/off) rather than permanent event flags,
/// so a monotonicity pass ensures that if a later milestone is complete, all earlier
/// milestones are also marked complete.
/// </summary>
public static class StoryProgress
{
    private const int EventSwitchOffset = 0x280;
    private const int EventSwitchSize = 32;
    private const int WorldOffset = 0x701;

    // --- Known event switch IDs (from ff4 disassembly event_switch table) ---

    // Act 1: The Dark Knight's Journey
    private const int SwDefeatedMistDragon = 13;
    private const int SwOpenedPackage = 14;        // Mist village destroyed
    private const int SwReachedKaipo = 15;
    private const int SwCuredRosa = 17;
    private const int SwDefeatedAntlion = 18;
    private const int SwDefeatedOctomamm = 19;
    private const int SwDamcyanDestroyed = 20;
    private const int SwDefeatedMombomb = 22;
    private const int SwRestedAfterFabul = 44;

    // Act 2: The Path to Paladin
    private const int SwPalomPoromJoined = 28;
    private const int SwCecilBecamePaladin = 11;

    // Act 3: The Crystals
    private const int SwObtainedBaronKey = 5;
    private const int SwObtainedTwinHarp = 36;
    private const int SwObtainedEarthCrystal = 35;
    private const int SwUsedTwinHarp = 46;
    private const int SwDefeatedMagusSisters = 42;

    // Act 4: The Underworld
    private const int SwPassageToUndergroundOpen = 48;
    private const int SwReturnedEarthCrystal = 49;
    private const int SwDefeatedDrLugae = 51;
    private const int SwEnterpriseHasHook = 54;
    private const int SwFalconOverLava = 57;
    private const int SwFalconHasDrill = 61;

    // Optional bosses
    private const int SwDefeatedAsura = 68;
    private const int SwDefeatedLeviatan = 69;
    private const int SwDefeatedBahamut = 70;

    private static bool GetSwitch(byte[] switches, int id)
        => (switches[id >> 3] & (1 << (id & 7))) != 0;

    public static List<StoryMilestone> Analyze(SaveSlot slot)
    {
        // Copy event switches to a byte array (ReadOnlySpan can't be captured in closures)
        var switches = new byte[EventSwitchSize];
        var raw = slot.RawData;
        for (int i = 0; i < EventSwitchSize; i++)
            switches[i] = raw[EventSwitchOffset + i];

        byte world = raw[WorldOffset]; // 0=overworld, 1=underground, 2=moon
        bool onMoon = world == 2;
        bool inUnderworld = world >= 1; // underground or moon

        // Party composition (fallback for late-game milestones without known switches)
        var charIds = new HashSet<CharacterId>();
        for (int i = 0; i < SaveSlot.CharacterCount; i++)
        {
            var c = slot.Characters[i];
            if (!c.IsEmpty)
                charIds.Add(c.CharacterId);
        }

        bool Sw(int id) => GetSwitch(switches, id);
        bool Has(CharacterId id) => charIds.Contains(id);

        var milestones = new List<StoryMilestone>();
        void Add(string name, string desc, bool done)
            => milestones.Add(new StoryMilestone(name, desc, done));

        // === Act 1: The Dark Knight's Journey ===

        Add("Raid on Mysidia",
            "Cecil delivers the Bomb Ring to Baron and questions the king's orders",
            true); // Can't save before this

        Add("Mist Cave",
            "Cecil and Kain slay the Mist Dragon",
            Sw(SwDefeatedMistDragon));

        Add("Village of Mist",
            "The Bomb Ring destroys Mist; Rydia joins Cecil",
            Sw(SwOpenedPackage));

        Add("Kaipo - Rescued Rydia",
            "Cecil protects Rydia from Baron's soldiers at the inn",
            Sw(SwReachedKaipo));

        Add("Underground Waterway",
            "Cecil, Rydia, and Tellah traverse the waterway; defeat Octomamm",
            Sw(SwDefeatedOctomamm));

        Add("Damcyan Castle",
            "Edward joins after Anna's death; Tellah departs seeking revenge",
            Sw(SwDamcyanDestroyed));

        Add("Antlion Cave",
            "Defeat the Antlion and obtain the SandRuby to cure Rosa",
            Sw(SwDefeatedAntlion));

        Add("Rosa Cured in Kaipo",
            "Rosa is healed with the SandRuby and joins the party",
            Sw(SwCuredRosa));

        Add("Mt. Hobs",
            "Yang joins the party; defeat Mombomb",
            Sw(SwDefeatedMombomb));

        Add("Battle of Fabul",
            "Defend Fabul Castle from Baron's forces; Kain kidnaps Rosa",
            Sw(SwRestedAfterFabul));

        // === Act 2: The Path to Paladin ===

        Add("Shipwreck - Mysidia",
            "Cecil washes ashore alone after Leviathan's attack; Palom and Porom join",
            Sw(SwPalomPoromJoined));

        Add("Mt. Ordeals",
            "Cecil becomes a Paladin; Tellah learns Meteo",
            Sw(SwCecilBecamePaladin));

        // === Act 3: The Crystals ===

        Add("Return to Baron",
            "Infiltrate Baron with Palom, Porom, and Tellah",
            Sw(SwObtainedBaronKey));

        Add("Baron Castle - Defeat Cagnazzo",
            "Defeat Cagnazzo; Palom and Porom sacrifice themselves",
            Sw(SwObtainedTwinHarp) || Sw(SwObtainedEarthCrystal));
        // No direct Cagnazzo switch known; acquiring the TwinHarp or Earth Crystal
        // requires having defeated Cagnazzo first

        Add("Troia - TwinHarp from Edward",
            "Edward sends the TwinHarp from his sickbed",
            Sw(SwObtainedTwinHarp));

        Add("Lodestone Cavern - Dark Elf",
            "Use the TwinHarp to defeat the Dark Elf; obtain the Earth Crystal",
            Sw(SwObtainedEarthCrystal));

        Add("Tower of Zot",
            "Tellah confronts Golbez and falls; rescue Rosa and Kain",
            Sw(SwDefeatedMagusSisters));

        // === Act 4: The Underworld ===

        Add("Passage to the Underworld",
            "Use the Magma Key to open the way underground",
            Sw(SwPassageToUndergroundOpen) || Sw(SwReturnedEarthCrystal) || Sw(SwDefeatedDrLugae));
        // Switch 48 is a state flag cleared when Cid seals the hole;
        // later underworld switches prove the passage was opened

        Add("Dwarven Castle",
            "Arrive in the Underworld; deliver the Earth Crystal to King Giott",
            Sw(SwReturnedEarthCrystal) || Sw(SwDefeatedDrLugae));

        Add("Tower of Babil (Underworld)",
            "Infiltrate the tower; defeat Dr. Lugae",
            Sw(SwDefeatedDrLugae));

        Add("Rydia Returns",
            "Adult Rydia rejoins the party from the Land of Summons",
            Sw(SwEnterpriseHasHook) || Has(CharacterId.RydiaAdult));

        Add("Edge Joins - Eblan Cave",
            "Prince Edge of Eblan joins the party",
            Sw(SwEnterpriseHasHook) || Has(CharacterId.Edge));

        Add("Tower of Babil (Upper)",
            "Ascend the tower; Cid seals the Underworld entrance",
            Sw(SwEnterpriseHasHook));

        Add("Sealed Cave",
            "Obtain the Dark Crystal; Kain betrays the party",
            Sw(SwFalconHasDrill) && (Sw(SwEnterpriseHasHook) || inUnderworld));
        // Drill is obtained before Sealed Cave; being in the underworld or having
        // the hook (a later flag) confirms progression past the Drill acquisition

        // === Act 5: The Moon ===

        Add("Giant of Babil",
            "The Giant attacks; all allies rally to stop it",
            onMoon);
        // Being on the moon requires completing the Giant of Babil

        Add("Arrived on the Moon",
            "Board the Lunar Whale and fly to the moon",
            onMoon);

        // === Optional Bosses (not ordered, shown separately) ===

        Add("Defeated Asura",
            "Optional: Defeat Asura in the Land of Summons",
            Sw(SwDefeatedAsura));

        Add("Defeated Leviathan",
            "Optional: Defeat Leviathan in the Land of Summons",
            Sw(SwDefeatedLeviatan));

        Add("Defeated Bahamut",
            "Optional: Defeat Bahamut on the moon",
            Sw(SwDefeatedBahamut));

        // Monotonicity pass: story milestones are chronological, so if a later
        // milestone is complete, all earlier ones must also be complete.
        // Optional bosses at the end are excluded from this pass.
        int lastStoryIndex = milestones.Count - 4; // Last index before optional bosses
        EnsureMonotonicity(milestones, 0, lastStoryIndex);

        return milestones;
    }

    /// <summary>
    /// If milestone[j] is completed, mark all milestone[0..j-1] as completed too.
    /// This handles cases where an earlier milestone's specific flag is a toggle
    /// that was cleared, but a later milestone proves the earlier one happened.
    /// </summary>
    private static void EnsureMonotonicity(List<StoryMilestone> milestones, int start, int end)
    {
        // Find the highest completed milestone index in range
        int highestCompleted = -1;
        for (int i = end; i >= start; i--)
        {
            if (milestones[i].Completed)
            {
                highestCompleted = i;
                break;
            }
        }

        if (highestCompleted < 0) return;

        // Mark all earlier milestones as completed
        for (int i = start; i < highestCompleted; i++)
        {
            if (!milestones[i].Completed)
                milestones[i] = milestones[i] with { Completed = true };
        }
    }
}
