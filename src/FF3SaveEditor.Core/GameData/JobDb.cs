namespace FF3SaveEditor.Core.GameData;

/// <summary>
/// FF3 job definitions. 22 jobs (0-21), unlocked by crystal level.
/// </summary>
public static class JobDb
{
    public static readonly string[] JobNames =
    {
        "Onion Kid",    // 0
        "Fighter",      // 1
        "Monk",         // 2
        "White Mage",   // 3
        "Black Mage",   // 4
        "Red Mage",     // 5
        "Hunter",       // 6
        "Knight",       // 7
        "Thief",        // 8
        "Scholar",      // 9
        "Geomancer",    // 10
        "Dragoon",      // 11
        "Viking",       // 12
        "Karateka",     // 13
        "Magic Knight", // 14
        "Conjurer",     // 15
        "Bard",         // 16
        "Warlock",      // 17
        "Shaman",       // 18
        "Summoner",     // 19
        "Sage",         // 20
        "Ninja",        // 21
    };

    /// <summary>
    /// Crystal level bitmask required to unlock each job.
    /// 0x00=locked, 0x01=Wind, 0x03=Fire, 0x07=Water, 0x0F=Earth, 0x1F=Eureka.
    /// </summary>
    public static readonly byte[] CrystalRequirement =
    {
        0x00, // Onion Kid (always available)
        0x01, // Fighter (Wind)
        0x01, // Monk (Wind)
        0x01, // White Mage (Wind)
        0x01, // Black Mage (Wind)
        0x01, // Red Mage (Wind)
        0x01, // Hunter (Wind)
        0x03, // Knight (Fire)
        0x03, // Thief (Fire)
        0x03, // Scholar (Fire)
        0x03, // Geomancer (Fire)
        0x03, // Dragoon (Fire)
        0x07, // Viking (Water)
        0x07, // Karateka (Water)
        0x07, // Magic Knight (Water)
        0x07, // Conjurer (Water)
        0x07, // Bard (Water)
        0x0F, // Warlock (Earth)
        0x0F, // Shaman (Earth)
        0x0F, // Summoner (Earth)
        0x1F, // Sage (Eureka)
        0x1F, // Ninja (Eureka)
    };

    public static string GetJobName(byte jobId)
        => jobId < JobNames.Length ? JobNames[jobId] : $"Unknown ({jobId})";
}
