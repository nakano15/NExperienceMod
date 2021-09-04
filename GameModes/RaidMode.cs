using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NExperience.GameModes
{
    public class RaidMode : GameModeBase
    {
        public const string RaidRpgModeID = "raidrpg";

        public RaidMode()
        {
            this.GameModeID = RaidRpgModeID;
            this.Name = "Raid Mode";
            this.Description = "Not intended to be played alone.\n" +
                "All enemies have status increased for intended gameplay with multiple characters in combat.";
            DefenseToHealthConversionValue = 4;
            MaxLevel = 99;
            this.Status.Add(new StatusInfo() { Name = "Power", Description = "Increases Physical Abilities damage." });
            this.Status.Add(new StatusInfo() { Name = "Magic", Description = "Increases Magical Abilities damage." });
            this.Status.Add(new StatusInfo() { Name = "Life", Description = "Increases Maximum Health." });
            this.Status.Add(new StatusInfo() { Name = "Death", Description = "Increases Critical Power." });
            this.Status.Add(new StatusInfo() { Name = "Efficience", Description = "Increases Critical Rate." });
            this.Status.Add(new StatusInfo() { Name = "Avarice", Description = "Increases Luck." });
        }

        public override void PlayerStatus(int Level, int UncappedLevel, Dictionary<byte, int> PointsUnderEffect, Dictionary<byte, int> PointsInvested, out PlayerStatusMod mod)
        {
            int Pow = PointsUnderEffect[0],
                Mag = PointsUnderEffect[1],
                Life = PointsUnderEffect[2],
                Death = PointsUnderEffect[3],
                Eff = PointsUnderEffect[4],
                Av = PointsUnderEffect[5];
            mod = new PlayerStatusMod();
            mod.MaxHealthSum += 3 * Level * Life;
            mod.MaxHealthMult += 0.01f * Level;
            mod.MaxManaMult += 0.01f * Level;
            float DamageMod = 0.01f * Level + 0.5f;
            mod.MeleeDamageSum += DamageMod + Pow * 0.01f;
            mod.RangedDamageSum += DamageMod + Pow * 0.01f;
            mod.MagicDamageSum += DamageMod + Mag * 0.01f;
            mod.MinionDamageSum += DamageMod + Mag * 0.01f;
            mod.NeutralDamageMult += DamageMod;

            mod.MeleeCritMult += Eff * 0.01f;
            mod.RangedCritMult += Eff * 0.01f;
            mod.MagicCritMult += Eff * 0.01f;
            mod.CriticalDamageSum += Death * 0.01f;

            mod.LuckFactorSum += 0.5f * Av + Level;

            mod.ArmorPenetrationMult += 0.01f * Level;
        }

        public override void NpcStatus(NPC npc, GameModeData Data)
        {
            if(npc.lifeMax > 5)
                npc.lifeMax = (int)((npc.lifeMax + 50 + 10 * (Data.Level * 0.1f) + (npc.lifeMax * 0.01f) * 100) * Data.Level2);
            if(npc.damage > 0) npc.damage += 20 + 5 * (int)(Data.Level * 0.1f) + Data.Level2 * 5;
            npc.defense += 5 + (int)(Data.Level2 * 0.5f);
            npc.knockBackResist *= 0.3f;
            //npc.knockBackResist -= npc.knockBackResist * Data.Level2 * 0.005f;
            Data.Exp = (int)(npc.lifeMax * Data.Level * 0.1f);
        }

        public override int MobSpawnLevel(NPC npc)
        {
            switch (npc.netID)
            {
                case 1:
                    return 3;
                case -3:
                    return 1;
                case -4:
                    return 10;
                case -10:
                    return 35;
            }
            switch (npc.type)
            {
                case 4:
                    return 30;
                case 13:
                case 14:
                case 15:
                    return 40;
                case 68:
                    return 99;
                case 266:
                    return 40;
                case 267:
                    return 12;
                case 35:
                    return 60;
                case 36:
                    return 38;
                case NPCID.MoonLordCore:
                case NPCID.MoonLordFreeEye:
                case NPCID.MoonLordHand:
                case NPCID.MoonLordHead:
                case NPCID.MoonLordLeechBlob:
                    return 99;
            }
            if (npc.defDefense > 999)
            {
                return 99;
            }
            int ResultLevel = npc.defDefense * 2;
            if (ResultLevel > MaxLevel)
                ResultLevel = MaxLevel;
            if (ResultLevel < 1)
                ResultLevel = 1;
            return ResultLevel;
        }

        public override void BiomeLevelRules(Player player, GameModeData Data, out int MinLevel, out int MaxLevel)
        {
            MinLevel = 1;
            MaxLevel = 99;
            if(player.ZoneTowerNebula || player.ZoneTowerSolar || player.ZoneTowerStardust || player.ZoneTowerVortex) {
                MinLevel = 90;
                MaxLevel = 99;
            }
            else if (player.ZoneUnderworldHeight)
            {
                MinLevel = 50;
                MaxLevel = 60;
            }
            else if(player.ZoneDungeon)
            {
                MinLevel = 40;
                MaxLevel = 50;
            }
            else if (player.ZoneJungle)
            {
                if (Main.hardMode)
                {
                    MinLevel = 70;
                    MaxLevel = 80;
                }
                else
                {
                    MinLevel = 30;
                    MaxLevel = 40;
                }
            }
            else if(player.ZoneCorrupt || player.ZoneCrimson)
            {
                MinLevel = 20;
                MaxLevel = 30;
                if (Main.hardMode)
                {
                    MinLevel += 40;
                    MaxLevel += 40;
                }
            }
            else if (player.ZoneDesert)
            {
                MinLevel = 5;
                MaxLevel = 15;
                if (Main.hardMode)
                {
                    MinLevel += 40;
                    MaxLevel += 40;
                }
            }
            else
            {
                MinLevel = 1;
                MaxLevel = 8;
                bool HardmodeCalc = false;
                if (player.ZoneRockLayerHeight)
                {
                    MinLevel += 12;
                    MaxLevel += 12;
                    HardmodeCalc = true;
                }
                else if (player.ZoneDirtLayerHeight)
                {
                    MinLevel += 5;
                    MaxLevel += 5;
                    HardmodeCalc = true;
                }
                else if (Main.eclipse)
                {
                    MinLevel = 70;
                    MaxLevel = 80;
                }
                else if (!Main.dayTime)
                {
                    MinLevel = 9;
                    MaxLevel = 18;
                    HardmodeCalc = true;
                }
                else
                {
                    HardmodeCalc = true;
                }
                if(HardmodeCalc && Main.hardMode)
                {
                    MinLevel += 40;
                    MaxLevel += 40;
                }
            }
        }

        public override int ExpFormula(int level, GameModeData gmd)
        {
            int a = 382, b = 279, c = 100;
            int Level = level - 1;
            int FinalLevel = a * Level * Level + b * Level + c;
            FinalLevel += (int)(level * 0.03f * FinalLevel);
            return FinalLevel;
        }
    }
}
