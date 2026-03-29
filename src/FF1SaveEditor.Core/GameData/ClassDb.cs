namespace FF1SaveEditor.Core.GameData;

/// <summary>
/// FF1 character class definitions.
/// 6 base classes (0-5) and 6 promoted classes (6-11).
/// </summary>
public static class ClassDb
{
    public static readonly string[] ClassNames =
    {
        "Fighter",    // 0
        "Thief",      // 1
        "Black Belt", // 2
        "Red Mage",   // 3
        "White Mage", // 4
        "Black Mage", // 5
        "Knight",     // 6
        "Ninja",      // 7
        "Master",     // 8
        "Red Wizard",  // 9
        "White Wizard", // 10
        "Black Wizard", // 11
    };

    public static string GetClassName(byte classId)
        => classId < ClassNames.Length ? ClassNames[classId] : $"Unknown ({classId})";
}
