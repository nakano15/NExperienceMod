using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using Terraria.ID;
using Microsoft.Xna.Framework;

namespace NExperience.GameModes
{
    public class BasicRPG : GameModeBase
    {
        public const byte Strength = 0, Agility = 1, Vitality = 2, Intelligence = 3, Dexterity = 4, Luck = 5, Charisma = 6, Wisdom = 7;
        public const string BasicRpgModeID = "basicrpg";

        public BasicRPG()
        {
            this.GameModeID = BasicRpgModeID;
            this.Name = "Basic RPG Mode";
            WikiPageID = "Basic_RPG_Mode";
            this.Description = "Created just for test, in the mod just for fun.";
            DefenseToHealthConversionValue = 2;
            MaxLevel = 125;
            SetRules();
        }

        public override void PlayerStatus(int Level, int UncappedLevel, Dictionary<byte, int> PointsUnderEffect, Dictionary<byte, int> PointsInvested, out PlayerStatusMod mod)
        {
            mod = new PlayerStatusMod();
            int STR = PointsInvested[Strength],
                AGI = PointsInvested[Agility],
                VIT = PointsInvested[Vitality],
                INT = PointsInvested[Intelligence],
                DEX = PointsInvested[Dexterity],
                LUK = PointsInvested[Luck],
                CHA = PointsInvested[Charisma],
                WIS = PointsInvested[Wisdom];
            if (Level < 50)
            {
                mod.MaxHealthMult *= (0.5f + 0.02f * (Level - 1));
                //player.statLifeMax2 = (int)(player.statLifeMax2 * (0.5f + 0.02f * (Level - 1)));
            }
            const float PrimaryBonus = 0.5f, SecondaryBonus = 0.25f, LevelBonus = 1f, TotalBonus = 0.1f;
            mod.MaxHealthMult += (VIT * PrimaryBonus + WIS * SecondaryBonus + Level * LevelBonus) * TotalBonus;
            mod.MaxManaMult += (INT * PrimaryBonus + WIS * SecondaryBonus + Level * LevelBonus) * TotalBonus;
            mod.MeleeDamageMult += (STR * PrimaryBonus + DEX * SecondaryBonus + Level * LevelBonus) * TotalBonus;
            mod.RangedDamageMult += (DEX * PrimaryBonus + STR * SecondaryBonus + Level * LevelBonus) * TotalBonus;
            mod.MagicDamageMult += (INT * PrimaryBonus + WIS * SecondaryBonus + Level * LevelBonus) * TotalBonus;
            mod.MinionDamageMult += (CHA * PrimaryBonus + WIS * SecondaryBonus + Level * LevelBonus) * TotalBonus;
            mod.NeutralDamageMult += Level * LevelBonus * TotalBonus;
            //mod.DefenseMult += (VIT * PrimaryBonus + STR * SecondaryBonus + Level * LevelBonus) * TotalBonus;
            mod.ManaCostMult += (((INT + 100) / (WIS + 100)) * SecondaryBonus + Level * LevelBonus) * TotalBonus;
            if (mod.ManaCostMult < 0)
                mod.ManaCostMult = 0;
            mod.DefenseSum += (VIT + STR * PrimaryBonus + CHA * SecondaryBonus) * TotalBonus;
            mod.MeleeSpeedMult += (AGI * SecondaryBonus) * TotalBonus;
            mod.MeleeCritSum += LUK * PrimaryBonus + STR * SecondaryBonus;
            mod.RangedCritSum += LUK * PrimaryBonus + DEX * SecondaryBonus;
            mod.MagicCritSum += LUK * PrimaryBonus + INT * SecondaryBonus;
            mod.LuckFactorSum += Luck * 0.5f + Level;
            mod.CriticalDamageSum += 0.01f * (STR + DEX + INT + WIS) * 0.33f;
            mod.ArmorPenetrationMult += (DEX * PrimaryBonus + LUK * SecondaryBonus + Level * LevelBonus) * TotalBonus;
        }

        public override void NpcStatus(NPC npc, GameModeData data)
        {
            if (npc.lifeMax > 5) data.Exp = ScaleStatusToLevel(npc.lifeMax, data.Level, true) -
                    (int)(ScaleStatusToLevel(npc.damage, data.Level) - ScaleStatusToLevel(npc.defense, data.Level) * 0.5f);
            int Level = data.Level2;
            float StatusBonus = Level * 0.1f;
            if (npc.lifeMax > 5) npc.lifeMax = ScaleStatusToLevel(npc.lifeMax, Level, true);
            if (npc.damage > 0) npc.damage = ScaleStatusToLevel(npc.damage, Level);
            if(!WorldMod.IsDeathMode)
                npc.defense = ScaleStatusToLevel(npc.defense, Level);
            data.ProjDamageMult += StatusBonus;
            if (npc.damage < 0)
                npc.damage = npc.defDamage;
            if (npc.defense < 0)
                npc.defense = npc.defDefense;
            if (data.Exp < 1)
                data.Exp = 1;
        }

        public void SetRules()
        {
            GameModeBase.StatusInfo status = new StatusInfo();
            status.Name = "Strength";
            status.Description = "Increases Melee Damage and some Ranged Damage, Critical Damage, Defense and Max Health.";
            status.InitialPoints = 1;
            status.MaxPoints = 150;
            Status.Add(status);
            status = new StatusInfo();
            status.Name = "Agility";
            status.Description = "Increases Melee attack speed.";
            status.InitialPoints = 1;
            status.MaxPoints = 150;
            Status.Add(status);
            status = new StatusInfo();
            status.Name = "Vitality";
            status.Description = "Increases Max Health and Defense.";
            status.InitialPoints = 1;
            status.MaxPoints = 150;
            Status.Add(status);
            status = new StatusInfo();
            status.Name = "Intelligence";
            status.Description = "Increases Magic Damage and Critical Damage.";
            status.InitialPoints = 1;
            status.MaxPoints = 150;
            Status.Add(status);
            status = new StatusInfo();
            status.Name = "Dexterity";
            status.Description = "Increases Ranged Damage, Critical Damage and some Melee Damage.";
            status.InitialPoints = 1;
            status.MaxPoints = 150;
            Status.Add(status);
            status = new StatusInfo();
            status.Name = "Luck";
            status.Description = "Increases Critical Rate and the chance of luck happening.";
            status.InitialPoints = 1;
            status.MaxPoints = 150;
            Status.Add(status);
            status = new StatusInfo();
            status.Name = "Charisma";
            status.Description = "Increases Summon damage and some Defense.";
            status.InitialPoints = 1;
            status.MaxPoints = 150;
            Status.Add(status);
            status = new StatusInfo();
            status.Name = "Wisdom";
            status.Description = "Increases some Summon Damage, Critical Damage, Magic damage and Max Health.";
            status.InitialPoints = 1;
            status.MaxPoints = 150;
            Status.Add(status);
        }

        public static int ScaleStatusToLevel(int Status, int Level, bool IsHP = false)
        {
            if (IsHP && Status <= 5) return Status;
            return Status + (int)(Status * Level * 0.1f);
        }

        public override int ExpFormula(int level, GameModeData gmd)
        {
            int x = level - 1, 
			a = 16,  //12
			b = -3, 
			c = 400; //40
            int Exp = a * (x * x) + b * x + c;
            //if (Exp < 0 || Exp > GameModeBase.MaxExpPossible) //To avoid overflow
            //    Exp = GameModeBase.MaxExpPossible;
            return Exp;
        }

        public override void BiomeLevelRules(Player player, GameModeData Data, out int MinLevel, out int MaxLevel)
        {
            MinLevel = 1;
            MaxLevel = 2;
            bool HardmodeChange = Main.hardMode;
            bool WorldEdge = Math.Abs(player.Center.X / 16 - Main.spawnTileX) >= Main.maxTilesX / 3;
            bool IsUnderground = player.ZoneDirtLayerHeight || player.ZoneRockLayerHeight;
            bool OnLihzahrdDungeon = MainMod.InsideLihzahrdDungeon(player);
            if (player.ZoneTowerNebula || player.ZoneTowerSolar || player.ZoneTowerStardust || player.ZoneTowerVortex)
            {
                MinLevel = 100;
                MaxLevel = 120;
                WorldEdge = false;
                HardmodeChange = false;
            }
            else if (OnLihzahrdDungeon)
            {
                MinLevel = 75;
                MaxLevel = 90;
                WorldEdge = false;
                HardmodeChange = false;
            }
            else if (player.ZoneUnderworldHeight)
            {
                WorldEdge = false;
                if (!Main.hardMode || !NPC.downedMechBossAny)
                {
                    HardmodeChange = false;
                }
                MinLevel = 44;
                MaxLevel = 50;
            }
            else if (player.ZoneDungeon)
            {
                WorldEdge = false;
                if (!Main.hardMode || !NPC.downedPlantBoss)
                {
                    HardmodeChange = false;
                }
                MinLevel = 37;
                MaxLevel = 43;
            }
            else if (player.ZoneJungle)
            {
                WorldEdge = false;
                if (!IsUnderground)
                {
                    MinLevel = 20;
                    MaxLevel = 26;
                }
                else
                {
                    MinLevel = 32;
                    MaxLevel = 37;
                }
            }
            else if (player.ZoneCorrupt || player.ZoneCrimson)
            {
                if (!IsUnderground)
                {
                    MinLevel = 12;
                    MaxLevel = 17;
                }
                else
                {
                    MinLevel = 16;
                    MaxLevel = 21;
                }
            }
            else //Forest biome
            {
                if (IsUnderground)
                {
                    MinLevel = 13;
                    MaxLevel = 22;
                }
                else
                {
                    if (player.GetModPlayer<PlayerMod>().ZoneGraveyard)
                    {
                        MinLevel = 20;
                        MaxLevel = 30;
                    }
                    else if (Main.dayTime)
                    {
                        MinLevel = 1;
                        MaxLevel = 7;
                    }
                    else
                    {
                        MinLevel = 9;
                        MaxLevel = 14;
                    }
                }
            }
            if (WorldEdge)
            {
                const int WorldEdgeLevelSum = 20;
                MinLevel += WorldEdgeLevelSum;
                MaxLevel += WorldEdgeLevelSum;
            }
            if (HardmodeChange)
            {
                const int HardmodeLevelBonus = 50;
                MinLevel += HardmodeLevelBonus;
                MaxLevel += HardmodeLevelBonus;
            }
        }

        public override int MobSpawnLevel(NPC npc)
        {
            int Level = -1;
            if (npc.townNPC)
            {
                if (Main.hardMode)
                {
                    if (NPC.downedMoonlord)
                        Level = 100;
                    else
                        Level = 65;
                }
                else
                {
                    Level = 20;
                }
            }
            switch (npc.type)
            {
                case Terraria.ID.NPCID.EyeofCthulhu:
                    Level = 15;
                    break;
                case Terraria.ID.NPCID.EaterofWorldsHead:
                case Terraria.ID.NPCID.EaterofWorldsBody:
                case Terraria.ID.NPCID.EaterofWorldsTail:
                    Level = 22;
                    break;
                case Terraria.ID.NPCID.BrainofCthulhu:
                    Level = 25;
                    break;
                case Terraria.ID.NPCID.Creeper:
                    Level = 18;
                    break;
                case Terraria.ID.NPCID.KingSlime:
                    Level = 50;
                    break;
                case Terraria.ID.NPCID.QueenBee:
                    Level = 55;
                    break;
                case NPCID.Retinazer:
                case NPCID.Spazmatism:
                    Level = 60;
                    break;
                case NPCID.TheDestroyer:
                case NPCID.TheDestroyerBody:
                case NPCID.TheDestroyerTail:
                    Level = 70;
                    break;
                case NPCID.SkeletronPrime:
                case NPCID.PrimeCannon:
                case NPCID.PrimeLaser:
                case NPCID.PrimeSaw:
                case NPCID.PrimeVice:
                    Level = 80;
                    break;
                case Terraria.ID.NPCID.WallofFlesh:
                case Terraria.ID.NPCID.WallofFleshEye:
                    Level = 65;
                    break;
                case Terraria.ID.NPCID.Golem:
                case Terraria.ID.NPCID.GolemFistLeft:
                case Terraria.ID.NPCID.GolemFistRight:
                case Terraria.ID.NPCID.GolemHead:
                case Terraria.ID.NPCID.GolemHeadFree:
                    Level = 100;
                    break;
                case NPCID.CultistBoss:
                case NPCID.CultistBossClone:
                    Level = 110;
                    break;
                case NPCID.DukeFishron:
                case NPCID.Sharkron:
                case NPCID.Sharkron2:
                    Level = 105;
                    break;
                case NPCID.Pinky:
                    Level = 30;
                    break;
                case NPCID.Nymph:
                case NPCID.LostGirl:
                    Level = 45;
                    break;
                case NPCID.Tim:
                    Level = 37;
                    break;
                case NPCID.RuneWizard:
                    Level = 90;
                    break;
                case NPCID.DoctorBones:
                    Level = 55;
                    break;
                case NPCID.WyvernBody:
                case NPCID.WyvernBody2:
                case NPCID.WyvernBody3:
                case NPCID.WyvernHead:
                case NPCID.WyvernLegs:
                case NPCID.WyvernTail:
                    Level = 80;
                    break;
                //Blood Moon
                case NPCID.TheGroom:
                case NPCID.TheBride:
                    Level = 35;
                    break;
                case NPCID.BloodZombie:
                    Level = 26;
                    break;
                case NPCID.Drippler:
                    Level = 28;
                    break;
                case NPCID.CorruptBunny:
                case NPCID.CrimsonBunny:
                case NPCID.CorruptGoldfish:
                case NPCID.CorruptPenguin:
                case NPCID.CrimsonGoldfish:
                case NPCID.CrimsonPenguin:
                    Level = 25;
                    break;
                case NPCID.Clown:
                    Level = 80;
                    break;
                //Goblin Army
                case Terraria.ID.NPCID.GoblinPeon:
                    Level = 22;
                    break;
                case Terraria.ID.NPCID.GoblinThief:
                    Level = 24;
                    break;
                case Terraria.ID.NPCID.GoblinSorcerer:
                    Level = 26;
                    break;
                case Terraria.ID.NPCID.GoblinWarrior:
                    Level = 28;
                    break;
                case Terraria.ID.NPCID.GoblinArcher:
                    Level = 30;
                    break;
                case Terraria.ID.NPCID.GoblinSummoner:
                    Level = 65;
                    break;
                //Old One's Army
                case Terraria.ID.NPCID.DD2EterniaCrystal:
                    Level = 30;
                    if (Main.hardMode && NPC.downedMechBossAny)
                        Level = 70;
                    if (Main.hardMode && NPC.downedGolemBoss)
                        Level = 90;
                    break;
                case Terraria.ID.NPCID.DD2GoblinT1:
                    Level = 20;
                    break;
                case Terraria.ID.NPCID.DD2GoblinT2:
                    Level = 60;
                    break;
                case Terraria.ID.NPCID.DD2GoblinT3:
                    Level = 80;
                    break;
                case Terraria.ID.NPCID.DD2GoblinBomberT1:
                    Level = 23;
                    break;
                case Terraria.ID.NPCID.DD2GoblinBomberT2:
                    Level = 63;
                    break;
                case Terraria.ID.NPCID.DD2GoblinBomberT3:
                    Level = 83;
                    break;
                case NPCID.DD2JavelinstT1:
                    Level = 25;
                    break;
                case NPCID.DD2JavelinstT2:
                    Level = 65;
                    break;
                case NPCID.DD2JavelinstT3:
                    Level = 85;
                    break;
                case NPCID.DD2WyvernT1:
                    Level = 27;
                    break;
                case NPCID.DD2WyvernT2:
                    Level = 67;
                    break;
                case NPCID.DD2WyvernT3:
                    Level = 87;
                    break;
                case NPCID.DD2DarkMageT1:
                    Level = 32;
                    break;
                case NPCID.DD2DarkMageT3:
                    Level = 92;
                    break;
                case NPCID.DD2SkeletonT1:
                    Level = 25;
                    break;
                case NPCID.DD2SkeletonT3:
                    Level = 85;
                    break;
                case NPCID.DD2KoboldWalkerT2:
                    Level = 24;
                    break;
                case NPCID.DD2KoboldWalkerT3:
                    Level = 84;
                    break;
                case NPCID.DD2DrakinT2:
                    Level = 68;
                    break;
                case NPCID.DD2DrakinT3:
                    Level = 88;
                    break;
                case NPCID.DD2KoboldFlyerT2:
                    Level = 66;
                    break;
                case NPCID.DD2KoboldFlyerT3:
                    Level = 86;
                    break;
                case NPCID.DD2WitherBeastT2:
                    Level = 72;
                    break;
                case NPCID.DD2WitherBeastT3:
                    Level = 92;
                    break;
                case NPCID.DD2OgreT2:
                    Level = 76;
                    break;
                case NPCID.DD2OgreT3:
                    Level = 96;
                    break;
                case NPCID.DD2LightningBugT3:
                    Level = 94;
                    break;
                case NPCID.DD2Betsy:
                    Level = 105;
                    break;
                //Pirate Invasion
                case NPCID.PirateCorsair:
                    Level = 56;
                    break;
                case NPCID.PirateCrossbower:
                    Level = 58;
                    break;
                case NPCID.PirateDeadeye:
                    Level = 60;
                    break;
                case NPCID.PirateDeckhand:
                    Level = 54;
                    break;
                case NPCID.Parrot:
                    Level = 50;
                    break;
                case NPCID.PirateCaptain:
                    Level = 75;
                    break;
                case NPCID.PirateShip:
                case NPCID.PirateShipCannon:
                    Level = 80;
                    break;
                //Frost Legion
                case NPCID.MisterStabby:
                    Level = 70;
                    break;
                case NPCID.SnowmanGangsta:
                    Level = 73;
                    break;
                case NPCID.SnowBalla:
                    Level = 75;
                    break;
                //Martian Madness
                case NPCID.RayGunner:
                case NPCID.GrayGrunt:
                case NPCID.BrainScrambler:
                    Level = 82;
                    break;
                case NPCID.MartianEngineer:
                case NPCID.MartianDrone:
                case NPCID.MartianOfficer:
                    Level = 84;
                    break;
                case NPCID.GigaZapper:
                case NPCID.MartianTurret:
                    Level = 85;
                    break;
                case NPCID.MartianWalker:
                    Level = 95;
                    break;
                case NPCID.Scutlix:
                case NPCID.ScutlixRider:
                    Level = 88;
                    break;
                case NPCID.MartianSaucer:
                case NPCID.MartianSaucerCannon:
                case NPCID.MartianSaucerCore:
                case NPCID.MartianSaucerTurret:
                    Level = 100;
                    break;
                //Solar Eclipse
                case NPCID.ThePossessed:
                    Level = 62;
                    break;
                case NPCID.Reaper:
                    Level = 70;
                    break;
                case NPCID.Fritz:
                    Level = 67;
                    break;
                case NPCID.CreatureFromTheDeep:
                    Level = 63;
                    break;
                case NPCID.Vampire:
                case NPCID.VampireBat:
                    Level = 68;
                    break;
                case NPCID.SwampThing:
                    Level = 69;
                    break;
                case NPCID.Frankenstein:
                    Level = 71;
                    break;
                case NPCID.Eyezor:
                    Level = 73;
                    break;
                case NPCID.Mothron:
                case NPCID.MothronSpawn:
                case NPCID.MothronEgg:
                    Level = 80;
                    break;
                case NPCID.DeadlySphere:
                    Level = 82;
                    break;
                case NPCID.Butcher:
                    Level = 76;
                    break;
                case NPCID.DrManFly:
                    Level = 79;
                    break;
                case NPCID.Nailhead:
                    Level = 85;
                    break;
                case NPCID.Psycho:
                    Level = 84;
                    break;
                //Pumpkin Moon
                case NPCID.Scarecrow1:
                case NPCID.Scarecrow2:
                case NPCID.Scarecrow3:
                case NPCID.Scarecrow4:
                case NPCID.Scarecrow5:
                case NPCID.Scarecrow6:
                case NPCID.Scarecrow7:
                case NPCID.Scarecrow8:
                case NPCID.Scarecrow9:
                case NPCID.Scarecrow10:
                    Level = 92;
                    break;
                case NPCID.Splinterling:
                    Level = 95;
                    break;
                case NPCID.Hellhound:
                    Level = 97;
                    break;
                case NPCID.Poltergeist:
                    Level = 100;
                    break;
                case NPCID.HeadlessHorseman:
                    Level = 105;
                    break;
                case NPCID.MourningWood:
                    Level = 110;
                    break;
                case NPCID.Pumpking:
                    Level = 115;
                    break;
                //Frost Moon
                case NPCID.GingerbreadMan:
                    Level = 92;
                    break;
                case NPCID.Flocko:
                    Level = 95;
                    break;
                case NPCID.ZombieElf:
                case NPCID.ZombieElfBeard:
                case NPCID.ZombieElfGirl:
                    Level = 94;
                    break;
                case NPCID.ElfArcher:
                    Level = 98;
                    break;
                case NPCID.Nutcracker:
                case NPCID.NutcrackerSpinning:
                    Level = 100;
                    break;
                case NPCID.PresentMimic:
                    Level = 100;
                    break;
                case NPCID.ElfCopter:
                    Level = 102;
                    break;
                case NPCID.Yeti:
                    Level = 105;
                    break;
                case NPCID.Krampus:
                    Level = 108;
                    break;
                case NPCID.Everscream:
                    Level = 110;
                    break;
                case NPCID.SantaNK1:
                    Level = 115;
                    break;
                case NPCID.IceQueen:
                    Level = 120;
                    break;
                //Lunar Events
                case NPCID.LunarTowerNebula:
                case NPCID.LunarTowerSolar:
                case NPCID.LunarTowerStardust:
                case NPCID.LunarTowerVortex:
                    Level = 120;
                    break;
                case NPCID.MoonLordCore:
                case NPCID.MoonLordFreeEye:
                case NPCID.MoonLordHand:
                case NPCID.MoonLordHead:
                    Level = 125;
                    break;
            }
            return Level;
        }
    }
}
