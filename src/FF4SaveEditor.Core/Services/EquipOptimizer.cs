using FF4SaveEditor.Core.GameData;
using FF4SaveEditor.Core.Models;

namespace FF4SaveEditor.Core.Services;

/// <summary>
/// Recommends optimal equipment for a character based on scoring profiles.
/// </summary>
public static class EquipOptimizer
{
    public record EquipRecommendation(
        ItemDef? RightHand,
        ItemDef? LeftHand,
        ItemDef? Helmet,
        ItemDef? Armor,
        ItemDef? Accessory);

    /// <summary>
    /// Recommend best equipment from available items for a character.
    /// </summary>
    public static EquipRecommendation Optimize(
        CharacterId characterId,
        IReadOnlyList<InventorySlot> inventory,
        OptimizeProfile profile)
    {
        var db = ItemDb.Instance;
        var available = inventory
            .Where(s => !s.IsEmpty)
            .Select(s => db.GetById(s.ItemId))
            .Where(item => item.CanEquip(characterId))
            .ToList();

        return OptimizeFromPool(characterId, available, profile);
    }

    /// <summary>
    /// Recommend theoretically best equipment regardless of inventory.
    /// </summary>
    public static EquipRecommendation OptimizeTheoretical(
        CharacterId characterId,
        OptimizeProfile profile)
    {
        var db = ItemDb.Instance;
        var allEquippable = db.All
            .Where(item => item.CanEquip(characterId) && item.Name != "(None)" && item.Name != "(Empty)")
            .ToList();

        return OptimizeFromPool(characterId, allEquippable, profile);
    }

    private static EquipRecommendation OptimizeFromPool(
        CharacterId characterId,
        List<ItemDef> pool,
        OptimizeProfile profile)
    {
        // Best helmet
        var bestHelmet = pool
            .Where(i => i.Category == ItemCategory.Helmet)
            .OrderByDescending(i => ScoreDefensiveItem(i, profile))
            .FirstOrDefault();

        // Best armor
        var bestArmor = pool
            .Where(i => i.Category == ItemCategory.BodyArmor)
            .OrderByDescending(i => ScoreDefensiveItem(i, profile))
            .FirstOrDefault();

        // Best accessory
        var bestAccessory = pool
            .Where(i => i.Category == ItemCategory.Accessory)
            .OrderByDescending(i => ScoreAccessory(i, profile))
            .FirstOrDefault();

        // Weapons and shields are interdependent (two-handed, bow+arrow, dual-wield)
        var (bestRight, bestLeft) = OptimizeHands(characterId, pool, profile);

        return new EquipRecommendation(bestRight, bestLeft, bestHelmet, bestArmor, bestAccessory);
    }

    private static (ItemDef? Right, ItemDef? Left) OptimizeHands(
        CharacterId characterId,
        List<ItemDef> pool,
        OptimizeProfile profile)
    {
        var weapons = pool.Where(i => i.Category == ItemCategory.Weapon && !i.IsArrow).ToList();
        var arrows = pool.Where(i => i.IsArrow).ToList();
        var shields = pool.Where(i => i.Category == ItemCategory.Shield).ToList();

        bool canDualWield = characterId == CharacterId.Edge;

        double bestScore = 0;
        ItemDef? bestRight = null;
        ItemDef? bestLeft = null;

        foreach (var weapon in weapons)
        {
            if (weapon.TwoHanded)
            {
                double score = ScoreWeapon(weapon, profile);
                if (score > bestScore) { bestScore = score; bestRight = weapon; bestLeft = null; }
            }
            else if (weapon.IsBow)
            {
                // Bow + best arrow
                var bestArrow = arrows.OrderByDescending(a => ScoreWeapon(a, profile)).FirstOrDefault();
                double score = ScoreWeapon(weapon, profile) + (bestArrow != null ? ScoreWeapon(bestArrow, profile) : 0);
                if (score > bestScore) { bestScore = score; bestRight = weapon; bestLeft = bestArrow; }
            }
            else
            {
                // Weapon + shield
                var bestShield = shields.OrderByDescending(s => ScoreDefensiveItem(s, profile)).FirstOrDefault();
                double shieldScore = bestShield != null ? ScoreDefensiveItem(bestShield, profile) : 0;
                double score = ScoreWeapon(weapon, profile) + shieldScore;
                if (score > bestScore) { bestScore = score; bestRight = weapon; bestLeft = bestShield; }

                // Dual-wield (Edge only)
                if (canDualWield)
                {
                    foreach (var offhand in weapons.Where(w => w != weapon && !w.TwoHanded && !w.IsBow))
                    {
                        double dualScore = ScoreWeapon(weapon, profile) + ScoreWeapon(offhand, profile);
                        if (dualScore > bestScore) { bestScore = dualScore; bestRight = weapon; bestLeft = offhand; }
                    }
                }
            }
        }

        // Also consider shield-only if no weapon is better
        if (bestRight == null && shields.Count > 0)
        {
            bestLeft = shields.OrderByDescending(s => ScoreDefensiveItem(s, profile)).FirstOrDefault();
        }

        return (bestRight, bestLeft);
    }

    private static double ScoreWeapon(ItemDef item, OptimizeProfile profile) => profile switch
    {
        OptimizeProfile.PhysicalAttack => item.Attack * 2.0 + item.HitRate * 0.5 + StatBonus(item, profile),
        OptimizeProfile.MagicPower => item.Attack * 0.5 + StatBonus(item, profile),
        OptimizeProfile.Defense => item.Attack * 0.5 + item.Defense + StatBonus(item, profile),
        _ => item.Attack * 1.5 + item.HitRate * 0.3 + StatBonus(item, profile), // Balanced
    };

    private static double ScoreDefensiveItem(ItemDef item, OptimizeProfile profile) => profile switch
    {
        OptimizeProfile.PhysicalAttack => item.Defense * 0.5 + item.Evasion * 0.3 + StatBonus(item, profile),
        OptimizeProfile.MagicPower => item.MagicDefense * 1.5 + item.Defense * 0.5 + StatBonus(item, profile),
        OptimizeProfile.Defense => item.Defense * 2.0 + item.MagicDefense * 1.5 + item.Evasion * 0.5 + StatBonus(item, profile),
        _ => item.Defense + item.MagicDefense + item.Evasion * 0.3 + StatBonus(item, profile), // Balanced
    };

    private static double ScoreAccessory(ItemDef item, OptimizeProfile profile)
        => item.Defense + item.MagicDefense + StatBonus(item, profile);

    private static double StatBonus(ItemDef item, OptimizeProfile profile)
    {
        if (item.StatBonuses == null) return 0;
        var b = item.StatBonuses;
        return profile switch
        {
            OptimizeProfile.PhysicalAttack => b.Strength * 3.0 + b.Agility * 1.5 + b.Stamina * 0.5,
            OptimizeProfile.MagicPower => b.Intellect * 3.0 + b.Spirit * 2.0 + b.Agility * 0.5,
            OptimizeProfile.Defense => b.Stamina * 3.0 + b.Agility * 1.5 + b.Spirit * 1.0,
            _ => b.Strength + b.Agility + b.Stamina + b.Intellect + b.Spirit, // Balanced
        };
    }
}
