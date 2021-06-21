using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using Microsoft.Xna.Framework;

namespace NExperience.GameModes
{
    public class FreeMode : GameModeBase
    {
        public const string FreeRpgModeID = "freerpg";
        public static bool BossesHaveNerfedLevels = true;

        public FreeMode()
        {
            GameModeID = "freerpg";
            Name = "Free Mode";
            WikiPageID = "Free_Mode";
            Description = "Levels? Rules? What are those?";
            InitialLevel = 0;
            MaxLevel = int.MaxValue;
            InitialStatusPoints = 0;
            StatusPointsPerLevel = 0.01;
            AllowLevelCapping = false;
            StatusRules();
        }

        public override int MobSpawnLevel(NPC npc)
        {
            int Level = 1;
            const int LowestStatus = 14 + (6 * 8) + 0;
            int ThisMobStatus = npc.lifeMax + (npc.damage * 8) + (npc.defense * 8);
            bool BossMob = Terraria.ID.NPCID.Sets.TechnicallyABoss[npc.type];
            Level += (ThisMobStatus - LowestStatus) / 8;
            Level += GetLevelBonus(npc.Center);
            if (BossMob && BossesHaveNerfedLevels)
                Level /= 8;
            else
                Level += ProgressionIncrement();
            if (npc.type >= 134 && npc.type <= 136)
                Level /= 16;
            return Level;
        }

        public override void PlayerStatus(int Level, int UncappedLevel, Dictionary<byte, int> PointsUnderEffect, Dictionary<byte, int> PointsInvested, out PlayerStatusMod mod)
        {
            int fgt = PointsUnderEffect[0],
                ran = PointsUnderEffect[1],
                mag = PointsUnderEffect[2],
                thi = PointsUnderEffect[3],
                aco = PointsUnderEffect[4],
                sum = PointsUnderEffect[5];
            mod = new PlayerStatusMod();
            int Level2 = UncappedLevel;
            float StatusBonusPerLevel = Level2 * 0.0001f;
            mod.MaxHealthMult += StatusBonusPerLevel * 100;
            mod.MaxManaMult += StatusBonusPerLevel * 100;
            mod.MeleeDamageMult += StatusBonusPerLevel;
            mod.RangedDamageMult += StatusBonusPerLevel;
            mod.MagicDamageMult += StatusBonusPerLevel;
            mod.MinionDamageMult += StatusBonusPerLevel;
            mod.NeutralDamageMult += StatusBonusPerLevel;
            mod.DefenseMult += StatusBonusPerLevel;
            mod.NeutralDamageMult += StatusBonusPerLevel;
            mod.LuckFactorSum += Level * 0.01f;

            mod.MaxHealthSum += 4 * fgt;
            mod.MeleeDamageSum += 3 * fgt;
            mod.DefenseSum += 2 * fgt;
            mod.RangedDamageSum += 1 * fgt;

            mod.RangedDamageSum += 4 * ran;
            mod.MinionDamageSum += 3 * ran;
            mod.MeleeDamageSum += 2 * ran;
            mod.DefenseSum += 1 * ran;

            mod.MagicDamageSum += 4 * mag;
            mod.MinionDamageSum += 3 * mag;
            mod.RangedDamageSum += 2 * mag;
            mod.MeleeDamageSum += 1 * mag;

            mod.MeleeDamageSum += 4 * thi;
            mod.RangedDamageSum += 3 * thi;
            mod.MaxHealthSum += 2 * thi;
            mod.DefenseSum += 1 * thi;

            mod.MaxHealthSum += 4 * aco;
            mod.MagicDamageSum += 3 * aco;
            mod.MeleeDamageSum += 2 * aco;
            mod.MinionDamageSum += 1 * aco;

            mod.MinionDamageSum += 4 * sum;
            mod.RangedDamageSum += 3 * sum;
            mod.MagicDamageSum += 2 * sum;
            mod.MaxHealthSum += 1 * sum;
        }

        public override void NpcStatus(NPC npc, GameModeData Data)
        {
            float StatusIncrease = Data.Level * 0.01f;
            if (npc.lifeMax > 5)
                npc.lifeMax += (int)(npc.lifeMax * StatusIncrease);
            npc.damage += (int)(npc.damage * StatusIncrease);
            Data.ProjDamageMult += StatusIncrease;
            Data.Exp = npc.lifeMax * 2 + (npc.damage * 4) + (npc.defense * 8) + Data.Level * 16;
        }

        public override string LevelText(GameModeData gmd)
        {
            return "Rank: " + RomanAlgarismMaker((int)(gmd.Level * 0.1) + 1);
        }

        public override int ExpFormula(int Level, GameModeData gmd)
        {
            int ExpGrade = Level / 100;
            int Exp = 50 * (ExpGrade + 1);
            float Bonus = 0.1f * (ExpGrade + 1);
            return (int)(Exp + (Exp * (Bonus * (Level - (ExpGrade * 100)))));
        }

        public void StatusRules()
        {
            GameModeBase.StatusInfo status = new StatusInfo();
            status.Name = "Fighter";
            status.Description = "MHP [++++], Melee-Damage [+++], DEF [++], Ranged-Damage [+]";
            status.InitialPoints = 0;
            status.MaxPoints = int.MaxValue;
            Status.Add(status);
            status = new StatusInfo();
            status.Name = "Ranger";
            status.Description = "Ranged-Damage [++++], Summon-Damage [+++], Melee-Damage [++], DEF [+]";
            status.InitialPoints = 0;
            status.MaxPoints = int.MaxValue;
            Status.Add(status);
            status = new StatusInfo();
            status.Name = "Mage";
            status.Description = "Magic-Damage [++++], Minion-Damage [+++], Ranged-Damage [++], Melee-Damage [+]";
            status.InitialPoints = 0;
            status.MaxPoints = int.MaxValue;
            Status.Add(status);
            status = new StatusInfo();
            status.Name = "Thief";
            status.Description = "Melee-Damage [++++], Ranged-Damage [+++], MHP [++], Defense [+]";
            status.InitialPoints = 0;
            status.MaxPoints = int.MaxValue;
            Status.Add(status);
            status = new StatusInfo();
            status.Name = "Acolyte";
            status.Description = "MHP [++++], Magic-Damage [+++], Melee-Damage [++], Summon-Damage [+]";
            status.InitialPoints = 0;
            status.MaxPoints = int.MaxValue;
            Status.Add(status);
            status = new StatusInfo();
            status.Name = "Summoner";
            status.Description = "Summon-Damage [++++], Ranged-Damage [+++], Magic-Damage [++], MHP [+]";
            status.InitialPoints = 0;
            status.MaxPoints = int.MaxValue;
            Status.Add(status);
        }

        public static string RomanAlgarismMaker(int Number)
        {
            string Text = "";
            byte MCounter = 0;
            while (Number > 0)
            {
                if (Number >= 1000)
                {
                    MCounter++;
                    //Text += "M";
                    Number -= 1000;
                }
                else if (Number >= 900)
                {
                    Text += "CM";
                    Number -= 900;
                }
                else if (Number >= 500)
                {
                    Text += "D";
                    Number -= 500;
                }
                else if (Number >= 400)
                {
                    Text += "CD";
                    Number -= 400;
                }
                else if (Number >= 100)
                {
                    Text += "C";
                    Number -= 100;
                }
                else if (Number >= 90)
                {
                    Text += "XC";
                    Number -= 90;
                }
                else if (Number >= 50)
                {
                    Text += "L";
                    Number -= 50;
                }
                else if (Number >= 40)
                {
                    Text += "XL";
                    Number -= 40;
                }
                else if (Number >= 10)
                {
                    Text += "X";
                    Number -= 10;
                }
                else if (Number >= 9)
                {
                    Text += "IX";
                    Number -= 9;
                }
                else if (Number >= 5)
                {
                    Text += "V";
                    Number -= 5;
                }
                else if (Number >= 4)
                {
                    Text += "IV";
                    Number -= 1000;
                }
                else
                {
                    Text += "I";
                    Number--;
                }
            }
            if (MCounter > 3)
            {
                Text = (MCounter - 3) + "M+MMM" + Text;
                MCounter = 0;
            }
            while (MCounter > 0)
            {
                Text = "M" + Text;
                MCounter--;
            }
            return Text;
        }

        public static int GetLevelBonus(Vector2 Position)
        {
            Vector2 NewPos = Position * 0.0625f;
            int BonusLevel = 0;
            int HorizontalLevel = (int)Math.Abs(NewPos.X - Main.spawnTileX) / 8,
                VerticalLevel = (int)Math.Abs(NewPos.Y - Main.spawnTileY) / 8;
            BonusLevel += HorizontalLevel + VerticalLevel;
            BonusLevel += ProgressionIncrement();
            return BonusLevel;
        }

        public static int ProgressionIncrement()
        {
            int LevelBonus = 0;
            if (NPC.downedBoss1)
                LevelBonus += 10;
            if (NPC.downedBoss2)
                LevelBonus += 20;
            if (NPC.downedBoss3)
                LevelBonus += 30;
            if (NPC.downedQueenBee)
                LevelBonus += 20;
            if (NPC.downedGoblins)
                LevelBonus += 25;
            if (NPC.downedSlimeKing)
                LevelBonus += 30;
            if (Main.hardMode)
                LevelBonus += 50;
            if (NPC.downedMechBoss1)
                LevelBonus += 30;
            if (NPC.downedMechBoss2)
                LevelBonus += 30;
            if (NPC.downedMechBoss3)
                LevelBonus += 30;
            if (NPC.downedPirates)
                LevelBonus += 20;
            if (NPC.downedPlantBoss)
                LevelBonus += 30;
            if (NPC.downedGolemBoss)
                LevelBonus += 30;
            if (NPC.downedAncientCultist)
                LevelBonus += 30;
            if (NPC.downedMartians)
                LevelBonus += 25;
            if (NPC.downedFrost)
                LevelBonus += 20;
            if (NPC.downedTowerSolar)
                LevelBonus += 25;
            if (NPC.downedTowerStardust)
                LevelBonus += 25;
            if (NPC.downedTowerVortex)
                LevelBonus += 25;
            if (NPC.downedTowerNebula)
                LevelBonus += 25;
            if (NPC.downedMoonlord)
                LevelBonus += 100;
            return LevelBonus * 5;
        }
    }
}
