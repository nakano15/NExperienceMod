using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using Terraria.ID;

namespace NExperience.GameModes
{
    public class AscensionRPG : GameModeBase
    {
        public const string AscensionGameModeID = "ascensionrpg";
        private static int[] MaxExp = new int[0];

        public AscensionRPG()
        {
            GameModeID = AscensionGameModeID;
            Name = "Ascension RPG Mode";
            Description = "Begin your journey on Normal world, and end it on a Expert world.\n" +
                "Quite challenging gameplay experience.\n" +
                "Simple status with their impacts easily understandable.";
            MaxLevel = 200;
            InitialStatusPoints = 1;
            StatusPointsPerLevel = 1.2;
            CreateMaxExpTable();
            StatusList();
        }

        public void StatusList()
        {
            StatusInfo status = new StatusInfo();
            status.Name = "Offensive";
            status.Description = "Increases offensive abilities.";
            Status.Add(status);
            //
            status = new StatusInfo();
            status.Name = "Defensive";
            status.Description = "Increases defense power.";
            Status.Add(status);
            //
            status = new StatusInfo();
            status.Name = "Health";
            status.Description = "Increases your maximum health.";
            Status.Add(status);
            //
            status = new StatusInfo();
            status.Name = "Mana";
            status.Description = "Increases your maximum mana.";
            Status.Add(status);
            //
            status = new StatusInfo();
            status.Name = "Potence";
            status.Description = "Increases the impact of your attacks.\n" +
                "Also increases summon count, depending on how high it is.";
            Status.Add(status);
            //
            status = new StatusInfo();
            status.Name = "Velocity";
            status.Description = "Increases your movement and attack speed.";
            Status.Add(status);
        }

        public void CreateMaxExpTable()
        {
            const int InitialMaxExp = 1200;
            const int IncreasePerLevel = 326;
            const int BoostPerLevel = 228;
            MaxExp = new int[201];
            for(int i = 0; i < 201; i++)
            {
                if (i == 0)
                    MaxExp[0] = 1;
                else
                {
                    int l = i - 1;
                    MaxExp[i] = InitialMaxExp +
                        (int)(IncreasePerLevel * (l * 0.5f) * (l * 0.5f)) +
                        BoostPerLevel * l;
                }
            }
        }

        public override int ExpFormula(int level, GameModeData gmd)
        {
            if (level < 0)
                return MaxExp[0];
            if (level >= MaxExp.Length)
                return MaxExp[MaxExp.Length - 1];
            return MaxExp[level];
        }

        public override void PlayerStatus(int Level, int UncappedLevel, Dictionary<byte, int> PointsCapped, Dictionary<byte, int> PointsInvested, out PlayerStatusMod mod)
        {
            int Offensive = PointsCapped[0],
                Defensive = PointsCapped[1],
                Health = PointsCapped[2],
                Mana = PointsCapped[3],
                Potence = PointsCapped[4],
                Velocity = PointsCapped[5];
            mod = new PlayerStatusMod();
            //Health
            mod.MaxHealthMult = 0.7f + Level * 0.01f + Level * Level * 0.005f;
            mod.MaxHealthSum = Health * 0.25f;
            //Mana
            mod.MaxManaMult = 0.5f + Level * 0.0095f;
            mod.ManaCostMult = mod.MaxManaMult;
            mod.MaxManaSum = Mana * 0.25f;
            //Offensive
            float DamageMult = 0.5f + Level * 0.01f;
            float DamageSum = Offensive * 0.2f;
            mod.MeleeDamageMult = mod.RangedDamageMult = mod.MagicDamageMult =
                mod.MinionDamageMult = DamageMult;
            mod.MeleeDamageSum = mod.RangedDamageSum = mod.MagicDamageSum =
                mod.MinionDamageSum = DamageMult;
            //Defensive
            mod.DefenseMult = 1f + Level * 0.01f;
            mod.DefenseSum = Defensive * 0.25f;
            //Potence
            mod.MeleeCritMult = mod.RangedCritMult = mod.MagicCritMult = 0.5f + Level * 0.005f;
            mod.MeleeCritSum = mod.RangedCritSum = mod.MagicCritSum = Potence * 0.001f;
            mod.KnockbackSum = 1f + Potence * 0.01f;
            mod.SummonCountSum += Potence * 0.025f;
            //Velocity
            float SpeedMult = 1f + Level * 0.005f;
            mod.MoveSpeedMult = SpeedMult;
            mod.MeleeSpeedMult = SpeedMult;
            mod.MoveSpeedSum = mod.MeleeSpeedSum = Velocity * 0.0025f;

            mod.LuckFactorSum = Level +
                Offensive * 0.1f + Defensive * 0.1f +
                Health * 0.2f + Mana * 0.1f +
                Potence * 0.25f + Velocity * 0.25f;
        }

        public override void NpcStatus(NPC npc, GameModeData Data)
        {
            int Level = Data.Level2, 
                ComplexityLevel = (int)((Level - 1) * 0.34f);
            if(npc.lifeMax > 5)npc.lifeMax = (int)(npc.lifeMax * (1f + Data.Level * 0.02f + ComplexityLevel * 0.1f));
            npc.damage = (int)(npc.damage * (1f + Data.Level * 0.01f + ComplexityLevel * 0.05f));
            if (npc.damage > 0)
                npc.damage += 4 * Level;
            npc.defense = (int)(npc.defense * (0.5f + Data.Level * 0.01f + ComplexityLevel * 0.05f));
            Data.Exp = (int)(npc.lifeMax * 1.2);
            if (NpcMod.IsOoAMob(npc.type))
            {
                Data.Exp = (int)(Data.Exp * 0.6);
            }
        }

        public override void BiomeLevelRules(Player player, GameModeData Data, out int MinLevel, out int MaxLevel)
        {
            MinLevel = 1;
            MaxLevel = 2;
            bool InEvent = false;
            if (Main.invasionType > 0 &&
                PlayerMod.IsPlayerInInvasionPosition(player))
            {
                switch (Main.invasionType)
                {
                    case InvasionID.GoblinArmy:
                        MinLevel = 22;
                        MaxLevel = 28;
                        break;
                    case InvasionID.PirateInvasion:
                        MinLevel = 62;
                        MaxLevel = 68;
                        break;
                    case InvasionID.SnowLegion:
                        MinLevel = 70;
                        MaxLevel = 76;
                        break;
                    case InvasionID.MartianMadness:
                        MinLevel = 74;
                        MaxLevel = 82;
                        break;
                }
                InEvent = true;
            }
            else if (Main.snowMoon)
            {
                MinLevel = 90;
                MaxLevel = 96;
                InEvent = true;
            }
            else if (Main.pumpkinMoon)
            {
                MinLevel = 82;
                MaxLevel = 88;
                InEvent = true;
            }
            else if (!Main.hardMode)
            {
                if (player.ZoneUnderworldHeight)
                {
                    MinLevel = 44;
                    MaxLevel = 50;
                }
                else if (player.ZoneDungeon)
                {
                    MinLevel = 36;
                    MaxLevel = 44;
                }
                else if (player.ZoneJungle)
                {
                    if (player.ZoneDirtLayerHeight)
                    {
                        MinLevel = 30;
                        MaxLevel = 36;
                    }
                    else
                    {
                        MinLevel = 26;
                        MaxLevel = 32;
                    }
                }
                else if (player.ZoneCorrupt || player.ZoneCrimson)
                {
                    if (!player.ZoneDirtLayerHeight)
                    {
                        MinLevel = 18;
                        MaxLevel = 24;
                    }
                    else
                    {
                        MinLevel = 22;
                        MaxLevel = 28;
                    }
                }
                else if (player.ZoneSnow)
                {
                    if (!player.ZoneDirtLayerHeight)
                    {
                        MinLevel = 7;
                        MaxLevel = 13;
                    }
                    else
                    {
                        MinLevel = 5;
                        MaxLevel = 10;
                    }
                }
                else if (player.ZoneDesert)
                {
                    if (player.ZoneUndergroundDesert)
                    {
                        MinLevel = 20;
                        MaxLevel = 26;
                    }
                    else
                    {
                        if (Main.dayTime)
                        {
                            MinLevel = 4;
                            MaxLevel = 9;
                        }
                        else
                        {
                            MinLevel = 8;
                            MaxLevel = 13;
                        }
                    }
                }
                else
                {
                    if (player.ZoneRockLayerHeight)
                    {
                        MinLevel = 9;
                        MaxLevel = 16;
                    }
                    else if (player.ZoneDirtLayerHeight)
                    {
                        MinLevel = 3;
                        MaxLevel = 12;
                    }
                    else
                    {
                        if (Main.dayTime)
                        {
                            MinLevel = 1;
                            MaxLevel = 5;
                        }
                        else
                        {
                            MinLevel = 4;
                            MaxLevel = 11;
                        }
                    }
                }
            }
            else
            {
                bool InsideJungleTemple = false;
                {
                    Terraria.DataStructures.Point16 TilePos = player.Center.ToTileCoordinates16();
                    InsideJungleTemple = (Main.tile[TilePos.X, TilePos.Y].wall == Terraria.ID.WallID.LihzahrdBrickUnsafe) ;
                }
                if(player.ZoneTowerNebula || player.ZoneTowerSolar || 
                    player.ZoneTowerStardust || player.ZoneTowerVortex)
                {
                    InEvent = true;
                    MinLevel = 90;
                    MaxLevel = 98;
                }
                else if (InsideJungleTemple)
                {
                    MinLevel = 76;
                    MaxLevel = 82;
                }
                else if (player.ZoneUnderworldHeight)
                {
                    if (!NPC.downedClown)
                    {
                        MinLevel = 44;
                        MaxLevel = 50;
                    }
                    else
                    {
                        MinLevel = 64;
                        MaxLevel = 70;
                    }
                }
                else if (player.ZoneDungeon)
                {
                    if (!NPC.downedPlantBoss)
                    {
                        MinLevel = 36;
                        MaxLevel = 44;
                    }
                    else
                    {
                        MinLevel = 82;
                        MaxLevel = 90;
                    }
                }
                else if (player.ZoneJungle)
                {
                    if (player.ZoneDirtLayerHeight)
                    {
                        MinLevel = 70;
                        MaxLevel = 78;
                    }
                    else
                    {
                        MinLevel = 65;
                        MaxLevel = 72;
                    }
                }
                else if (player.ZoneCorrupt || player.ZoneCrimson || player.ZoneHoly)
                {
                    if (!player.ZoneDirtLayerHeight)
                    {
                        MinLevel = 52;
                        MaxLevel = 60;
                    }
                    else
                    {
                        MinLevel = 58;
                        MaxLevel = 66;
                    }
                }
                else
                {
                    if (player.ZoneRockLayerHeight)
                    {
                        MinLevel = 56;
                        MaxLevel = 60;
                    }
                    else if (player.ZoneDirtLayerHeight)
                    {
                        MinLevel = 54;
                        MaxLevel = 58;
                    }
                    else
                    {
                        if (Main.dayTime)
                        {
                            MinLevel = 50;
                            MaxLevel = 56;
                        }
                        else
                        {
                            MinLevel = 54;
                            MaxLevel = 60;
                        }
                    }
                }
            }
            if (Main.expertMode)
            {
                if (InEvent)
                {
                    MinLevel += 100;
                    MaxLevel += 100;
                }
                else
                {
                    MinLevel = 100 + (int)(MinLevel * 0.5f);
                    MaxLevel = 100 + (int)(MaxLevel * 0.5f);
                }
            }
        }

        public override int MobSpawnLevel(NPC npc)
        {
            int Level = 1;
            switch (npc.type)
            {
                default:
                    return base.MobSpawnLevel(npc);
                case NPCID.EyeofCthulhu:
                    Level = 12;
                    break;
                case NPCID.KingSlime:
                    Level = 25;
                    break;
                case NPCID.EaterofWorldsHead:
                case NPCID.EaterofWorldsBody:
                case NPCID.EaterofWorldsTail:
                    Level = 27;
                    break;
                case NPCID.BrainofCthulhu:
                case NPCID.Creeper:
                    Level = 27;
                    break;
                case NPCID.SkeletronHead:
                case NPCID.SkeletronHand:
                    Level = 40;
                    break;
                case NPCID.QueenBee:
                    Level = 32;
                    break;
                case NPCID.WallofFlesh:
                case NPCID.WallofFleshEye:
                case NPCID.TheHungry:
                case NPCID.TheHungryII:
                    Level = 50;
                    break;
                //
                case NPCID.Spazmatism:
                case NPCID.Retinazer:
                    Level = 60;
                    break;
                case NPCID.TheDestroyer:
                case NPCID.TheDestroyerBody:
                case NPCID.TheDestroyerTail:
                    Level = 65;
                    break;
                case NPCID.SkeletronPrime:
                case NPCID.PrimeCannon:
                case NPCID.PrimeLaser:
                case NPCID.PrimeSaw:
                case NPCID.PrimeVice:
                    Level = 70;
                    break;
                case NPCID.Plantera:
                case NPCID.PlanterasHook:
                case NPCID.PlanterasTentacle:
                    Level = 78;
                    break;
                case NPCID.Golem:
                case NPCID.GolemFistLeft:
                case NPCID.GolemFistRight:
                case NPCID.GolemHead:
                case NPCID.GolemHeadFree:
                    Level = 82;
                    break;
                case NPCID.DukeFishron:
                    Level = 80;
                    break;
                case NPCID.CultistBoss:
                    Level = 90;
                    break;
                case NPCID.MoonLordCore:
                case NPCID.MoonLordHand:
                case NPCID.MoonLordHead:
                    Level = 100;
                    break;
            }
            if (Level < 50 && Main.hardMode)
                Level += 50;
            if (Main.expertMode)
                Level = 100 + Level;
            return Level;
        }

        public override int GetDigExp(int ID)
        {
            switch (ID)
            {
                case TileID.Pots:
                    return 30;
                case TileID.Copper:
                case TileID.Tin:
                    return 3;
                case TileID.Iron:
                case TileID.Lead:
                    return 5;
                case TileID.Silver:
                case TileID.Tungsten:
                    return 7;
                case TileID.Gold:
                case TileID.Platinum:
                    return 9;
                case TileID.Hellstone:
                    return 15;
                case TileID.Silt:
                case TileID.Slush:
                    return 6;
                case TileID.DesertFossil:
                    return 10;
                case TileID.Cobalt:
                case TileID.Palladium:
                    return 230;
                case TileID.Mythril:
                case TileID.Orichalcum:
                    return 460;
                case TileID.Adamantite:
                case TileID.Titanium:
                    return 890;
                case TileID.Chlorophyte:
                    return 1192;
                case TileID.Heart:
                    return 200;
                case TileID.ShadowOrbs:
                    return 1000;
                case TileID.Cobweb:
                    return 4;
            }
            return base.GetDigExp(ID);
        }

        public override int GetExpReward(float Level, float Difficulty, ExpReceivedPopText.ExpSource source, GameModeData gmd)
        {
            if(source == ExpReceivedPopText.ExpSource.Extractinator)
                return (int)(Math.Max(1, base.GetExpReward(Level, Difficulty, source, gmd) * 0.2f));
            return base.GetExpReward(Level, Difficulty, source, gmd);
        }
    }
}
