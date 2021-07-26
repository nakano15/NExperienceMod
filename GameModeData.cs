using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace NExperience
{
    public class GameModeData
    {
        public GameModeBase Base { get { return MainMod.GetGameMode(GameModeID); } }
        public string GameModeID = "";
        public int Level = 1, Level2 = -1;
        public int Exp = 0, MaxExp = -1;
        public int StatusPoints = 0;
        public int LastFrameHealth = -1, LastFrameMaxHealth = -1;
        public int OriginalHealth = -1;
        public float OriginalKB = 1f;
        public float HealthChangePercentage = 1f, ManaChangePercentage = 1f;
        public float ProjDamageMult = 1f;
        public bool RecalcStatus = false;
        public PlayerStatusMod SavedStatusMod;
        private PlayerRebirthStatus SavedRebirthStatus = null;
        public int RebirthLevel = 0, AP = 0;
        public Dictionary<byte, int> PointsSpent = new Dictionary<byte, int>(),
            PointsUnderEffect = new Dictionary<byte,int>();
        private List<byte> PointsToSpend = new List<byte>();
        public bool? IsHardmodeMonster = null;
        public float ExpMult
        {
            get
            {
                if (RebirthLevel == 0)
                    return 1f;
                float Exp = (float)Math.Ceiling(Math.Pow(0.57142857142857142857142857142857f, RebirthLevel));

                return Exp;
            }
        }
        public int RBLevelRequired
        {
            get
            {
                int Lv = 125;

                return Lv;
            }
        }

        public bool HasPointsToSpend { get { return PointsToSpend.Count > 0; } }

        public PlayerRebirthStatus GetRebirthStatus()
        {
            if (SavedRebirthStatus == null)
                SavedRebirthStatus = new PlayerRebirthStatus();
            return SavedRebirthStatus;
        }

        public GameModeData()
        {
            if(Base != null)
                Level = Base.InitialLevel;
        }

        public void ForceSetStatusValue(byte Status, int Value)
        {
            if (PointsSpent.ContainsKey(Status))
                PointsSpent[Status] = Value;
            else
                PointsSpent.Add(Status, Value);
        }

        public void SetSpawnHealthValue(Player player)
        {
            LastFrameHealth = (int)(player.statLifeMax2 * (100f / player.statLifeMax2));
        }

        public void ChangePointsToSpend(byte Status, int Value)
        {
            while (StatusPoints > 0 && Value > 0)
            {
                PointsToSpend.Add(Status);
                StatusPoints--;
                Value--;
            }
            while (Value < 0)
            {
                if (PointsToSpend.Exists(x => Status == x))
                {
                    PointsToSpend.Remove(Status);
                    StatusPoints++;
                }
                else
                {
                    break;
                }
                Value++;
            }
        }

        public int GetPointsToSpend(byte Status)
        {
            return PointsToSpend.Count(x => x == Status);
        }

        public void InvestPoints()
        {
            while (PointsToSpend.Count > 0)
            {
                ChangePointsInvested(PointsToSpend[0], 1);
                PointsToSpend.RemoveAt(0);
            }
            RecalcStatus = true;
        }

        public void ResetPointsToSpend()
        {
            StatusPoints += PointsToSpend.Count;
            PointsToSpend.Clear();
        }

        public void ResetPointsInvested()
        {
            byte[] Keys = PointsSpent.Keys.ToArray();
            foreach (byte i in Keys)
            {
                StatusPoints += PointsSpent[i];
                PointsSpent[i] = 0;
            }
            RecalcStatus = true;
        }

        public int GetPointsInvested(byte Status)
        {
            if (!PointsSpent.ContainsKey(Status))
                return 0;
            return PointsSpent[Status];
        }

        public int GetPointsUnderEffect(byte Status)
        {
            if (!PointsUnderEffect.ContainsKey(Status))
                return 0;
            return PointsUnderEffect[Status];
        }

        public int GetPointDifference(byte Status)
        {
            int PointInvested = 0, PointEffect = 0;
            if (PointsSpent.ContainsKey(Status))
                PointInvested = PointsSpent[Status];
            if(PointsUnderEffect.ContainsKey(Status))
                PointEffect = PointsUnderEffect[Status];
            return PointEffect - PointInvested;
        }

        public int GetTotalPointsInvested()
        {
            int Points = 0;
            foreach (byte k in PointsSpent.Keys)
                Points += PointsSpent[k];
            return Points;
        }

        public void ChangePointsInvested(byte Status, int Value)
        {
            if (!PointsSpent.ContainsKey(Status))
                PointsSpent.Add(Status, Value);
            else
                PointsSpent[Status] += Value;
        }

        public void ChangeExp(int Value, Player player)
        {
            if (MainMod.InfiniteLeveling || Level < Base.MaxLevel)
            {
                try
                {
                    checked
                    {
                        Exp += Value;
                    }
                }
                catch
                {
                    if (Value > 0)
                        Exp = int.MaxValue;
                    else
                        Exp = int.MinValue;
                }
            }
        }

        public int GetMaxExp()
        {
            int LevelToGetExp = Level;
            int ResultingExp = 0;
            while (LevelToGetExp > Base.MaxLevel)
            {
                LevelToGetExp -= Base.MaxLevel;
                ResultingExp += Base.ExpFormula(Base.MaxLevel, this);
                if (ResultingExp > GameModeBase.MaxExpPossible || ResultingExp < 0)
                    return GameModeBase.MaxExpPossible;
            }
            ResultingExp += Base.ExpFormula(LevelToGetExp, this);
            if (ResultingExp < 0 || ResultingExp > GameModeBase.MaxExpPossible)
                ResultingExp = GameModeBase.MaxExpPossible;
            return ResultingExp;
        }

        public void UpdatePlayer(Player player)
        {
            if (MaxExp == -1)
                MaxExp = GetMaxExp();
            int LastLevel2 = Level2;
            Level2 = Level;
            bool LeveledUp = false;
            if (player.whoAmI == Main.myPlayer)
            {
                bool IsFreeMode = Base is GameModes.FreeMode;
                while ((Level < Base.MaxLevel || MainMod.InfiniteLeveling) && Exp >= MaxExp)
                {
                    Exp -= MaxExp;
                    Level++;
                    if (IsFreeMode)
                    {
                        LeveledUp = Level % 100 == 0;
                    }
                    else
                    {
                        LeveledUp = true;
                    }
                    MaxExp = GetMaxExp();
                }
                if (LeveledUp)
                {
                    if (IsFreeMode)
                    {
                        Main.NewText("You reached rank " + GameModes.FreeMode.RomanAlgarismMaker(Level / 100 + 1) + ".");
                    }
                    else
                    {
                        Main.NewText("You reached level " + Level + ".");
                    }
                    if (MainMod.FullRegenOnLevelUp)
                    {
                        player.statLife = player.statLifeMax2;
                        player.statMana = player.statManaMax2;
                    }
                    if (Main.netMode == 1)
                        NetPlayMod.SendPlayerLevel(player.whoAmI, -1, player.whoAmI);
                }
            }
            Base.BiomeLevelRules(player, this, out player.GetModPlayer<PlayerMod>().BiomeMinLv, out player.GetModPlayer<PlayerMod>().BiomeMaxLv);
            if (WorldMod.IsDeathMode)
            {
                Level2 = 1;
            }
            else
            {
                if ((!MainMod.InfiniteLeveling || MainMod.CapLevelOnInfiniteLeveling) && Level2 > Base.MaxLevel)
                {
                    Level2 = Base.MaxLevel;
                }
                if (MainMod.LevelCapping && Base.AllowLevelCapping)
                {
                    int BiomeMinLevel = player.GetModPlayer<PlayerMod>().BiomeMinLv, BiomeMaxLevel = player.GetModPlayer<PlayerMod>().BiomeMaxLv;
                    if (Level2 > BiomeMaxLevel)
                        Level2 = BiomeMaxLevel;
                    int OverLevelValue = 1;
                    for (int n = 0; n < 200; n++)
                    {
                        if (Main.npc[n].active && !Main.npc[n].friendly && !Main.npc[n].townNPC)
                        {
                            if (Math.Abs(Main.npc[n].Center.X - player.Center.X) < NPC.sWidth * 2 && Math.Abs(Main.npc[n].Center.Y - player.Center.Y) < NPC.sHeight * 2)
                            {
                                int NpcStatusLevel = Main.npc[n].GetGlobalNPC<NpcMod>().NpcStatus.Level2,
                                    NpcBaseLevel = Main.npc[n].GetGlobalNPC<NpcMod>().NpcStatus.Level;
                                if (Level2 < NpcStatusLevel)
                                {
                                    Level2 = NpcStatusLevel;
                                    if (NpcBaseLevel < NpcStatusLevel)
                                        OverLevelValue = NpcStatusLevel;
                                }
                            }
                        }
                    }
                    if (OverLevelValue < Level)
                        OverLevelValue = Level;
                    if (Level2 > OverLevelValue)
                        Level2 = OverLevelValue;
                }
            }
            int BeforeScaleMaxHealth = player.statLifeMax2, BeforeScaleMaxMana = player.statManaMax2;
            bool StatusChanged = false;
            if (LastLevel2 != Level2 || RecalcStatus)
            {
                StatusChanged = true;
                RecalcStatus = false;
                int LevelStatusPoints = (int)(Base.InitialStatusPoints + (Level - 1) * Base.StatusPointsPerLevel),
                    Level2StatusPoints = (int)(Base.InitialStatusPoints + (Level2 - 1) * Base.StatusPointsPerLevel),
                    PlayerInvestedPoints = LevelStatusPoints;
                PlayerInvestedPoints = GetTotalPointsInvested();
                Dictionary<byte, int> PointsCount = new Dictionary<byte, int>(), PointsInvested = new Dictionary<byte, int>();
                int PointsSum = 0;
                for (byte s = 0; s < Base.Status.Count; s++)
                {
                    int Points = GetPointsInvested(s);
                    PointsSum += Points;
                    PointsCount.Add(s, Points);
                    PointsInvested.Add(s, Points);
                }
                if (PointsSum > LevelStatusPoints)
                {
                    ResetPointsInvested();
                    Main.NewText("Due to abnormal status distribution fix issue, your status points were resetted.");
                }
                if (Level2StatusPoints < PlayerInvestedPoints)
                {
                    float PointsReduction = (float)Level2StatusPoints / PlayerInvestedPoints;
                    int PointsResult = (int)(PointsSum * PointsReduction);
                    PointsSum = 0;
                    for (byte s = 0; s < Base.Status.Count; s++)
                    {
                        PointsCount[s] = (int)(Math.Round(PointsCount[s] * PointsReduction));
                        if(PointsCount[s] + PointsSum > PointsResult)
                        {
                            PointsCount[s] = PointsResult - PointsSum;
                        }
                        PointsSum += PointsCount[s];
                    }
                    if (PointsSum > 0)
                    {
                        byte[] Keys = PointsCount.OrderByDescending(x => x.Value).Select(x => x.Key).ToArray();
                        byte CurrentKey = 0;
                        while (PointsSum < PointsResult)
                        {
                            if (PointsCount[Keys[CurrentKey]] > 0) PointsCount[Keys[CurrentKey]]++;
                            PointsSum++;
                            CurrentKey++;
                            if (CurrentKey >= Keys.Length)
                                CurrentKey -= (byte)Keys.Length;
                        }
                    }
                    if(PointsSum != PointsResult)
                    {
                        Main.NewText("Status wasn't calculated correctly: " + (PointsSum - PointsResult) + " status points left.");
                    }
                }
                PointsUnderEffect = PointsCount;
                StatusPoints = LevelStatusPoints - PlayerInvestedPoints;
                Base.PlayerStatus(Level2, Level, PointsCount, PointsInvested, out SavedStatusMod);
                //Main.NewText("Recalculating status from " + LastLevel2 + " to " + Level2 + ".");
            }
            SavedStatusMod.ApplyStatus(player);
            CalculatePlayerStatusChangeBasedOnEquipment(player);
            GetRebirthStatus().Update(player, RebirthLevel);
            if (!player.dead)
            {
                if (StatusChanged)
                {
                    HealthChangePercentage = (float)player.statLifeMax2 / BeforeScaleMaxHealth;
                    ManaChangePercentage = (float)player.statManaMax2 / BeforeScaleMaxMana;
                    if (LastFrameMaxHealth != -1 && LastFrameMaxHealth != player.statLifeMax2)
                    {
                        float HealthValue = 1f;
                        if (LastFrameHealth < LastFrameMaxHealth)
                            HealthValue = (float)LastFrameHealth / LastFrameMaxHealth;
                        player.statLife = (int)(player.statLifeMax2 * HealthValue);
                    }
                }
                LastFrameHealth = player.statLife;
                LastFrameMaxHealth = player.statLifeMax2;
            }
            if (Level > MainMod.TempHighestLeveledPlayer)
                MainMod.TempHighestLeveledPlayer = Level;
        }

        public void RebirthPlayer(Player player)
        {
            RebirthLevel++;
            Main.NewText(player.name + " has been rebirth.");
            Level = 1;
            Level2 = 0;
            StatusPoints = 0;
            PointsSpent.Clear();
            PointsToSpend.Clear();
            PointsUnderEffect.Clear();
            GetRebirthStatus().Reset();
        }

        public string GetLevelText()
        {
            return Base.LevelText(this);
        }

        public void CalculatePlayerStatusChangeBasedOnEquipment(Player player)
        {
            if (!MainMod.ItemStatusCapper || !Base.AllowLevelCapping)
                return;
            Item weapon = player.inventory[player.selectedItem];
            if (weapon.damage > 0)
            {
                int DamageCap = Base.LevelDamageScale(Level2);
                if (DamageCap > 0)
                {
                    if (weapon.melee && weapon.damage > DamageCap)
                    {
                        player.meleeDamage *= (float)DamageCap / weapon.damage;
                    }
                    if (weapon.ranged && weapon.damage > DamageCap)
                    {
                        player.rangedDamage *= (float)DamageCap / weapon.damage;
                    }
                    if (weapon.magic && weapon.damage > DamageCap)
                    {
                        player.magicDamage *= (float)DamageCap / weapon.damage;
                    }
                    if (weapon.summon && weapon.damage > DamageCap)
                    {
                        player.minionDamage *= (float)DamageCap / weapon.damage;
                    }
                }
            }
            int DefenseSum = 0;
            for (int i = 0; i < 3; i++)
            {
                DefenseSum += player.armor[i].defense;
            }
            int DefenseCap = Base.LevelDefenseScale(Level2);
            if (DefenseCap > 0 && DefenseSum > DefenseCap)
            {
                player.statDefense = (int)(player.statDefense * ((float)DefenseCap / DefenseSum));
            }
        }

        public void UpdateNpc(NPC npc)
        {
            int LastLevel = Level2;
            bool GameModeChange = GameModeID != MainMod.FixedGameMode;
            if (GameModeID == "")
            {
                LastFrameHealth = npc.life;
                LastFrameMaxHealth = npc.lifeMax;
                OriginalHealth = npc.lifeMax;
                OriginalKB = npc.knockBackResist;
            }
            GameModeID = MainMod.FixedGameMode;
            if (GameModeChange)
            {
                NPC n = npc;
                if (n.realLife != -1)
                    n = Main.npc[n.realLife];
                int SpawnLevel = Base.MobSpawnLevel(n);
                if (SpawnLevel == -1 && npc.target > -1)
                {
                    int MinLevel = Main.player[npc.target].GetModPlayer<PlayerMod>().BiomeMinLv, MaxLevel = Main.player[npc.target].GetModPlayer<PlayerMod>().BiomeMaxLv;
                    SpawnLevel = Main.rand.Next(MinLevel, MaxLevel + 1);
                }
                Level = SpawnLevel;
                if(Main.netMode > 0) NetPlayMod.SyncNPCLevels(npc.whoAmI, -1, Main.myPlayer);
                IsHardmodeMonster = NpcMod.IsPreHardmodeMonster(npc);
                HealthChangePercentage = (float)OriginalHealth / npc.lifeMax;
            }
            Level2 = GetLevelScale(npc, Level);
            if (npc.type == Terraria.ID.NPCID.CultistArcherBlue || npc.type == Terraria.ID.NPCID.CultistDevote)
            {
                for (int p = 0; p < 255; p++)
                {
                    if (Main.player[p].active)
                    {
                        int PlayerLevel = Main.player[p].GetModPlayer<PlayerMod>().GetGameModeInfo.Level;
                        if(PlayerLevel < Level2)
                            Level2 = PlayerLevel;
                    }
                }
            }
            if (npc.townNPC)
                Level2 = MainMod.LastHighestLeveledNpc;
            npc.knockBackResist = OriginalKB;
            npc.lifeMax = OriginalHealth;
            ProjDamageMult = 1f;
            if (MainMod.BuffPreHardmodeEnemiesOnHardmode && !Main.expertMode && Main.hardMode && IsHardmodeMonster.HasValue && IsHardmodeMonster.Value)
            {
                bool IsABoss = false;
                if (npc.type < Terraria.ID.NPCID.Sets.TechnicallyABoss.Length && !Terraria.ID.NPCID.Sets.TechnicallyABoss[npc.type])
                    IsABoss = true;
                else if (npc.boss || (npc.realLife > -1 && Main.npc[npc.realLife].boss))
                    IsABoss = true;
                if (!IsABoss)
                {
                    if(npc.lifeMax > 5) npc.lifeMax += (Main.expertMode ? 300 : 200);
                }
                else
                {
                    if (npc.lifeMax > 5) npc.lifeMax *= (Main.expertMode ? 15 : 10);
                    //npc.damage += (Main.expertMode ? 45 : 30); //30 : 20
                }
                npc.damage += (Main.expertMode ? 45 : 30);
                npc.defense += 10;
            }
            Base.NpcStatus(npc, this);
            if (npc.type == Terraria.ID.NPCID.WallofFleshEye || npc.type == Terraria.ID.NPCID.Nailhead)
                ProjDamageMult = 1f;
            if (npc.friendly && !npc.townNPC)
            {
                npc.damage = npc.defDamage;
            }
            if (MainMod.MobDefenseConvertToHealth && !WorldMod.IsDeathMode && npc.defDefense < 1000)
            {
                int Diference = npc.defense - npc.defDefense;
                npc.lifeMax += Diference * Base.DefenseToHealthConversionValue;
                npc.defense = npc.defDefense;
            }
            if (LastFrameMaxHealth != npc.lifeMax)
            {
                float HealthValue = 1f;
                if (LastFrameHealth < LastFrameMaxHealth)
                    HealthValue = (float)LastFrameHealth / LastFrameMaxHealth;
                npc.life = (int)(npc.lifeMax * HealthValue);
            }
            if (npc.life > npc.lifeMax)
                npc.life = npc.lifeMax;
            LastFrameHealth = npc.life;
            LastFrameMaxHealth = npc.lifeMax;
            if (Level > MainMod.TempHighestLeveledNpc)
                MainMod.TempHighestLeveledNpc = Level;
        }

        public int GetLevelScale(NPC npc, int DefaultLevel)
        {
            bool CanScale = false;
            if (MainMod.Playthrough1dot5OnEndgame && NPC.downedMoonlord)
            {
                CanScale = true;
            }
            else
            {
                switch (npc.type)
                {
                    case Terraria.ID.NPCID.EyeofCthulhu:
                    case Terraria.ID.NPCID.ServantofCthulhu:
                        CanScale = MainMod.BossesAsToughasMe && NPC.downedBoss1;
                        break;
                    case Terraria.ID.NPCID.EaterofWorldsBody:
                    case Terraria.ID.NPCID.EaterofWorldsHead:
                    case Terraria.ID.NPCID.EaterofWorldsTail:
                    case Terraria.ID.NPCID.BrainofCthulhu:
                    case Terraria.ID.NPCID.Creeper:
                        CanScale = MainMod.BossesAsToughasMe && NPC.downedBoss2;
                        break;
                    case Terraria.ID.NPCID.SkeletronHead:
                    case Terraria.ID.NPCID.SkeletronHand:
                        CanScale = MainMod.BossesAsToughasMe && NPC.downedBoss3;
                        break;
                    case Terraria.ID.NPCID.KingSlime:
                    case Terraria.ID.NPCID.SlimeSpiked:
                        CanScale = MainMod.BossesAsToughasMe && NPC.downedSlimeKing;
                        break;
                    case Terraria.ID.NPCID.QueenBee:
                        CanScale = MainMod.BossesAsToughasMe && NPC.downedQueenBee;
                        break;
                    case Terraria.ID.NPCID.WallofFlesh:
                    case Terraria.ID.NPCID.WallofFleshEye:
                    case Terraria.ID.NPCID.TheHungry:
                    case Terraria.ID.NPCID.TheHungryII:
                    case Terraria.ID.NPCID.LeechBody:
                    case Terraria.ID.NPCID.LeechHead:
                    case Terraria.ID.NPCID.LeechTail:
                        CanScale = MainMod.BossesAsToughasMe && Main.hardMode;
                        break;
                    case Terraria.ID.NPCID.TheDestroyer:
                    case Terraria.ID.NPCID.TheDestroyerBody:
                    case Terraria.ID.NPCID.TheDestroyerTail:
                        CanScale = MainMod.BossesAsToughasMe && NPC.downedMechBoss1;
                        break;
                    case Terraria.ID.NPCID.Spazmatism:
                    case Terraria.ID.NPCID.Retinazer:
                        CanScale = MainMod.BossesAsToughasMe && NPC.downedMechBoss2;
                        break;
                    case Terraria.ID.NPCID.SkeletronPrime:
                    case Terraria.ID.NPCID.PrimeCannon:
                    case Terraria.ID.NPCID.PrimeLaser:
                    case Terraria.ID.NPCID.PrimeSaw:
                    case Terraria.ID.NPCID.PrimeVice:
                        CanScale = MainMod.BossesAsToughasMe && NPC.downedMechBoss3;
                        break;
                    case Terraria.ID.NPCID.Plantera:
                    case Terraria.ID.NPCID.PlanterasHook:
                    case Terraria.ID.NPCID.PlanterasTentacle:
                        CanScale = MainMod.BossesAsToughasMe && NPC.downedPlantBoss;
                        break;
                    case Terraria.ID.NPCID.Golem:
                    case Terraria.ID.NPCID.GolemFistLeft:
                    case Terraria.ID.NPCID.GolemFistRight:
                    case Terraria.ID.NPCID.GolemHead:
                    case Terraria.ID.NPCID.GolemHeadFree:
                        CanScale = MainMod.BossesAsToughasMe && NPC.downedGolemBoss;
                        break;
                    case Terraria.ID.NPCID.DukeFishron:
                    case Terraria.ID.NPCID.Sharkron:
                    case Terraria.ID.NPCID.Sharkron2:
                        CanScale = MainMod.BossesAsToughasMe && NPC.downedFishron;
                        break;
                    case Terraria.ID.NPCID.AncientCultistSquidhead:
                    case Terraria.ID.NPCID.CultistBoss:
                    case Terraria.ID.NPCID.CultistBossClone:
                    case Terraria.ID.NPCID.CultistDragonBody1:
                    case Terraria.ID.NPCID.CultistDragonBody2:
                    case Terraria.ID.NPCID.CultistDragonBody3:
                    case Terraria.ID.NPCID.CultistDragonBody4:
                    case Terraria.ID.NPCID.CultistDragonHead:
                    case Terraria.ID.NPCID.CultistDragonTail:
                        CanScale = MainMod.BossesAsToughasMe && NPC.downedAncientCultist;
                        break;
                    case 379: //Cultist Archer
                        if (npc.type == 379)
                        {
                            if (npc.ai[3] < 0)
                            {
                                CanScale = true;
                            }
                            else
                            {
                                return MainMod.LastHighestLeveledNpc;
                            }

                        }
                        else
                        {
                            CanScale = true;
                        }
                        break;
                    case 380:
                    //case 437:
                    case 438:
                        {
                            if (npc.type == 438 || npc.type == 379)
                            {
                                if (npc.ai[1] == 1)
                                {
                                    CanScale = true;
                                }
                                else
                                {
                                    return MainMod.LastHighestLeveledNpc;
                                }

                            }
                            else
                            {
                                CanScale = true;
                            }
                        }
                        break;
                    case Terraria.ID.NPCID.MoonLordCore:
                    case Terraria.ID.NPCID.MoonLordFreeEye:
                    case Terraria.ID.NPCID.MoonLordHand:
                    case Terraria.ID.NPCID.MoonLordHead:
                    case Terraria.ID.NPCID.MoonLordLeechBlob:
                        CanScale = MainMod.BossesAsToughasMe && NPC.downedMoonlord;
                        break;
                }
            }
            if (!CanScale)
                return DefaultLevel;
            int NewLevel = MainMod.LastHighestLeveledPlayer;
            return NewLevel > DefaultLevel ? NewLevel : DefaultLevel;
        }

        public void Save(Terraria.ModLoader.IO.TagCompound tag, string Key)
        {
            tag.Add("Game_Level_" + Key, Level);
            tag.Add("Game_Exp_" + Key, Exp);
            int StatusPointsSpent = PointsSpent.Keys.Count;
            tag.Add("Game_PointsSaved_"+Key, StatusPointsSpent);
            int Count = 0;
            foreach (byte key in PointsSpent.Keys.ToArray())
            {
                tag.Add("Game_PointKey_" + Count + "_" + Key, key);
                tag.Add("Game_PointCount_" + Count + "_" + Key, PointsSpent[key]);
                Count++;
            }
        }

        public void Load(Terraria.ModLoader.IO.TagCompound tag, string Key, int ModVersion)
        {
            Level = tag.GetInt("Game_Level_" + Key);
            Exp = tag.GetInt("Game_Exp_" + Key);
            int StatusPointsSpent = tag.GetInt("Game_PointsSaved_" + Key);
            int Count = 0;
            while (Count < StatusPointsSpent)
            {
                byte StatusKey = tag.GetByte("Game_PointKey_" + Count + "_" + Key);
                int StatusCount = tag.GetInt("Game_PointCount_" + Count + "_" + Key);
                PointsSpent.Add(StatusKey, StatusCount);
                Count++;
            }
            if(ModVersion < 1)
            {
                Level = (int)(Level * 0.55f);
                if (Level < 1)
                    Level = 1;
                MaxExp = GetMaxExp();
                Exp = 0;
                ResetPointsInvested();
            }
            if(ModVersion < 3)
            {
                if(GameModeID == GameModes.RegularRPG.HardcoreRpgModeID || GameModeID == GameModes.RegularRPG.RegularRpgModeID ||
                    GameModeID == GameModes.RegularRPG.SoftcoreRpgModeID)
                {
                    if(Level > 75)
                    {
                        Level = 75;
                        ResetPointsInvested();
                    }
                }
            }
        }
    }
}
