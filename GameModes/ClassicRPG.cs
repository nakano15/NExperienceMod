using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using Terraria.ID;

namespace NExperience.GameModes
{
    public class ClassicRPG : GameModeBase
    {
        public const string ModeID = "classicrpg";
        private int[] MaxExpTable = new int[101];

        public ClassicRPG()
        {
            GameModeID = ModeID;
            Name = "Classic RPG";
            WikiPageID = "Classic_RPG_Mode";
            Description = "An attempt of recreation of the early N Terraria rpg mode.";
            MaxLevel = 100;
            GenerateMaxExpTable();
            StatusPointsPerLevel = 10;
            StatusList();
        }

        public void GenerateMaxExpTable()
        {
            int Exp = 125;
            int Divisor = 2;
            float DivisorValue = 1f / Divisor;
            int DivisorIncreaseDelay = 7;
            int NextDivisorIncreaseLevel = DivisorIncreaseDelay;
            MaxExpTable[0] = 0;
            for (int i = 1; i <= MaxLevel; i++)
            {
                MaxExpTable[i] = Exp;
                Exp += (int)(Exp * DivisorValue);
                if(i == NextDivisorIncreaseLevel)
                {
                    NextDivisorIncreaseLevel += DivisorIncreaseDelay;
                    Divisor++;
                    DivisorValue = 1f / Divisor;
                }
            }
        }

        public override void PlayerStatus(int Level, int UncappedLevel, Dictionary<byte, int> PointsCapped, Dictionary<byte, int> PointsInvested, out PlayerStatusMod mod)
        {
            const float StatusProgressionPerLevel = 0.5f;
            mod = new PlayerStatusMod();
            float MeleeBonus = StatusProgressionPerLevel * Level + PointsCapped[0] * 0.1f,
                RangedBonus = StatusProgressionPerLevel * Level + PointsCapped[1] * 0.1f,
                MagicBonus = StatusProgressionPerLevel * Level + PointsCapped[2] * 0.1f,
                SummonBonus = StatusProgressionPerLevel * Level + PointsCapped[3] * 0.1f,
                DefenseBonus = StatusProgressionPerLevel * Level + PointsCapped[4] * 0.1f,
                CriticalBonus = StatusProgressionPerLevel * Level + PointsCapped[5] * 0.1f,
                AttackSpeedBonus = StatusProgressionPerLevel * Level + PointsCapped[6] * 0.1f,
                MovementSpeedBonus = StatusProgressionPerLevel * Level + PointsCapped[7] * 0.1f,
                KnockbackBonus = StatusProgressionPerLevel * Level + PointsCapped[8] * 0.1f,
                DodgeBonus = StatusProgressionPerLevel * Level + PointsCapped[9] * 0.1f;
            float StatusMultiplier = !Main.hardMode ? 0.05f : 0.075f;
            mod.MeleeDamageSum += StatusMultiplier * MeleeBonus;
            mod.RangedDamageSum += StatusMultiplier * RangedBonus;
            mod.MagicDamageSum += StatusMultiplier * MagicBonus;
            mod.NeutralDamageSum += StatusProgressionPerLevel * Level;
            //
            StatusMultiplier = !Main.hardMode ? 0.5f : 0.75f;
            mod.DefenseSum += StatusMultiplier * DefenseBonus;
            mod.MaxHealthMult += Level * 0.0025f;
            mod.MaxHealthSum += DefenseBonus * 0.075f;
            StatusMultiplier = 1f / StatusMultiplier;
            mod.MeleeCritSum += CriticalBonus * StatusMultiplier;
            mod.RangedCritSum += CriticalBonus * StatusMultiplier;
            mod.MagicCritSum += CriticalBonus * StatusMultiplier;
            mod.DodgeRate = DodgeBonus * (1f / 4);
            //
            StatusMultiplier = !Main.hardMode ? 0.003f : 0.001f;
            mod.MeleeSpeedSum += StatusMultiplier * AttackSpeedBonus;
            mod.MoveSpeedSum += StatusMultiplier * MovementSpeedBonus;
            mod.KnockbackMult += StatusMultiplier * KnockbackBonus;
            //
            mod.ArmorPenetrationMult += StatusProgressionPerLevel * Level;
        }

        public void StatusList()
        {
            const int MaxPoints = 1000;
            StatusInfo status = new StatusInfo();
            status.Name = "Melee";
            status.Description = "Increases your Melee damage.";
            status.InitialPoints = 0;
            status.MaxPoints = MaxPoints;
            Status.Add(status);
            status = new StatusInfo();
            status.Name = "Ranged";
            status.Description = "Increases your Ranged damage.";
            status.InitialPoints = 0;
            status.MaxPoints = MaxPoints;
            Status.Add(status);
            status = new StatusInfo();
            status.Name = "Magic";
            status.Description = "Increases your Magic damage.";
            status.InitialPoints = 0;
            status.MaxPoints = MaxPoints;
            Status.Add(status);
            status = new StatusInfo();
            status.Name = "Summon";
            status.Description = "Increases your Summon damage.";
            status.InitialPoints = 0;
            status.MaxPoints = MaxPoints;
            Status.Add(status);
            status = new StatusInfo();
            status.Name = "Defense";
            status.Description = "Increases your Defense.";
            status.InitialPoints = 0;
            status.MaxPoints = MaxPoints;
            Status.Add(status);
            status = new StatusInfo();
            status.Name = "Critical";
            status.Description = "Increases all Critical rates.";
            status.InitialPoints = 0;
            status.MaxPoints = MaxPoints;
            Status.Add(status);
            status = new StatusInfo();
            status.Name = "ASPD";
            status.Description = "Increases your Melee Attack Speed.";
            status.InitialPoints = 0;
            status.MaxPoints = MaxPoints;
            Status.Add(status);
            status = new StatusInfo();
            status.Name = "Move Speed";
            status.Description = "Increases your Movement Speed.";
            status.InitialPoints = 0;
            status.MaxPoints = MaxPoints;
            Status.Add(status);
            status = new StatusInfo();
            status.Name = "Knockback";
            status.Description = "Increases your Knockback.";
            status.InitialPoints = 0;
            status.MaxPoints = MaxPoints;
            Status.Add(status);
            status = new StatusInfo();
            status.Name = "Dodge";
            status.Description = "Increases your Dodge rate.";
            status.InitialPoints = 0;
            status.MaxPoints = MaxPoints;
            Status.Add(status);
        }

        public override void NpcStatus(NPC npc, GameModeData Data)
        {
            int Level = Data.Level2;
            if(npc.damage > 0) npc.damage += (int)(Level * 0.333f);
            npc.defense += (int)(Level * 0.333f);
            if (npc.lifeMax > 5)
            {
                float HealthBonusStack = 0;
                float HealthDivisor = 1f / 25;
                for (int i = 1; i < Level; i++)
                {
                    HealthBonusStack += npc.lifeMax * HealthDivisor;
                    switch (i)
                    {
                        case 10:
                            HealthDivisor = 1f / 22;
                            break;
                        case 25:
                            HealthDivisor = 1f / 19;
                            break;
                        case 40:
                            HealthDivisor = 1f / 16;
                            break;
                        case 65:
                            HealthDivisor = 1f / 12;
                            break;
                        case 85:
                            HealthDivisor = 1f / 10;
                            break;
                        case 90:
                            HealthDivisor = 1f / 8;
                            break;
                        case 95:
                            HealthDivisor = 1f / 5;
                            break;
                    }
                }
                npc.lifeMax += (int)HealthBonusStack;
            }
            if (Level >= 25)
            {
                if (npc.lifeMax > 5) npc.lifeMax += 100;
                if (npc.damage > 0) npc.damage++;
                npc.defense++;
            }
            if (Level >= 50)
            {
                if (npc.lifeMax > 5) npc.lifeMax += 250;
                if (npc.damage > 0) npc.damage += 2;
                npc.defense += 2;
            }
            if (Level >= 70)
            {
                if (npc.lifeMax > 5) npc.lifeMax += 350;
                if (npc.damage > 0) npc.damage += 3;
                npc.defense += 3;
            }
            if (Level >= 90)
            {
                if (npc.lifeMax > 5) npc.lifeMax += 400;
                if (npc.damage > 0) npc.damage += 4;
                npc.defense += 4;
            }
            if (Level >= 120)
            {
                if (npc.lifeMax > 5) npc.lifeMax += 500;
                if (npc.damage > 0) npc.damage += 5;
                npc.defense += 5;
            }
            if (Level >= 150)
            {
                if (npc.lifeMax > 5) npc.lifeMax += 750;
                if (npc.damage > 0) npc.damage += 5;
                npc.defense += 5;
            }
            bool HMBoss = false;
            switch (npc.type)
            {
                case NPCID.Spazmatism:
                case NPCID.Retinazer:
                case NPCID.SkeletronPrime:
                case NPCID.PrimeCannon:
                case NPCID.PrimeLaser:
                case NPCID.PrimeSaw:
                case NPCID.PrimeVice:
                case NPCID.TheDestroyer:
                case NPCID.TheDestroyerBody:
                case NPCID.TheDestroyerTail:
                case NPCID.Plantera:
                case NPCID.PlanterasHook:
                case NPCID.PlanterasTentacle:
                case NPCID.Golem:
                case NPCID.GolemFistLeft:
                case NPCID.GolemFistRight:
                case NPCID.GolemHead:
                case NPCID.GolemHeadFree:
                case NPCID.DukeFishron:
                case NPCID.CultistBoss:
                case NPCID.CultistBossClone:
                case NPCID.MoonLordCore:
                case NPCID.MoonLordHand:
                case NPCID.MoonLordHead:
                    HMBoss = true;
                    break;
            }
            if (Terraria.ID.NPCID.Sets.TechnicallyABoss[npc.type])
            {
                if (Level >= 25)
                {
                    float Mult = !HMBoss ? 0.05f : 0.01f;
                    npc.lifeMax += (int)(npc.lifeMax * 0.25f);
                    if (npc.damage > 0) npc.damage += (int)(npc.damage * Mult);
                    npc.defense += (int)(npc.defense * Mult);
                }
                if (Level >= 50)
                {
                    float Mult = !HMBoss ? 0.1f : 0.03f;
                    npc.lifeMax += (int)(npc.lifeMax * 0.4f);
                    if (npc.damage > 0) npc.damage += (int)(npc.damage * Mult);
                    npc.defense += (int)(npc.defense * Mult);
                }
                if (Level >= 75)
                {
                    npc.lifeMax += (int)(npc.lifeMax * 0.65f);
                    if (npc.damage > 0) npc.damage += (int)(npc.damage * 0.03f);
                    npc.defense += (int)(npc.defense * 0.03f);
                }
                if (Level >= 100)
                {
                    float Mult = !HMBoss ? 0.3f : 0.1f;
                    npc.lifeMax *= 2;
                    if (npc.damage > 0) npc.damage += (int)(npc.damage * Mult);
                    npc.defense += (int)(npc.defense * Mult);
                }
            }
            if(npc.aiStyle == 27)
            {
                float StatusBonus = 0;
                for(int p = 0; p < 255; p++)
                {
                    if (Main.player[p].active && p != npc.target)
                    {
                        StatusBonus += PlayerMod.GetPlayerLevel(Main.player[p], true);
                    }
                }
                npc.lifeMax += (int)(npc.life * 0.1f * StatusBonus);
                npc.defense += (int)(npc.defense * 0.01f * StatusBonus);
            }
            if (HMBoss)
            {
                npc.lifeMax = (int)(npc.lifeMax * 0.3f);
                if (npc.damage > 0) npc.damage = (int)(npc.damage * 1.25f);
                npc.defense = (int)(npc.defense * 0.6f);
            }
            if (npc.lifeMax > 5)
            {
                float ExpMult = !Main.hardMode ? 0.01f : 0.005f;
                if (Data.Exp == 0)
                    Data.Exp = (int)(Data.GetMaxExp() * ExpMult);
            }
        }

        public override int AttackExp(GameModeData playerdata, int Damage, bool Critical, int OponentLevel)
        {
            return 0;
            //return Damage;
        }

        public override int MobSpawnLevel(NPC npc)
        {
            if (Terraria.ID.NPCID.Sets.TechnicallyABoss[npc.type])
            {
                int LevelSum = 0, PlayersChecked = 0;
                for(int p = 0; p < 255; p++)
                {
                    if (Main.player[p].active)
                    {
                        LevelSum += PlayerMod.GetPlayerLevel(Main.player[p], false);
                        PlayersChecked++;
                    }
                }
                if (PlayersChecked > 0)
                    return (int)(LevelSum / PlayersChecked);
            }
            else
            {
                switch (npc.type)
                {
                    case NPCID.GoblinArcher:
                    case NPCID.GoblinPeon:
                    case NPCID.GoblinScout:
                    case NPCID.GoblinSorcerer:
                    case NPCID.GoblinSummoner:
                    case NPCID.GoblinThief:
                    case NPCID.GoblinWarrior:
                        if (Main.hardMode)
                        {
                            return Main.rand.Next(60, 68);
                        }
                        else
                        {
                            return Main.rand.Next(23, 29);
                        }

                    case NPCID.SnowBalla:
                    case NPCID.SnowmanGangsta:
                    case NPCID.MisterStabby:
                        return Main.rand.Next(75, 84);

                    case NPCID.PirateCaptain:
                    case NPCID.PirateCorsair:
                    case NPCID.PirateCrossbower:
                    case NPCID.PirateDeadeye:
                    case NPCID.PirateDeckhand:
                    case NPCID.PirateShip:
                    case NPCID.PirateShipCannon:
                    case NPCID.Parrot:
                        return Main.rand.Next(71, 79);

                    case NPCID.MartianDrone:
                    case NPCID.MartianEngineer:
                    case NPCID.MartianOfficer:
                    case NPCID.MartianProbe:
                    case NPCID.MartianSaucer:
                    case NPCID.MartianSaucerCannon:
                    case NPCID.MartianSaucerCore:
                    case NPCID.MartianSaucerTurret:
                    case NPCID.MartianTurret:
                    case NPCID.MartianWalker:
                    case NPCID.BrainScrambler:
                    case NPCID.Scutlix:
                    case NPCID.ScutlixRider:
                    case NPCID.GigaZapper:
                    case NPCID.RayGunner:
                    case NPCID.GrayGrunt:
                        return Main.rand.Next(100, 106);
                }
            }
            return base.MobSpawnLevel(npc);
        }

        public override int ExpFormula(int level, GameModeData gmd)
        {
            return MaxExpTable[level];
        }

        public override int GetDigExp(int TileID)
        {
            switch (TileID)
            {
                case 0:
                case 1:
                case 2:
                case 23:
                case 40:
                case 53:
                case 57:
                case 59:
                case 60:
                case 70:
                    return !Main.hardMode ? 1 : 10;
                case 3:
                case 24:
                case 61:
                case 71:
                case 73:
                case 74:
                    return 1;
                case 5:
                    return !Main.hardMode ? 10 : 100;
                case 6:
                case 167:
                    return 10;
                case 7:
                case 166:
                    return 5;
                case 8:
                case 169:
                    return 50;
                case 9:
                case 168:
                    return 25;
                case 12:
                    return 1000;
                case 22:
                case 204:
                    return 100;
                case 25:
                    return 25;
                case 26:
                    return 40000;
                case 28:
                    return !Main.hardMode ? 100 : 500;
                case 31:
                    return 6000;
                case 32:
                case 69:
                    return 15;
                case 37:
                    return 75;
                case 48:
                    return 70;
                case 51:
                    return 25;
                case 52:
                case 62:
                    return 5;
                case 56:
                    return 80;
                case 58:
                    return 150;
                case 63:
                case 64:
                case 65:
                case 66:
                case 67:
                case 68:
                    return 50;
                case 72:
                    return !Main.hardMode ? 40 : 400;
                case 80:
                    return !Main.hardMode ? 40 : 400;
                case 82:
                    return 5;
                case 83:
                    return 10;
                case 84:
                    return 25;
                case 85:
                    return 15;
                case 107:
                case 221:
                    return 1000;
                case 108:
                case 222:
                    return 2000;
                case 109:
                case 112:
                case 116:
                case 117:
                case 123:
                    return !Main.hardMode ? 1 : 10;
                case 110:
                case 113:
                    return 1;
                case 111:
                case 223:
                    return 4000;
                case 115:
                    return 5;
                case 141:
                    return 100;
                case 147:
                    return !Main.hardMode ? 1 : 10;
            }
            return 0;
        }

        public override void BiomeLevelRules(Player player, GameModeData Data, out int MinLevel, out int MaxLevel)
        {
            MinLevel = 1;
            MaxLevel = 3;
            bool Deep = player.GetModPlayer<PlayerMod>().ZoneDeep;
            bool LihzahrdDungeon = Framing.GetTileSafely(player.Center.ToTileCoordinates()).wall == Terraria.ID.WallID.LihzahrdBrickUnsafe;
            if (LihzahrdDungeon)
            {
                MinLevel = 80;
                MaxLevel = 90;
            }
            else if(player.ZoneTowerNebula || player.ZoneTowerSolar || player.ZoneTowerStardust || player.ZoneTowerVortex)
            {
                MinLevel = 90;
                MaxLevel = 100;
            }
            else if (Main.pumpkinMoon && player.ZoneOverworldHeight)
            {
                MinLevel = 110;
                MaxLevel = 120;
            }
            else if (Main.snowMoon && player.ZoneOverworldHeight)
            {
                MinLevel = 120;
                MaxLevel = 130;
            }
            else if (player.ZoneMeteor)
            {
                MinLevel = 33;
                MaxLevel = 41;
            }
            else if (player.GetModPlayer<PlayerMod>().ZoneGraveyard)
            {
                if (!Main.hardMode)
                {
                    MinLevel = 20;
                    MaxLevel = 30;
                }
                else
                {
                    MinLevel = 60;
                    MaxLevel = 70;
                }
            }
            else if (player.ZoneSkyHeight)
            {
                if (!Main.hardMode)
                {
                    if (Main.dayTime)
                    {
                        MinLevel = 19;
                        MaxLevel = 26;
                    }
                    else
                    {
                        MinLevel = 22;
                        MaxLevel = 29;
                    }
                }
                else
                {
                    if (Main.dayTime)
                    {
                        MinLevel = 67;
                        MaxLevel = 76;
                    }
                    else
                    {
                        MinLevel = 74;
                        MaxLevel = 82;
                    }
                }
            }
            else if (player.ZoneDungeon)
            {
                if (!NPC.downedBoss3)
                {
                    MinLevel = 2576;
                    MaxLevel = 3958;
                }
                else
                {
                    MinLevel = 30;
                    MaxLevel = 40;
                    if (Deep)
                    {
                        MinLevel += 5;
                        MaxLevel += 5;
                    }
                }
            }
            else if (player.ZoneUnderworldHeight)
            {
                if (!Main.hardMode)
                {
                    MinLevel = 40;
                    MaxLevel = 50;
                }
                else if (!NPC.downedClown)
                {
                    MinLevel = 50;
                    MaxLevel = 55;
                }
                else
                {
                    MinLevel = 90;
                    MaxLevel = 100;
                }
            }
            else if (player.ZoneJungle)
            {
                if (!player.ZoneRockLayerHeight)
                {
                    if (!Main.hardMode)
                    {
                        MinLevel = 30;
                        MaxLevel = 37;
                    }
                    else
                    {
                        MinLevel = 68;
                        MaxLevel = 75;
                    }
                }
                else
                {
                    if (!Deep)
                    {
                        if (!Main.hardMode)
                        {
                            MinLevel = 34;
                            MaxLevel = 40;
                        }
                        else
                        {
                            MinLevel = 75;
                            MaxLevel = 79;
                        }
                    }
                    else
                    {
                        if (!Main.hardMode)
                        {
                            MinLevel = 37;
                            MaxLevel = 42;
                        }
                        else
                        {
                            MinLevel = 79;
                            MaxLevel = 85;
                        }
                    }
                }
            }
            else if (player.ZoneCorrupt || player.ZoneCrimson)
            {
                if (!player.ZoneRockLayerHeight)
                {
                    if (!Main.hardMode)
                    {
                        MinLevel = 23;
                        MaxLevel = 27;
                    }
                    else
                    {
                        MinLevel = 47;
                        MaxLevel = 53;
                    }
                }
                else
                {
                    if (!Deep)
                    {
                        if (!Main.hardMode)
                        {
                            MinLevel = 25;
                            MaxLevel = 31;
                        }
                        else
                        {
                            MinLevel = 29;
                            MaxLevel = 36;
                        }
                    }
                    else
                    {
                        if (!Main.hardMode)
                        {
                            MinLevel = 23;
                            MaxLevel = 27;
                        }
                        else
                        {
                            MinLevel = 53;
                            MaxLevel = 59;
                        }
                    }
                }
            }
            else if (player.ZoneHoly)
            {
                if (!player.ZoneRockLayerHeight)
                {
                    if (!Main.hardMode)
                    {
                        MinLevel = 32;
                        MaxLevel = 41;
                    }
                    else
                    {
                        MinLevel = 48;
                        MaxLevel = 55;
                    }
                }
                else
                {
                    if (!Deep)
                    {
                        if (!Main.hardMode)
                        {
                            MinLevel = 36;
                            MaxLevel = 44;
                        }
                        else
                        {
                            MinLevel = 55;
                            MaxLevel = 62;
                        }
                    }
                    else
                    {
                        if (!Main.hardMode)
                        {
                            MinLevel = 42;
                            MaxLevel = 45;
                        }
                        else
                        {
                            MinLevel = 62;
                            MaxLevel = 75;
                        }
                    }
                }
            }
            else if (player.ZoneDesert)
            {
                if (!Main.hardMode)
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
            else if (player.ZoneRockLayerHeight)
            {
                if (!Main.hardMode)
                {
                    MinLevel = 15;
                    MaxLevel = 28;
                }
                else
                {
                    if (!Deep)
                    {
                        MinLevel = 67;
                        MaxLevel = 75;
                    }
                    else
                    {
                        MinLevel = 75;
                        MaxLevel = 80;
                    }
                }
            }
            else if (player.ZoneDirtLayerHeight)
            {
                if (!Main.hardMode)
                {
                    MinLevel = 9;
                    MaxLevel = 17;
                }
                else
                {
                    MinLevel = 60;
                    MaxLevel = 68;
                }
            }
            else if (player.ZoneSnow)
            {
                if (!Main.hardMode)
                {
                    if (Main.dayTime)
                    {
                        MinLevel = 1;
                        MaxLevel = 10;
                    }
                    else
                    {
                        MinLevel = 7;
                        MaxLevel = 15;
                    }
                }
                else
                {
                    if (Main.dayTime)
                    {
                        MinLevel = 52;
                        MaxLevel = 55;
                    }
                    else
                    {
                        MinLevel = 55;
                        MaxLevel = 62;
                    }
                }
            }
            else if (player.ZoneOverworldHeight)
            {
                if (!Main.hardMode)
                {
                    if (Main.dayTime)
                    {
                        MinLevel = 1;
                        MaxLevel = 8;
                    }
                    else
                    {
                        MinLevel = 7;
                        MaxLevel = 15;
                    }
                }
                else
                {
                    if (Main.dayTime)
                    {
                        MinLevel = 44;
                        MaxLevel = 56;
                    }
                    else
                    {
                        MinLevel = 55;
                        MaxLevel = 60;
                    }
                }
            }
        }
    }
}
