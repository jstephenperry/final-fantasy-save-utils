using FF4SaveEditor.Core.GameData;
using FF4SaveEditor.Core.Models;
using FF4SaveEditor.Core.Services;

namespace FF4SaveEditor.Tests;

public class EquipOptimizerTests
{
    [Fact]
    public void OptimizeTheoretical_CecilPaladin_RecommendsBestSword()
    {
        var rec = EquipOptimizer.OptimizeTheoretical(
            CharacterId.CecilPaladin,
            OptimizeProfile.PhysicalAttack);

        Assert.NotNull(rec.RightHand);
        // Cecil Paladin should get Crystal (200 atk) or similar top weapon
        Assert.True(rec.RightHand!.Attack >= 100,
            $"Expected high-attack weapon, got {rec.RightHand.Name} (ATK {rec.RightHand.Attack})");
    }

    [Fact]
    public void OptimizeTheoretical_Edge_CanDualWield()
    {
        var rec = EquipOptimizer.OptimizeTheoretical(
            CharacterId.Edge,
            OptimizeProfile.PhysicalAttack);

        // Edge should get a weapon in both hands (dual-wield)
        Assert.NotNull(rec.RightHand);
        Assert.NotNull(rec.LeftHand);
        // Left hand should be a weapon (not a shield) for attack profile
        Assert.Equal(ItemCategory.Weapon, rec.LeftHand!.Category);
    }

    [Fact]
    public void OptimizeTheoretical_RecommendsArmor()
    {
        var rec = EquipOptimizer.OptimizeTheoretical(
            CharacterId.CecilPaladin,
            OptimizeProfile.Defense);

        Assert.NotNull(rec.Armor);
        Assert.NotNull(rec.Helmet);
        Assert.True(rec.Armor!.Defense > 0);
    }

    [Fact]
    public void Optimize_FromInventory_OnlyUsesAvailableItems()
    {
        var inventory = new InventorySlot[]
        {
            InventorySlot.FromBytes(63, 1),  // Crystal sword
            InventorySlot.FromBytes(224, 5), // Cure1 (not equippable)
        };

        var rec = EquipOptimizer.Optimize(
            CharacterId.CecilPaladin,
            inventory,
            OptimizeProfile.Balanced);

        // Should recommend the Crystal sword since it's the only equippable weapon
        Assert.NotNull(rec.RightHand);
        Assert.Equal(63, rec.RightHand!.Id);
    }

    [Fact]
    public void Optimize_EmptyInventory_ReturnsNulls()
    {
        var inventory = Array.Empty<InventorySlot>();

        var rec = EquipOptimizer.Optimize(
            CharacterId.CecilPaladin,
            inventory,
            OptimizeProfile.Balanced);

        Assert.Null(rec.RightHand);
        Assert.Null(rec.Helmet);
        Assert.Null(rec.Armor);
        Assert.Null(rec.Accessory);
    }

    [Fact]
    public void ItemDb_LoadsAllItems()
    {
        var db = ItemDb.Instance;
        Assert.Equal(256, db.All.Count);
    }

    [Fact]
    public void ItemDb_LookupById()
    {
        var db = ItemDb.Instance;
        var crystal = db.GetById(63);
        Assert.Equal("Crystal", crystal.Name);
        Assert.Equal(ItemCategory.Weapon, crystal.Category);
        Assert.Equal(200, crystal.Attack);
    }

    [Fact]
    public void ItemDb_FiltersByCategory()
    {
        var db = ItemDb.Instance;
        var weapons = db.GetByCategory(ItemCategory.Weapon).ToList();
        Assert.True(weapons.Count > 0);
        Assert.All(weapons, w => Assert.Equal(ItemCategory.Weapon, w.Category));
    }
}
