﻿using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.Composition;
using ZzukBot.Constants;
using ZzukBot.ExtensionFramework.Classes;
using ZzukBot.Game.Statics;
using ZzukBot.Objects;

/**
 * to test: 
 * test bandages,
 * poisons,
 * ranged pull,
 * Food selection
 * 
 * 
 * latest change: 
 * combat distances
 * 
 *confirmed working:
 * Kick all
 * SnD / Evisc
 * Riposte
 * Racials/Blade Flurry/AR
 * -> basically the full fighting arsenal.
 * 
 * 
 * Huge credits to krycess' QuickDraw for all of this.
 */

[Export(typeof(CustomClass))]
public class Rogue : CustomClass
{
    bool useKick = true;
    bool log = true;

    public override void Dispose() { }
    public override bool Load() { return true; }

    //Credits for the logger to baka
    public static void DebugMsg(string String)
    {
        ZzukBot.ExtensionMethods.StringExtensions.Log("Debug: " + String, "RogueLog.txt", true);
        Lua.Instance.Execute("DEFAULT_CHAT_FRAME:AddMessage(\"DEBUG: " + String + "\");");
    }

    public bool HavePoison()
    {
        string poisonToUse = Inventory.Instance.GetLastItem(poisonList);
        if (poisonToUse != "")
            return true;
        else return false;
    }

    public string[] poisonList =
    {
        "Instant Poison", "Instant Poison II", "Instant Poison III", "Instant Poison IV", "Instant Poison V", "Instant Poison VI"
    };

    public void EnchantPoison()
    {
        string poisonToUse = Inventory.Instance.GetLastItem(poisonList);
        if (!Local.IsMainhandEnchanted())
        {
            Local.EnchantMainhandItem(poisonToUse);
        }
        if (!Local.IsOffhandEnchanted())
        {
            Local.EnchantOffhandItem(poisonToUse);
        }
    }

    public override bool OnBuff()
    {//Buff TODO: add poisons
        if (HavePoison() && (!Local.IsMainhandEnchanted() || !Local.IsOffhandEnchanted()))
        {
            EnchantPoison();
            return false;
        }
        return true;
    }

    public bool HaveBandage()
    {
        if (Inventory.Instance.GetLastItem(firstAidBandages) != "")
            return true;
        else return false;
    }

    public string[] firstAidBandages =
       {
            "Linen Bandage", "Heavy Linen Bandage", "Wool Bandage", "Heavy Wool Bandage",
            "Silk Bandage", "Heavy Silk Bandage", "Mageweave Bandage",
            "Heavy Mageweave Bandage", "Runecloth Bandage", "Heavy Runecloth Bandage"
        };

    public void SelectBandage()
    { //TODO: implement this in OnRest
        string bandageToUse = Inventory.Instance.GetLastItem(firstAidBandages);
        WoWItem Bandage = Inventory.Instance.GetItem(bandageToUse);
        Bandage.Use();
    }

    public string[] foodNames =
    {
            "Tough Hunk of Bread", "Darnassian Bleu", "Slitherskin Mackeral",
            "Shiny Red Apple", "Forest Mushroom Cap", "Tough Jerky",
            "Freshly Baked Bread", "Dalaran Sharp", "Longjaw Mud Snapper",
            "Tel'Abim Banana", "Red-speckled Mushroom", "Haunch of Meat",
            "Moist Cornbread", "Dwarven Mild", "Bristle Whisker Catfish",
            "Snapvine Watermelon", "Spongy Morel", "Mutton Chop",
            "Mulgore Spice Bread", "Stormwind Brie", "Rockscale Cod",
            "Goldenbark Apple", "Delicious Cave Mold", "Wild Hog Shank",
            "Soft Banana Bread", "Fine Aged Cheddar", "Spotted Yellowtail",
            "Striped Yellowtail", "Moon Harvest Pumpkin", "Raw Black Truffle",
            "Cured Ham Steak", "Homemade Cherry Pie", "Alterac Swiss",
            "Spinefin Halibut", "Deep Fried Plantains", "Dried King Bolete",
            "Roasted Quail", "Conjured Sweet Roll", "Conjured Cinnamon Roll"
    };

    public void SelectFood()
    {
        string foodToUse = Inventory.Instance.GetLastItem(foodNames);
        Local.Eat(foodToUse);
    }


    public string[] potNames = { "Minor Healing Potion", "Lesser Healing Potion", "Discolored Healing Potion", "Healing Potion", "Greater Healing Potion", "Superior Healing Potion", "Major Healing Potion" };


    public void SelectHPotion()
    {
        string potToUse = Inventory.Instance.GetLastItem(potNames);
        if (potToUse != "")
        {
            WoWItem Potion = Inventory.Instance.GetItem(potToUse);
            if (Local.HealthPercent <= 20)
                Potion.Use();
        }
    }


    public void KickEnemy()
    {   //If this works as intended, the bot will try and silence any casting add, not just hte primary target.
        if (useKick && Attackers.Count >= 1)
        {
            int properTargetH = UnitInfo.Instance.NpcAttackers.Min(Target => Target.HealthPercent);
            var properTarget = UnitInfo.Instance.NpcAttackers.FirstOrDefault(Target => Target.HealthPercent == properTargetH);
            var castingUnit = UnitInfo.Instance.NpcAttackers.FirstOrDefault(Caster => Caster.Casting != 0 || Caster.Channeling != 0);
            if (castingUnit != null && Spell.Instance.IsSpellReady("Kick"))
            {
                if (log) DebugMsg("Found casting unit, trying to kick..");
                Spell.Instance.StopCasting();
                if (castingUnit.Guid != Target.Guid)
                {
                    Local.SetTarget(castingUnit);
                }
                if (Target.Casting != 0 || Target.Channeling != 0)
                {
                    Spell.Instance.Cast("Kick");
                }
                Local.SetTarget(properTarget);
                return;
            }
        }
    }

    private int[][] EviscerateDamage = new int[][]
            {
                new int[] {0, 0,0,0,0},
                new int[] {10, 10,15,20,25,30},
                new int[] {20, 22,33,44,55,66},
                new int[] {30, 39,58,77,96,115},
                new int[] {50, 61,92,123,154,185},
                new int[] {60, 60,135,180,225,270},
                new int[] {80, 143,220,297,374,451},
                new int[] {100, 212,322,432,542,652},
                new int[] {150, 295,446,597,748,899},
                new int[] {200, 332,502,672,842,1012},
            };


    private bool ShouldWeEviscerate()
    {
        int rank = Spell.Instance.GetSpellRank("Eviscerate");
        int damage = 0;
        int comboPoints = Local.ComboPoints;
        DebugMsg("Stats for Evis Calculation: Target Health: " + Target.Health + ", Rank: " + rank + ", calculated damage: " + damage);
        if (rank >= 1 && comboPoints >= 1)
        {
            damage = (EviscerateDamage[rank][comboPoints] + EviscerateDamage[rank][0]);
            DebugMsg("Stats for Evis Calculation: Target Health: " + Target.Health + ", Rank: " + rank + ", calculated damage: " + damage);
            if (Target.Health <= damage)
                return true;
        }
        return false;
    }

    public void HandleMultipleEnemies()
    {
        if (Attackers.Count >= 2)
        {
            if (Spell.Instance.GetSpellRank("Adrenaline Rush") != 0)
            {
                if (Spell.Instance.IsSpellReady("Adrenaline Rush"))
                    Spell.Instance.Cast("Adrenaline Rush");
            }
            if (Spell.Instance.GetSpellRank("Blood Fury") != 0)
            {
                if (Spell.Instance.IsSpellReady("Blood Fury"))
                    Spell.Instance.Cast("Blood Fury");
            }
            if (Spell.Instance.GetSpellRank("Blade Flurry") != 0)
            {
                if (Spell.Instance.IsSpellReady("Blade Flurry"))
                    Spell.Instance.Cast("Blade Flurry");
            }
            if (Spell.Instance.GetSpellRank("Evasion") != 0)
            {
                if (Spell.Instance.IsSpellReady("Evasion"))
                    Spell.Instance.Cast("Evasion");
            }
        }

    }

    private bool Riposte()
    {
        if (Spell.Instance.GetSpellRank("Riposte") != 0)
        {
            if (Spell.Instance.IsSpellReady("Riposte"))
                return true;
        }
        return false;
    }

    public void GeneralCombat()
    {
        int energy = Local.Energy;
        int comboPoint = Local.ComboPoints;
        if (energy >= 10 && Riposte())
            Spell.Instance.Cast("Riposte");
        if (energy <= 20)
            return;
        if (energy >= 25)
            KickEnemy();
        if (energy >= 35)
        {
            if (Spell.Instance.GetSpellRank("Eviscerate") != 0)
            {
                int eviscrank = Spell.GetSpellRank("Eviscerate");
                DebugMsg("Checking for Eviscerate. Current Rank: " + eviscrank + ", CPs: " + comboPoint  );
                if (ShouldWeEviscerate() || comboPoint == 5)
                {
                    Spell.Instance.Cast("Eviscerate");
                    return;
                }
            }
        }
        if (energy >= 25)
        {
            if (Spell.Instance.GetSpellRank("Slice and Dice") != 0)
            {
                if (!Local.GotAura("Slice and Dice") && !ShouldWeEviscerate() && comboPoint > 0)
                {
                    Spell.Instance.Cast("Slice and Dice");
                    return;
                }
            }
            if (energy >= 40)
                Spell.Instance.Cast("Sinister Strike");
        }
    }

    public override void OnFight()
    {//Fight
        CombatDistance = 5;
        int energy = Local.Energy;
        int comboPoint = Local.ComboPoints;
        SelectHPotion();
        Spell.Instance.Attack();
        KickEnemy();
        HandleMultipleEnemies();
        GeneralCombat();
    }

    public override void OnPull()
    {//PreFight
        //if (Target.DistanceToPlayer > 8 && Target.DistanceToPlayer < 27)
        //{//tbd: non hardcoded ranged pull cast, safety check on whether to ranged pull or melee pull
        //    if (log)
        //        DebugMsg("Sufficient distance for ranged pulling");
        //    CombatDistance = 25;
        //    if (Local.Casting == 0 && Local.Channeling == 0 && Spell.IsSpellReady("Sinister Strike"))
        //    Spell.Instance.Cast("Shoot Bow");
        //}
        //else if (Target.DistanceToPlayer < 8)
            CombatDistance = 5;
            Spell.Instance.Attack();
    }
    public override void OnRest()
    {
        if (Local.HealthPercent < 50)
        { 
        if (HaveBandage() && !Local.GotDebuff("Recently Bandaged") && Local.Channeling == 0)
            SelectBandage();

        //As mentioned before, resting should probably not be handled by the CC anymore. 
        if (Local.Channeling == 0 && !Local.IsEating)
        {
            if (Local.Race == "Night Elf" && Spell.Instance.IsSpellReady("Shadowmeld"))
            {
                SelectFood();
            }
            return;
               
        }
        }
    }


    public override void ShowGui() { }
    public override void Unload() { }

    public WoWUnit Target { get { return ObjectManager.Instance.Target; } }
    public LocalPlayer Local { get { return ObjectManager.Instance.Player; } }
    public List<WoWUnit> Attackers { get { return UnitInfo.Instance.NpcAttackers; } }
    public Spell Spell { get { return Spell.Instance; } }

    public override string Author { get { return "sensgates"; } }
    public override string Name { get { return "RoguePort, emu / krycess original authors"; } }
    public override int Version { get { return 1; } }
    public override Enums.ClassId Class { get { return Enums.ClassId.Rogue; } }
    public override bool SuppressBotMovement { get { return false; } }
    //public override float CombatDistance { get { return 5.0f; } }
    
}