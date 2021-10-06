using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using Terraria.ID;

namespace NExperience.GameModes
{
    public class RegularRPG : GameModeBase
    {
        public const string RegularRpgModeID = "regularrpg",
            HardcoreRpgModeID = "hardcorerpg",
            SoftcoreRpgModeID = "softrpg";
        public static int[] RegularRPGMaxExpTable = new int[0];
        public static bool PosLevel100Scale = true, PosLevel100ExpNerf = true, BloodmoonStatusBoost = true;
        public byte ModeVariant = 0;
        public bool IsHardcoreRPGMode { get { return ModeVariant == 1; } }
        public bool IsSoftRPGMode { get { return ModeVariant == 2; } }

        public RegularRPG(byte ModeVariant)
        {
            this.ModeVariant = ModeVariant;
            switch (ModeVariant)
            {
                case 0:
                    GameModeID = RegularRpgModeID;
                    Name = "Regular RPG";
                    WikiPageID = "Regular_RPG_Mode";
                    Description = "N Terraria's basic rpg mode.\n" +
                        "Has decent leveling rate, and monsters can worry you a bit.\n" +
                        "Good if you want a nice leveling experience.";
                    break;
                case 1:
                    GameModeID = HardcoreRpgModeID;
                    Name = "Hardcore RPG";
                    WikiPageID = "Hardcore_RPG_Mode";
                    Description = "If you want an extra challenge version of Regular RPG Mode,\nand with a better reward.\n" +
                        "Monsters are extra tougher, and due to that, award more exp.\n" +
                        "You will need to pull yourself over the limit starting from near the end game.";
                    break;
                case 2:
                    GameModeID = SoftcoreRpgModeID;
                    Name = "Softcore RPG";
                    WikiPageID = "Softcore_RPG_Mode";
                    Description = "An way easier version of Regular RPG Mode.\n" +
                        "Difficulty is greatly reduced.\n" +
                        "May be way too easy late in the game.";
                    break;
            }
            MaxLevel = 150;
            InitialStatusPoints = 1;
            StatusPointsPerLevel = 1;
            DefenseToHealthConversionValue = 5;
            StatusRules();
            FormulaCreator();
        }

        public override int GetExpReward(float RewardLevel, float Difficulty, ExpReceivedPopText.ExpSource source, GameModeData gmd)
        {
            float Level = RewardLevel / 100 * MaxLevel;
            if (Level == 0)
                Level = 1;
            else if(RewardLevel > 10)
            {
                float LevelDividedBy10 = Level;
                Level -= LevelDividedBy10 / (LevelDividedBy10 + 10);
            }
            if (Level > MaxLevel)
                Level = MaxLevel;
            if (Level < 0)
                Level = 0;
            //Main.NewText("Generated reward for level " + (int)Level + " and It gave " + (Difficulty * 100) + "% exp reward.");
            int Exp = (int)(RegularRPGMaxExpTable[(int)(Level)] * Difficulty);
            //if (source == ExpReceivedPopText.ExpSource.Extractinator)
            //    Exp = (int)(Exp * 0.1f);
            return Exp;
        }

        public void FormulaCreator()
        {
            if (RegularRPGMaxExpTable.Length == 0)
            {
                List<int> NewExpTable = new List<int>();
                int CurrentExp = 400; // 125
                float Growth = 0.25f; //0.2f -> 0.25f
                float GrowthLevels = 3; //5
                while (NewExpTable.Count <= MaxLevel)
                {
                    NewExpTable.Add(Math.Abs(CurrentExp));
                    int Exp = CurrentExp;
                    Exp = (int)(Exp * (Growth - (((float)NewExpTable.Count / GrowthLevels) * 0.01f)));
                    GrowthLevels += 0.02f;
                    CurrentExp += Exp;
                }
                RegularRPGMaxExpTable = NewExpTable.ToArray();
            }
        }

        public override int ExpFormula(int level, GameModeData gmd)
        {
            if (!PosLevel100ExpNerf || (gmd.Base as RegularRPG).ModeVariant == 1 || (level < 100 && (gmd.Base as RegularRPG).ModeVariant != 1))
            {
                return RegularRPGMaxExpTable[level];
            }
            else
            {
                return RegularRPGMaxExpTable[99] + (int)(Math.Pow(21, Math.Log(level, 4)));
            }
        }

        public override int LevelDamageScale(int Level)
        {
            int MaxDamage = 0;
            if (Level < 120)
            {
                if (Level >= 100)
                {
                    MaxDamage = 80;
                }
                else if (Level >= 80)
                {
                    MaxDamage = 60;
                }
                else if (Level >= 50)
                {
                    MaxDamage = 40;
                }
                else if (Level >= 30)
                {
                    MaxDamage = 30;
                }
                else if (Level >= 20)
                {
                    MaxDamage = 20;
                }
                else if (Level < 20)
                {
                    MaxDamage = 15;
                }
            }
            return MaxDamage;
        }

        public override int LevelDefenseScale(int Level)
        {
            int MaxDefense = 0;
            if (Level < 120)
            {
                if (Level >= 100)
                {
                    MaxDefense = 75;
                }
                else if (Level >= 80)
                {
                    MaxDefense = 60;
                }
                else if (Level >= 50)
                {
                    MaxDefense = 50;
                }
                else if (Level >= 30)
                {
                    MaxDefense = 25;
                }
                else if (Level >= 20)
                {
                    MaxDefense = 20;
                }
                else if (Level < 20)
                {
                    MaxDefense = 10;
                }
            }
            return MaxDefense;
        }

        public override void BiomeLevelRules(Player player, GameModeData Data, out int MinLevel, out int MaxLevel)
        {
            MinLevel = 1;
            MaxLevel = 2;
            bool IsHardmode = Main.hardMode;
            bool WorldEdge = Math.Abs(player.Center.X / 16 - Main.spawnTileX) >= Main.maxTilesX / 3;
            bool IsUnderground = player.ZoneDirtLayerHeight || player.ZoneRockLayerHeight;
            bool OnLihzahrdDungeon = MainMod.InsideLihzahrdDungeon(player);
            if (player.ZoneTowerNebula || player.ZoneTowerSolar || player.ZoneTowerStardust || player.ZoneTowerVortex)
            {
                MinLevel = 125;
                MaxLevel = 135;
                byte Score = 0;
                if (NPC.downedTowerNebula)
                    Score++;
                if (NPC.downedTowerSolar)
                    Score++;
                if (NPC.downedTowerStardust)
                    Score++;
                if (NPC.downedTowerVortex)
                    Score++;
                MinLevel += Score * 5;
                MaxLevel += Score * 5;
            }
            else if (OnLihzahrdDungeon)
            {
                MinLevel = 100;
                MaxLevel = 115;
            }
            else if (player.ZoneUnderworldHeight)
            {
                if (IsHardmode && NPC.downedMechBossAny)
                {
                    MinLevel = 80;
                    MaxLevel = 90;
                }
                else
                {
                    MinLevel = 50;
                    MaxLevel = 60;
                }
            }
            else if (player.ZoneDungeon)
            {
                if (IsHardmode && NPC.downedPlantBoss)
                {
                    MinLevel = 115;
                    MaxLevel = 130;
                }
                else
                {
                    MinLevel = 40;
                    MaxLevel = 50;
                }
            }
            else if (player.ZoneJungle)
            {
                if (player.ZoneDirtLayerHeight || player.ZoneRockLayerHeight)
                {
                    if (!IsHardmode)
                    {
                        MinLevel = 34;
                        MaxLevel = 40;
                    }
                    else
                    {
                        MinLevel = 88;
                        MaxLevel = 100;
                    }
                }
                else
                {
                    if (!IsHardmode)
                    {
                        MinLevel = 30;
                        MaxLevel = 37;
                    }
                    else
                    {
                        MinLevel = 80;
                        MaxLevel = 87;
                    }
                }
            }
            else if (player.ZoneCorrupt || player.ZoneCrimson || player.ZoneHoly)
            {
                if (player.ZoneRockLayerHeight)
                {
                    if (!IsHardmode)
                    {
                        MinLevel = 25;
                        MaxLevel = 31;
                    }
                    else
                    {
                        MinLevel = 63;
                        MaxLevel = 75;
                    }
                }
                else
                {
                    if (!IsHardmode)
                    {
                        MinLevel = 23;
                        MaxLevel = 27;
                    }
                    else
                    {
                        MinLevel = 54;
                        MaxLevel = 62;
                    }
                }
            }
            else if (player.GetModPlayer<PlayerMod>().ZoneGraveyard)
            {
                if (Main.hardMode)
                {
                    MinLevel = 70;
                    MaxLevel = 80;
                }
                else
                {
                    MinLevel = 20;
                    MaxLevel = 30;
                }
            }
            else if (player.ZoneBeach)
            {
                if (!IsHardmode)
                {
                    MinLevel = 27;
                    MaxLevel = 32;
                }
                else
                {
                    MinLevel = 65;
                    MaxLevel = 72;
                }
            }
            else if (player.ZoneDesert)
            {
                if (!IsHardmode)
                {
                    MinLevel = 16;
                    MaxLevel = 22;
                }
                else
                {
                    MinLevel = 63;
                    MaxLevel = 70;
                }
            }
            else if (player.ZoneSnow)
            {
                if (player.ZoneRockLayerHeight)
                {
                    if (!IsHardmode)
                    {
                        MinLevel = 16;
                        MaxLevel = 25;
                    }
                    else
                    {
                        MinLevel = 66;
                        MaxLevel = 76;
                    }
                }
                else if (player.ZoneDirtLayerHeight)
                {
                    if (!IsHardmode)
                    {
                        MinLevel = 7;
                        MaxLevel = 15;
                    }
                    else
                    {
                        MinLevel = 59;
                        MaxLevel = 65;
                    }
                }
                else
                {
                    if (!IsHardmode)
                    {
                        if (Main.dayTime)
                        {
                            MinLevel = 1;
                            MaxLevel = 7;
                        }
                        else
                        {
                            MinLevel = 8;
                            MaxLevel = 15;
                        }
                    }
                    else
                    {
                        if (Main.dayTime)
                        {
                            MinLevel = 54;
                            MaxLevel = 61;
                        }
                        else
                        {
                            MinLevel = 61;
                            MaxLevel = 69;
                        }
                    }
                }
            }
            else //Forest biome
            {
                if (player.ZoneRockLayerHeight)
                {
                    if (!IsHardmode)
                    {
                        MinLevel = 15;
                        MaxLevel = 23;
                    }
                    else
                    {
                        MinLevel = 60;
                        MaxLevel = 68;
                    }
                }
                else if (player.ZoneDirtLayerHeight)
                {
                    if (!IsHardmode)
                    {
                        MinLevel = 9;
                        MaxLevel = 15;
                    }
                    else
                    {
                        MinLevel = 60;
                        MaxLevel = 68;
                    }
                }
                else
                {
                    if (!IsHardmode)
                    {
                        if (Main.dayTime)
                        {
                            MinLevel = 1;
                            MaxLevel = 7;
                        }
                        else
                        {
                            MinLevel = 7;
                            MaxLevel = 14;
                        }
                    }
                    else
                    {
                        if (Main.dayTime)
                        {
                            MinLevel = 50;
                            MaxLevel = 58;
                        }
                        else
                        {
                            MinLevel = 56;
                            MaxLevel = 64;
                        }
                    }
                }
            }
        }

        public override int MobSpawnLevel(NPC npc)
        {
            int Level = -1;
            if (npc.type == 23) //Meteor Head
            {
                if (Main.hardMode)
                    Level = 65;
                else
                    Level = 35;
            }
            else if (npc.type == 73) //Goblin Scout
            {
                if (Main.hardMode)
                    Level = 67;
                else
                    Level = 27;
            }
            else if (npc.type == 26) //Goblin Peon
            {
                if (Main.hardMode)
                    Level = 66;
                else
                    Level = 26;
            }
            else if (npc.type == 29) //Goblin Sorcerer
            {
                if (Main.hardMode)
                    Level = 64;
                else
                    Level = 24;
            }
            else if (npc.type == 27) //Goblin Thief
            {
                if (Main.hardMode)
                    Level = 62;
                else
                    Level = 22;
            }
            else if (npc.type == 28) //Goblin Warrior
            {
                if (Main.hardMode)
                    Level = 68;
                else
                    Level = 28;
            }
            else if (npc.type == 111) //Goblin Archer
            {
                if (Main.hardMode)
                    Level = 67;
                else
                    Level = 27;
            }
            else if (npc.type == 471) //Goblin Summoner
            {
                Level = 70;
            }
            else if (npc.type == 144) //Mister Stabby
            {
                Level = 74;
            }
            else if (npc.type == 143) //Snowman Gangsta
            {
                Level = 69;
            }
            else if (npc.type == 145) //Snow Balla
            {
                Level = 65;
            }
            else if (npc.type == 212) //Pirate Deckhand
            {
                Level = 72;
            }
            else if (npc.type == 213) //Pirate Corsair
            {
                Level = 76;
            }
            else if (npc.type == 214) //Pirate Deadeye
            {
                Level = 75;
            }
            else if (npc.type == 215) //Pirate Crossbower
            {
                Level = 78;
            }
            else if (npc.type == 216) //Pirate Captain
            {
                Level = 83;
            }
            else if (npc.type == 252) //Parrot
            {
                Level = 70;
            }
            else if (npc.type == 491) //Flying Dutchman
            {
                Level = 80;
            }
            else if (npc.type == 381 || npc.type == 382) //Martian Ranger
            {
                Level = 103;
            }
            else if (npc.type == 383) //Martian Officer
            {
                Level = 110;
            }
            else if (npc.type == 385) //Martian Grunty
            {
                Level = 100;
            }
            else if (npc.type == 386) //Martian Engineer
            {
                Level = 105;
            }
            else if (npc.type == 387) //Tesla Turret
            {
                Level = 105;
            }
            else if (npc.type == 388) //Martian Drone
            {
                Level = 107;
            }
            else if (npc.type == 389) //Gigazapper
            {
                Level = 106;
            }
            else if (npc.type == 390) //Scutlix Gunner
            {
                Level = 110;
            }
            else if (npc.type == 391) //Scutlix
            {
                Level = 110;
            }
            else if (npc.type == 520) //Martian Walker
            {
                Level = 112;
            }
            else if (npc.type == 395) //Martian Saucer
            {
                Level = 115;
            }
            else if (npc.type == 166) //Swamp Thing
            {
                Level = 70;
            }
            else if (npc.type == 158 || npc.type == 159) //Vampire
            {
                Level = 75;
            }
            else if (npc.type == 251) //Eyezor
            {
                Level = 80;
            }
            else if (npc.type == 162) //Frankenstein
            {
                Level = 73;
            }
            else if (npc.type == 469) //The Possessed
            {
                Level = 83;
            }
            else if (npc.type == 462) //Fritz
            {
                Level = 77;
            }
            else if (npc.type == 461) //Creature from the Deep
            {
                Level = 85;
            }
            else if (npc.type == 253) //Reaper
            {
                Level = 90;
            }
            else if (npc.type == 477 || npc.type == Terraria.ID.NPCID.MothronEgg || npc.type == Terraria.ID.NPCID.MothronSpawn) //Mothron
            {
                Level = 100;
            }
            else if (npc.type == 460) //Butcher
            {
                Level = 105;
            }
            else if (npc.type == 467) //Deadly Sphere
            {
                Level = 110;
            }
            else if (npc.type == 466) //Psycho
            {
                Level = 107;
            }
            else if (npc.type == 468) //Dr. Man Fly
            {
                Level = 115;
            }
            else if (npc.type == 463) //Nailhead
            {
                Level = 120;
            }
            else if (npc.type == 87) //Wyvern
            {
                Level = 70;
            }
            else if (npc.type == 4 || npc.type == Terraria.ID.NPCID.ServantofCthulhu)
            {
                Level = 15;
            }
            else if (npc.type == 13 || npc.type == 14 || npc.type == 15 || npc.type == 266 || npc.type == Terraria.ID.NPCID.Creeper)
            {
                Level = 25;
            }
            else if (npc.type == 35 || npc.type == Terraria.ID.NPCID.SkeletronHand)
            {
                Level = 40;
            }
            else if (npc.type == 50)
            {
                Level = 30;
            }
            else if (npc.type == 68)
            {
                Level = 150;
            }
            else if (npc.type >= 113 && npc.type <= 116)
            {
                Level = 60;
            }
            else if (npc.type == 125 || npc.type == 126)
            {
                Level = 60; //70
            }
            else if (npc.type >= 127 && npc.type <= 131)
            {
                Level = 70; //90
            }
            else if (npc.type == 134 || npc.type == 135 || npc.type == 136)
            {
                Level = 65; //80
            }
            else if (npc.type == 222)
            {
                Level = 45;
            }
            else if (npc.type == 243)
            {
                Level = 70;
            }
            else if (npc.type >= 262 && npc.type <= 265)
            {
                Level = 90;
            }
            else if (npc.type >= 245 && npc.type <= 249)
            {
                Level = 110;
            }
            else if (npc.type == 370 || npc.type == 371 || npc.type == 372 || npc.type == 373)
            {
                Level = 135;
            }
            else if (npc.type == 439 || npc.type == 440 || (npc.type >= 454 && npc.type <= 459) || npc.type == 521 || npc.type == 522 || npc.type == 523) //Lunatic Cultist
            {
                Level = 130;
            }
            else if (npc.type == 396 || npc.type == 397 || npc.type == 398 || npc.type == 400 || npc.type == 401) //Moon Lord
            {
                Level = 150;
            }
            else if (npc.type == 493 || npc.type == 507 || npc.type == 422 || npc.type == 517) //Towers
            {
                Level = 145;
            }
            switch (npc.type)
            {
                case Terraria.ID.NPCID.DD2DarkMageT1:
                case Terraria.ID.NPCID.DD2DarkMageT3:
                case Terraria.ID.NPCID.DD2DrakinT2:
                case Terraria.ID.NPCID.DD2DrakinT3:
                case Terraria.ID.NPCID.DD2GoblinBomberT1:
                case Terraria.ID.NPCID.DD2GoblinBomberT2:
                case Terraria.ID.NPCID.DD2GoblinBomberT3:
                case Terraria.ID.NPCID.DD2GoblinT1:
                case Terraria.ID.NPCID.DD2GoblinT2:
                case Terraria.ID.NPCID.DD2GoblinT3:
                case Terraria.ID.NPCID.DD2JavelinstT1:
                case Terraria.ID.NPCID.DD2JavelinstT2:
                case Terraria.ID.NPCID.DD2JavelinstT3:
                case Terraria.ID.NPCID.DD2KoboldFlyerT2:
                case Terraria.ID.NPCID.DD2KoboldFlyerT3:
                case Terraria.ID.NPCID.DD2KoboldWalkerT2:
                case Terraria.ID.NPCID.DD2KoboldWalkerT3:
                case Terraria.ID.NPCID.DD2LightningBugT3:
                case Terraria.ID.NPCID.DD2OgreT2:
                case Terraria.ID.NPCID.DD2OgreT3:
                case Terraria.ID.NPCID.DD2SkeletonT1:
                case Terraria.ID.NPCID.DD2SkeletonT3:
                case Terraria.ID.NPCID.DD2WitherBeastT2:
                case Terraria.ID.NPCID.DD2WitherBeastT3:
                case Terraria.ID.NPCID.DD2WyvernT1:
                case Terraria.ID.NPCID.DD2WyvernT2:
                case Terraria.ID.NPCID.DD2WyvernT3:
                    {
                        int NpcLevel = 20 + (NPC.waveNumber - 1) * 6;
                        if (Terraria.GameContent.Events.DD2Event.ReadyForTier3)
                            NpcLevel = 100 + (NPC.waveNumber - 1) * 6;
                        else if (Terraria.GameContent.Events.DD2Event.ReadyForTier2)
                            NpcLevel = 70 + (NPC.waveNumber - 1) * 6;
                        Level = NpcLevel;
                    }
                    break;
                case Terraria.ID.NPCID.DD2EterniaCrystal:
                    {
                        int NpcLevel = 20;
                        if (Terraria.GameContent.Events.DD2Event.ReadyForTier3)
                            NpcLevel = 100;
                        else if (Terraria.GameContent.Events.DD2Event.ReadyForTier2)
                            NpcLevel = 70;
                        Level = NpcLevel;
                    }
                    break;
                case Terraria.ID.NPCID.DD2Betsy:
                    {
                        Level = 140;
                    }
                    break;
                //Pumpkin Moon
                case 305: //Scarecrows
                case 306:
                case 307:
                case 308:
                case 309:
                case 310:
                case 311:
                case 312:
                case 313:
                case 314:
                    Level = 80 + (NPC.waveNumber - 1) * 4;
                    break;
                case 326: //Splinterling
                    Level = 84 + (NPC.waveNumber - 1) * 4;
                    break;
                case 329: //Hellhound
                    Level = 88 + (NPC.waveNumber - 1) * 3;
                    break;
                case 330: //Poltergeist
                    Level = 92 + (NPC.waveNumber - 1) * 3;
                    break;
                case 315: //Headless Horsemen
                    Level = 96 + (NPC.waveNumber - 1) * 2;
                    break;
                case 325: //Mourning Wood
                    Level = 100 + (NPC.waveNumber - 1) * 2;
                    break;
                case 327: //Pumpking
                case 328:
                    Level = 100 + (NPC.waveNumber - 1) * 2;
                    break;
                //Frost Moon
                case 341: //Present Mimic
                    Level = 100 + (NPC.waveNumber - 1) * 2;
                    break;
                case 338: //Zombie Elf
                case 339:
                case 340:
                    Level = 80 + (NPC.waveNumber - 1) * 2;
                    break;
                case 342: //Gingerbread Man
                    Level = 90 + (NPC.waveNumber - 1) * 2;
                    break;
                case 350: //Elf Archer
                    Level = 84 + (NPC.waveNumber - 1) * 2;
                    break;
                case 348: //Nutcracker
                case 349:
                    Level = 88 + (NPC.waveNumber - 1) * 2;
                    break;
                case 344: //Everscream
                    Level = 102 + (NPC.waveNumber - 1);
                    break;
                case 347: //Elf Copter
                    Level = 106 + (NPC.waveNumber - 1);
                    break;
                case 346: //Santa-NK1
                    Level = 110 + (NPC.waveNumber - 1);
                    break;
                case 351: //Krampus
                    Level = 114 + (NPC.waveNumber - 1);
                    break;
                case 352: //Flocko
                    Level = 118 + (NPC.waveNumber - 1);
                    break;
                case 343: //Yeti
                    Level = 122 + (NPC.waveNumber - 1);
                    break;
                case 345: //Ice Queen
                    Level = 126 + (NPC.waveNumber - 1);
                    break;
            }
            return Level;
        }

        public override void PlayerStatus(int Level, int UncappedLevel, Dictionary<byte, int> PointsUnderEffect, Dictionary<byte, int> PointsInvested, out PlayerStatusMod mod)
        {
            mod = new PlayerStatusMod();
            int STR = PointsUnderEffect[0],
                AGI = PointsUnderEffect[1],
                VIT = PointsUnderEffect[2],
                INT = PointsUnderEffect[3],
                DEX = PointsUnderEffect[4],
                LUK = PointsUnderEffect[5],
                CHA = PointsUnderEffect[6],
                WIS = PointsUnderEffect[7];
            int CHAOriginal = PointsInvested[6];
            if (CHAOriginal > MaxLevel)
                CHAOriginal = MaxLevel;
            const int HealthReductionLevels = 50;
            const float Bonus = 1.5f;
            if (Level < HealthReductionLevels)
            {
                float Penalty = 1f - ((float)Level * (1f / HealthReductionLevels));
                mod.MaxHealthMult -= Penalty * 0.5f;
            }
            float SecondaryBonus = 0.5f, TotalBonus = 0.06f;
            if (IsSoftRPGMode)
            {
                TotalBonus *= 0.5f;
            }
            mod.MaxHealthMult += (VIT + CHA * SecondaryBonus + Level * Bonus) * TotalBonus;
            mod.MaxManaMult += (WIS + INT * SecondaryBonus) * TotalBonus; // + Level * Bonus
            mod.MeleeDamageMult += (STR + DEX * SecondaryBonus + Level * Bonus) * TotalBonus;
            mod.RangedDamageMult += (DEX + AGI * SecondaryBonus + Level * Bonus) * TotalBonus;
            mod.MagicDamageMult += (INT + WIS * SecondaryBonus + Level * Bonus) * TotalBonus;
            mod.MinionDamageMult += (CHA + INT * SecondaryBonus + Level * Bonus) * TotalBonus;
            mod.NeutralDamageMult += Level * Bonus * TotalBonus;
            mod.DefenseMult += ((VIT + STR * SecondaryBonus) * 0.5f + Level * Bonus) * TotalBonus;
            mod.MeleeSpeedMult += (AGI + Level * 0.5f) * 0.0055f;
            mod.MoveSpeedMult += (AGI + Level * 0.5f) * 0.0055f;
            mod.MeleeCritMult += (LUK + STR * 0.33f) * 0.0133f;
            mod.RangedCritMult += (LUK + DEX * 0.33f) * 0.0133f;
            mod.MagicCritMult += (LUK + INT * 0.33f) * 0.0133f;
            mod.ManaCostMult += (INT * 0.5f - WIS * 0.25f) * 0.05f; // +(Level * Bonus) * TotalBonus;
            mod.SummonCountMult += CHAOriginal * 0.005f; //0.01f
            mod.LuckFactorSum += LUK * 0.5f + Level;
            mod.CriticalDamageSum += 0.01f * (STR + DEX + INT + WIS) * 0.33f;
            mod.ArmorPenetrationMult += (DEX + LUK * 0.33f) * TotalBonus;
            const int BoostLevelStart = 100; //150;
            if (PosLevel100Scale && Level > BoostLevelStart)
            {
                float Boost = (Level - BoostLevelStart) * 0.025f;
                mod.MaxHealthMult += Boost;
                mod.MeleeDamageMult += Boost;
                mod.RangedDamageMult += Boost;
                mod.MagicDamageMult += Boost;
                mod.MinionDamageMult += Boost;
                mod.DefenseMult += Boost;
            }
        }

        public override void NpcStatus(NPC npc, GameModeData data)
        {
            int LifeMax = npc.lifeMax, Damage = npc.damage, Defense = npc.defense;

            npc.lifeMax = LifeMax;
            npc.damage = Damage;
            npc.defense = Defense;
            data.ProjDamageMult = 1f;
            float Level = data.Level2;
            if (IsSoftRPGMode)
            {
                Level++;
                Level *= 0.5f;
            }
            data.ProjDamageMult += Level * 0.1f;
            npc.damage += (int)(npc.damage * Level * 0.1f);
            npc.defense += (int)(npc.defense * Level * 0.1f);
            if (npc.lifeMax > 5)
            {
                npc.lifeMax += (int)(npc.lifeMax * Level * 0.1f);
            }
            if (IsHardcoreRPGMode)
            {
                if (npc.lifeMax > 5) npc.lifeMax += (int)(npc.lifeMax * 0.55f);
                npc.damage += (int)(npc.damage * 0.45f);
                npc.defense += (int)(npc.defense * 0.45f);
                data.ProjDamageMult += data.ProjDamageMult * 0.45f;
            }
            if (Level > 75 && Level <= 100)
            {
                if (npc.lifeMax > 5) npc.lifeMax += (int)(npc.lifeMax * ((Level - 75) * 0.03f));
                npc.damage += (int)(npc.damage * ((Level - 75) * 0.03f));
                npc.defense += (int)(npc.defense * ((Level - 75) * 0.03f));
                data.ProjDamageMult += (int)(data.ProjDamageMult * ((Level - 75) * 0.03f));
            }
            else if (PosLevel100Scale)
            {
                if (Level > 100 && Level <= 150)
                {
                    if (npc.lifeMax > 5) npc.lifeMax += (int)(npc.lifeMax * 0.75f) + (int)(npc.lifeMax * ((Level - 100) * 0.05f));
                    npc.damage += (int)(npc.damage * 0.75f) + (int)(npc.damage * ((Level - 100) * 0.05f));
                    npc.defense += (int)(npc.defense * 0.75f) + (int)(npc.defense * ((Level - 100) * 0.05f));
                    data.ProjDamageMult += (int)(data.ProjDamageMult * 0.75f) + (int)(data.ProjDamageMult * ((Level - 100) * 0.05f));
                }
                else if (Level > 150)
                {
                    if (npc.lifeMax > 5) npc.lifeMax += (int)(npc.lifeMax * 0.75f) + (int)(npc.lifeMax * 0.5f) + (int)(npc.lifeMax * ((Level - 150) * 0.05f));
                    npc.damage += (int)(npc.damage * 0.75f) + (int)(npc.damage * 0.5f) + (int)(npc.damage * ((Level - 150) * 0.05f));
                    npc.defense += (int)(npc.defense * 0.75f) + (int)(npc.defense * 0.5f) + (int)(npc.defense * ((Level - 150) * 0.05f));
                    data.ProjDamageMult += (int)(data.ProjDamageMult * 0.75f) + (int)(data.ProjDamageMult * 0.5f) + (int)(data.ProjDamageMult * ((Level - 150) * 0.05f));
                }
            }
            if (Main.bloodMoon && BloodmoonStatusBoost)
            {
                if (npc.lifeMax > 5) npc.lifeMax += (int)(npc.lifeMax * 1.2f);
                npc.damage += (int)(npc.damage * 1.2f);
                npc.defense += (int)(npc.defense * 1.2f);
                data.ProjDamageMult += (int)(data.ProjDamageMult * 1.2f);
            }
            int Exp = npc.lifeMax + (npc.damage + npc.defense) * 8;
            for (int l = 30; l < Level; l += 30)
            {
                Exp += (int)(Exp * (Level - l) * 0.01f);
            }
            if (IsSoftRPGMode)
                Exp *= 2;
            if (Level < 60)
                Exp -= (int)(Exp * ((60 - Level) * 0.011666666666667f));
            data.Exp = Exp;
            if (WorldMod.IsDeathMode)
                npc.defense = Defense;
        }

        public void StatusRules()
        {
            StatusInfo status = new StatusInfo();
            status.Name = "Strength";
            status.Description = "(+) Melee Damage. (+/2)Melee Critical Rate, (+/2) Critical Damage, (+/2) Defense";
            status.InitialPoints = 1;
            status.MaxPoints = 150;
            Status.Add(status);
            status = new StatusInfo();
            status.Name = "Agility";
            status.Description = "(+) Melee Speed, (+) Movement Speed. (+/2) Ranged Damage.";
            status.InitialPoints = 1;
            status.MaxPoints = 150;
            Status.Add(status);
            status = new StatusInfo();
            status.Name = "Vitality";
            status.Description = "(+) Defense, (+) Max Health, (+) Health Regeneration Rate.";
            status.InitialPoints = 1;
            status.MaxPoints = 150;
            Status.Add(status);
            status = new StatusInfo();
            status.Name = "Intelligence";
            status.Description = "(+) Magic Damage, (+) Mana Cost, (+) Critical Damage. (+/2) Summon Damage."; //, Max Mana
            status.InitialPoints = 1;
            status.MaxPoints = 150;
            Status.Add(status);
            status = new StatusInfo();
            status.Name = "Dexterity";
            status.Description = "(+) Ranged Damage, (+) Critical Damage. (+/2) Ranged Critical Chance.";
            status.InitialPoints = 1;
            status.MaxPoints = 150;
            Status.Add(status);
            status = new StatusInfo();
            status.Name = "Luck";
            status.Description = "(+) Critical Rates, (+) Luck Strike Chance.";
            status.InitialPoints = 1;
            status.MaxPoints = 150;
            Status.Add(status);
            status = new StatusInfo();
            status.Name = "Charisma";
            status.Description = "(+) Summon Damage. (+/2) Max Health.";
            status.InitialPoints = 1;
            status.MaxPoints = 150;
            Status.Add(status);
            status = new StatusInfo();
            status.Name = "Wisdom";
            status.Description = "(-) Mana Cost. (+) Mana Regeneration, (+) Critical Damage, (+/2) Magic Damage."; //Increases Max Mana, 
            status.InitialPoints = 1;
            status.MaxPoints = 150;
            Status.Add(status);
        }

        public override int GetDigExp(int Tile)
        {
            switch (Tile) {
                default:
                    return 0;
                case TileID.Copper:
                case TileID.Tin:
                    return 30;
                case TileID.Iron:
                case TileID.Lead:
                    return 50;
                case TileID.Silver:
                case TileID.Tungsten:
                    return 80;
                case TileID.Gold:
                case TileID.Platinum:
                    return 100;
                case TileID.Demonite:
                case TileID.Crimtane:
                    return 200;
                case TileID.DesertFossil:
                    return 120;
                case TileID.Meteorite:
                    return 150;
                case 12: //Life Crystal
                    return 3000;
                case TileID.LifeFruit:
                    return 6000;
                case TileID.ShadowOrbs:
                    return 9000;
                case TileID.Obsidian:
                    return 250;
                case TileID.Hellstone:
                    return 300;
                case TileID.Cobalt:
                case TileID.Palladium:
                    return 300;
                case TileID.Mythril:
                case TileID.Orichalcum:
                    return 350;
                case TileID.Adamantite:
                case TileID.Titanium:
                    return 400;
                case TileID.Chlorophyte:
                    return 500;
                case TileID.Crystals:
                    return 600;
                case TileID.Traps:
                    return 1000;
                case TileID.ExposedGems:
                    return 1200;
                case TileID.DyePlants:
                    return 500;
                case TileID.Larva:
                    return 10000;
                case TileID.PlanteraBulb:
                    return 20000;
                case TileID.BeeHive:
                    return 2000;
                case TileID.Cobweb:
                    return 55;
            }
        }
    }
}
