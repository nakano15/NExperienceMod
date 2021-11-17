using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace NExperience
{
    public class PlayerMod : ModPlayer
    {
        private Dictionary<string, GameModeData> GameModes = new Dictionary<string, GameModeData>();
        public int BiomeMinLv = 1, BiomeMaxLv = 2;
        private int LastHealthRegenValue = 0;
        public int EventScoreCounter = 0;
        public byte EventType = 0;
        public BitsByte biome = new BitsByte();
        public GameModeData GetGameModeInfo
        {
            get
            {
                return GetGameModeData(MainMod.FixedGameMode);
            }
        }
        public float ExpBonus = 0;
        public float Luck = 0, CriticalDamageBonusMult = 0f;
        public float NeutralDamage = 1f;
        public float DodgeRate = 0;
        public float KbMult = 1f, KbSum = 0;
        private int LastLoggedLevel = -1;
        public uint ClayPotMagicFindPoints = 0,
            AltarMagicFindPoints = 0,
            OrbMagicFindPoints = 0,
            LifeCrystalMagicPoints = 0,
            LifeFruitMagicPoints = 0;
        private bool Only1HPMode = false;
        public bool Is1HPMode { get { return Only1HPMode; } }

        public bool ZoneGraveyard { get { return biome[0]; } set { biome[0] = value; } }
        public bool ZoneDeep { get { return biome[1]; } set { biome[1] = value; } }

        public void Set1HPMode(bool Enable)
        {
            if(Enable != Only1HPMode)
            {
                if (Enable)
                {
                    player.statLifeMax2 = 1;
                }
                else
                {
                    player.statLife = 1;
                    int SicknessTime = 30 * 60 - player.potionDelay;
                    if(SicknessTime > 0)
                    {
                        player.potionDelay = 30 * 60;
                        player.AddBuff(21, player.potionDelay);
                    }
                }
                Only1HPMode = Enable;
            }
        }

        public GameModeData GetGameModeData(string GameModeID)
        {
            if (!GameModes.ContainsKey(GameModeID))
            {
                GameModeData gamemode = new GameModeData();
                gamemode.GameModeID = GameModeID;
                GameModes.Add(GameModeID, gamemode);
            }
            return GameModes[GameModeID];
        }

        public override bool CloneNewInstances
        {
            get
            {
                return false;
            }
        }
        
        public PlayerMod()
        {
            GameModes = new Dictionary<string, GameModeData>();
        }

        public bool HasGameModeData(string ID)
        {
            return GameModes.ContainsKey(ID);
        }

        public static int GetPlayerLevel(Player player, bool EffectiveLevel)
        {
            PlayerMod p = player.GetModPlayer<PlayerMod>();
            if (EffectiveLevel)
                return p.GetGameModeInfo.Level2;
            return p.GetGameModeInfo.Level;
        }

        public override void OnEnterWorld(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                if (Main.netMode == 0)
                {
                    if (MainMod.WarnGameModeChange)
                    {
                        MainMod.WarnGameModeChange = false;
                        Main.NewText("Game Mode set for the world: " + MainMod.GetGameModeNameByID(WorldMod.WorldGameMode));
                    }
                }
                else if(Main.netMode == 1)
                {
                    NetPlayMod.AskForGameMode(-1);
                }
            }
        }

        public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
        {
            NetPlayMod.SendPlayerLevel(player.whoAmI, -1, player.whoAmI);
            NetPlayMod.SendPlayerStatus(player.whoAmI, -1, player.whoAmI);
        }

        public override void SendClientChanges(ModPlayer clientPlayer)
        {
            if(LastLoggedLevel != GetGameModeInfo.Level)
            {
                NetPlayMod.SendPlayerLevel(player.whoAmI, -1, player.whoAmI);
            }
            LastLoggedLevel = GetGameModeInfo.Level;
        }

        public static bool IsPlayerInInvasionPosition(Player player)
        {
            if(Main.invasionType > 0 && Main.invasionDelay == 0 && Main.invasionSize > 0&& player.ZoneOverworldHeight)
            {
                bool InInvasionPlace = false;
                const int Distance = 5000;
                if (Math.Abs(player.position.X - Main.invasionX * 16) < Distance)
                    return true;
                if(Main.invasionX >= Main.maxTilesX / 2 - 5 && Main.invasionX <= Main.maxTilesX / 2 + 5)
                {
                    for(int i = 0; i < 200; i++)
                    {
                        if (Main.npc[i].active && Main.npc[i].townNPC && Math.Abs(player.position.X - Main.npc[i].Center.X) < Distance)
                            return true;
                    }
                }
            }
            return false;
        }

        public override void ResetEffects()
        {
            ExpBonus = 0f;
            Luck = ClayPotMagicFindPoints * 0.1f + AltarMagicFindPoints * 0.3f + OrbMagicFindPoints * 0.2f + LifeCrystalMagicPoints * 0.25f + LifeFruitMagicPoints * 0.05f;
            CriticalDamageBonusMult = 0f;
            NeutralDamage = 1f;
            DodgeRate = 0;
            KbMult = 1f;
            KbSum = 0;
        }

        public override void PostUpdateMiscEffects()
        {
            if (player.HasBuff(Terraria.ID.BuffID.WellFed))
                ExpBonus += 0.1f;
        }

        public override void UpdateBiomes()
        {
            ZoneGraveyard = WorldMod.InGraveyardRegion && !Main.dayTime && player.position.Y <= Main.worldSurface * 16;
            ZoneDeep = player.position.Y > Main.rockLayer * 24;
        }

        public override void UpdateLifeRegen()
        {
            if (player.whoAmI != Main.myPlayer || GetGameModeInfo.HealthChangePercentage <= 1 || player.statLife >= player.statLifeMax2 || player.lifeRegenCount < 0)
                return;
            if (player.lifeRegenCount >= 0)
            {
                if (player.lifeRegenCount < 60 && LastHealthRegenValue >= 60)
                {
                    player.statLife += (int)GetGameModeInfo.HealthChangePercentage - 1;
                }
                LastHealthRegenValue = player.lifeRegenCount;
            }
        }
        
        public override void UpdateBadLifeRegen()
        {
            if (player.lifeRegenCount == 0 && LastHealthRegenValue < 0)
            {
                int LifeChangeValue = (int)(GetGameModeInfo.HealthChangePercentage - 1);
                if (LifeChangeValue > 0)
                {
                    player.statLife -= LifeChangeValue;
                }
                LastHealthRegenValue = player.lifeRegenCount;
            }
            if(player.lifeRegenCount < 0)
                LastHealthRegenValue = player.lifeRegenCount;
        }

        public override bool PreHurt(bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref Terraria.DataStructures.PlayerDeathReason damageSource)
        {
            if(MainMod.LuckStrike(Luck, 3000))
            {
                CombatText.NewText(player.getRect(), Color.Green, "Luck", true);
                player.immuneTime = 30 * 60;
                MainMod.TriggerLuckyClovers(player.Center, false);
                return false;
            }
            if(Main.rand.NextDouble() * 100 < DodgeRate)
            {
                CombatText.NewText(player.getRect(), Color.Gray, "Dodge", true);
                player.immuneTime = 30 * 60;
                return false;
            }
            bool DealDamage = true;
            if (damageSource.SourceOtherIndex >= 0 && (damageSource.SourceOtherIndex != 4 || damage != player.statLife / 2))
            {
                damage = (int)(damage * GetGameModeInfo.HealthChangePercentage);
            }
            return DealDamage;
        }

        public override void OnHitNPC(Item item, NPC target, int damage, float knockback, bool crit)
        {
            int AttackExp = GetGameModeInfo.Base.AttackExp(GetGameModeInfo, damage, crit, target.GetGlobalNPC<NpcMod>().NpcStatus.Level);
            if(AttackExp > 0)
            {
                GetExp(AttackExp, ExpReceivedPopText.ExpSource.AttackExp);
            }
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, int damage, float knockback, bool crit)
        {
            int AttackExp = GetGameModeInfo.Base.AttackExp(GetGameModeInfo, damage, crit, target.GetGlobalNPC<NpcMod>().NpcStatus.Level);
            if (AttackExp > 0)
            {
                GetExp(AttackExp, ExpReceivedPopText.ExpSource.AttackExp);
            }
        }

        public void GetExp(int Value, bool ShowTooltip = false, Rectangle? expshowpos = null)
        {
            GetExp(Value, ExpReceivedPopText.ExpSource.Other, ShowTooltip, expshowpos);
        }

        public void GetExp(int Value, ExpReceivedPopText.ExpSource Source, bool ShowTooltip = false, Rectangle? expshowpos = null)
        {
            float ExpMult = (Value >= 0 ? MainMod.ExpRate + ExpBonus : 1f);
            if (Main.netMode < 2 && player.whoAmI == Main.myPlayer)
            {
                float PenaltyPercent = MainMod.AfkPenaltyDecimal;
                if (PenaltyPercent > 0)
                    ExpMult -= ExpMult * PenaltyPercent;
                if (ShowTooltip)
                {
                    string ExpBonus = "";
                    if (expshowpos == null)
                        expshowpos = player.getRect();
                    bool Penalty = Value < 0;
                    if (ExpMult != 1)
                    {
                        if (ExpMult > 1)
                            ExpBonus = " (" + ((int)((ExpMult - 1f) * 100)) + "% Bonus)";
                        else
                        {
                            ExpBonus = " (" + ((int)((ExpMult - 1f) * 100)) + "% Penalty)";
                            Penalty = !Penalty;
                        }
                    }
                    if (MainMod.ShowExpAsPercentage)
                    {
                        float ExpValue = (float)Math.Round((float)Value / GetGameModeInfo.MaxExp * 100, 2);
                        if (ExpValue == 0)
                        {
                            CombatText.NewText(expshowpos.Value, (Penalty ? Color.Red : Color.Cyan), "< 0.01% Exp" + ExpBonus, true);
                        }
                        else
                        {
                            CombatText.NewText(expshowpos.Value, (Penalty ? Color.Red : Color.Cyan), ExpValue + "% Exp" + ExpBonus, true);
                        }
                    }
                    else
                    {
                        CombatText.NewText(expshowpos.Value, (Penalty ? Color.Red : Color.Cyan), Value + " Exp" + ExpBonus, true);
                    }
                }
            }
            //if (Main.netMode < 2)
            {
                if (player.whoAmI != Main.myPlayer || Main.netMode == 2)
                    NetPlayMod.SendExpToPlayer(player.whoAmI, Value, Source, Main.myPlayer);
                else
                {
                    try
                    {
                        int Exp = checked((int)(Value * ExpMult));
                        if (player.whoAmI == Main.myPlayer && Main.netMode < 2)
                        {
                            MainMod.UpdateExpReceivedPopText(Source, Exp, this);
                        }
                        GetGameModeInfo.ChangeExp(Exp, player);
                    }
                    catch
                    {
                        GetGameModeInfo.ChangeExp((Value > 0 ? int.MaxValue : int.MinValue), player);
                    }
                }
            }
        }

        public int GetExpReward(float Level, float Percentage, bool ShowTooltip = true)
        {
            return GetExpReward(Level, Percentage, ExpReceivedPopText.ExpSource.Other, ShowTooltip);
        }

            /// <summary>
            /// Gives an exp reward to the player, based on a few factors.
            /// </summary>
            /// <param name="Level">From 0 to 100, determines the level which the player will get exp reward related. 0 = Early level reward, 100 = Max level reward</param>
            /// <param name="Percentage">The pecentage of exp the player will get from this.</param>
        public int GetExpReward(float Level, float Percentage, ExpReceivedPopText.ExpSource source, bool ShowTooltip = true)
        {
            GameModeData gmd = GetGameModeInfo;
            int ExpReward = gmd.Base.GetExpReward(Level, Percentage, source, gmd);
            if (ExpReward < 1)
                ExpReward = 1;
            GetExp(ExpReward, source, ShowTooltip);
            return ExpReward;
        }

        public static int[] GetPlayerTeamMates(Player player, float DistanceX = 800, float DistanceY = 600)
        {
            List<int> Players = new List<int>();
            if (player.whoAmI < 255)
            {
                if (!player.dead)
                    Players.Add(player.whoAmI);
                for (int i = 0; i < 255; i++)
                {
                    if (i != player.whoAmI && Main.player[i].active && !Main.player[i].dead &&
                        ((player.team == 0 && Main.player[i].team == 0 && !player.hostile && !Main.player[i].hostile) ||
                        player.team == Main.player[i].team) &&
                        Math.Abs(Main.player[i].Center.X - player.Center.X) < DistanceX && Math.Abs(Main.player[i].Center.Y - player.Center.Y) < DistanceY)
                    {
                        Players.Add(i);
                    }
                }
            }
            return Players.ToArray();
        }

        public override void PostUpdate()
        {

        }

        public override void GetHealLife(Item item, bool quickHeal, ref int healValue)
        {
            healValue = (int)(healValue * GetGameModeInfo.HealthChangePercentage);
        }

        public override void GetHealMana(Item item, bool quickHeal, ref int healValue)
        {
            healValue = (int)(healValue * GetGameModeInfo.ManaChangePercentage);
        }

        public override void UpdateEquips(ref bool wallSpeedBuff, ref bool tileSpeedBuff, ref bool tileRangeBuff)
        {
            if(player.armor[0].type == Terraria.ID.ItemID.PumpkinHelmet && player.armor[1].type == Terraria.ID.ItemID.PumpkinBreastplate && 
                player.armor[2].type == Terraria.ID.ItemID.PumpkinLeggings)
            {
                NeutralDamage += 0.1f;
            }
            switch (player.armor[0].type)
            {
                case Terraria.ID.ItemID.CrimsonHelmet:
                    NeutralDamage += 0.02f;
                    break;
            }
            switch (player.armor[1].type)
            {
                case Terraria.ID.ItemID.CrimsonScalemail:
                    NeutralDamage += 0.02f;
                    break;
                case Terraria.ID.ItemID.PalladiumBreastplate:
                    NeutralDamage += 0.03f;
                    break;
                case Terraria.ID.ItemID.MythrilChainmail:
                    NeutralDamage += 0.07f;
                    break;
                case Terraria.ID.ItemID.AdamantiteBreastplate:
                    NeutralDamage += 0.08f;
                    break;
                case Terraria.ID.ItemID.TitaniumBreastplate:
                    NeutralDamage += 0.04f;
                    break;
                case Terraria.ID.ItemID.ChlorophytePlateMail:
                    NeutralDamage += 0.05f;
                    break;
                case Terraria.ID.ItemID.Gi:
                    NeutralDamage += 0.05f;
                    break;
            }
            switch (player.armor[2].type)
            {
                case Terraria.ID.ItemID.CrimsonGreaves:
                    NeutralDamage += 0.02f;
                    break;
                case Terraria.ID.ItemID.CobaltLeggings:
                    NeutralDamage += 0.03f;
                    break;
                case Terraria.ID.ItemID.PalladiumLeggings:
                    NeutralDamage += 0.02f;
                    break;
                case Terraria.ID.ItemID.TitaniumLeggings:
                    NeutralDamage += 0.03f;
                    break;
                case Terraria.ID.ItemID.HallowedGreaves:
                    NeutralDamage += 0.07f;
                    break;
            }
            for (int i = 3; i < 9; i++)
            {
                if (i == 8 && (!player.extraAccessory || !Main.expertMode))
                {
                    continue;
                }
                if(player.armor[i].type > 0)
                {
                    switch (player.armor[i].prefix)
                    {
                        case Terraria.ID.PrefixID.Menacing:
                            NeutralDamage += 0.04f;
                            break;
                        case Terraria.ID.PrefixID.Angry:
                            NeutralDamage += 0.03f;
                            break;
                        case Terraria.ID.PrefixID.Spiked:
                            NeutralDamage += 0.02f;
                            break;
                        case Terraria.ID.PrefixID.Jagged:
                            NeutralDamage += 0.01f;
                            break;
                    }
                    switch (player.armor[i].type)
                    {
                        case Terraria.ID.ItemID.AvengerEmblem:
                            NeutralDamage += 0.12f;
                            break;
                        case Terraria.ID.ItemID.CelestialStone:
                        case Terraria.ID.ItemID.CelestialShell:
                        case Terraria.ID.ItemID.DestroyerEmblem:
                            NeutralDamage += 0.1f;
                            break;
                        case Terraria.ID.ItemID.SunStone:
                            if (Main.dayTime) NeutralDamage += 0.1f;
                            break;
                        case Terraria.ID.ItemID.MoonStone:
                            if (!Main.dayTime) NeutralDamage += 0.1f;
                            break;
                        case Terraria.ID.ItemID.PutridScent:
                            NeutralDamage += 0.05f;
                            break;
                    }
                }
            }
        }

        public override void PostUpdateBuffs()
        {
            for(int b = 0; b < player.buffType.Length; b++)
            {
                if(player.buffType[b] > -1 && player.buffTime[b] > 0)
                {
                    switch (player.buffType[b])
                    {
                        case Terraria.ID.BuffID.NebulaUpDmg1:
                            NeutralDamage += 0.15f;
                            break;
                        case Terraria.ID.BuffID.NebulaUpDmg2:
                            NeutralDamage += 0.3f;
                            break;
                        case Terraria.ID.BuffID.NebulaUpDmg3:
                            NeutralDamage += 0.45f;
                            break;
                    }
                }
            }
        }

        public override void PostUpdateEquips()
        {
            GetGameModeInfo.UpdatePlayer(player);
            if (Only1HPMode)
            {
                player.statLifeMax2 = 1;
                if (player.statLife > player.statLifeMax2)
                    player.statLife = player.statLifeMax2;
            }
        }

        public override void ModifyHitNPC(Item item, NPC target, ref int damage, ref float knockback, ref bool crit)
        {
            if (crit)
            {
                damage += (int)(damage * CriticalDamageBonusMult);
            }
            knockback = (knockback + KbSum) * KbMult;
        }

        public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            if (crit)
            {
                damage += GetCriticalDamageBonus(damage);
            }
            knockback = (knockback + KbSum) * KbMult;
        }

        public override void ModifyHitPvp(Item item, Player target, ref int damage, ref bool crit)
        {
            if (crit)
            {
                damage += GetCriticalDamageBonus(damage);
            }
        }

        public override void ModifyHitPvpWithProj(Projectile proj, Player target, ref int damage, ref bool crit)
        {
            if (crit)
            {
                damage += GetCriticalDamageBonus(damage);
            }
        }

        public int GetCriticalDamageBonus(int RealDamage)
        {
            return (int)(RealDamage * CriticalDamageBonusMult);
        }

        public override void ModifyHitByProjectile(Projectile proj, ref int damage, ref bool crit)
        {
            if (proj.type == Terraria.ID.ProjectileID.Boulder || proj.type == Terraria.ID.ProjectileID.PoisonDartTrap || proj.type == Terraria.ID.ProjectileID.FlamesTrap ||
                proj.type == Terraria.ID.ProjectileID.FlamethrowerTrap || proj.type == Terraria.ID.ProjectileID.SpikyBallTrap || proj.type == Terraria.ID.ProjectileID.SpearTrap ||
                proj.type == Terraria.ID.ProjectileID.SandBallFalling || proj.type == Terraria.ID.ProjectileID.PearlSandBallFalling || proj.type == Terraria.ID.ProjectileID.MudBall ||
                proj.type == Terraria.ID.ProjectileID.AshBallFalling || proj.type == Terraria.ID.ProjectileID.CrimsandBallFalling || proj.type == Terraria.ID.ProjectileID.EbonsandBallFalling ||
                proj.type == Terraria.ID.ProjectileID.SiltBall || proj.type == Terraria.ID.ProjectileID.SlushBall)
            {
                damage = (int)(damage * GetGameModeInfo.HealthChangePercentage);
            }
        }

        public override void Kill(double damage, int hitDirection, bool pvp, Terraria.DataStructures.PlayerDeathReason damageSource)
        {
            GetGameModeInfo.SetSpawnHealthValue(player);
            if(MainMod.DeathExpPenalty > 0)
            {
                GetExp((int)(-GetGameModeInfo.MaxExp * (MainMod.DeathExpPenalty * 0.01f)), true);
            }
        }
        
        public override Terraria.ModLoader.IO.TagCompound Save()
        {
            Terraria.ModLoader.IO.TagCompound tag = new Terraria.ModLoader.IO.TagCompound();
            int GameModeCount = GameModes.Count;
            tag.Add("GameModeCount", GameModeCount);
            tag.Add("ModVersion", MainMod.ModVersion);
            tag.Add("ClayPotMagicFind", (int)ClayPotMagicFindPoints);
            tag.Add("AltarMagicFind", (int)AltarMagicFindPoints);
            tag.Add("ShadowOrbMagicFind", (int)OrbMagicFindPoints);
            tag.Add("LifeCrystalMagicFind", (int)LifeCrystalMagicPoints);
            tag.Add("LifeFruitMagicFind", (int)LifeFruitMagicPoints);
            tag.Add("1HPMode", Only1HPMode);
            int Count = 0;
            foreach (string g in GameModes.Keys.ToArray())
            {
                tag.Add("GameModeKey_" + Count, g);
                GameModes[g].Save(tag, g);
                Count++;
            }
            return tag;
        }

        public override void Load(Terraria.ModLoader.IO.TagCompound tag)
        {
            GameModes = new Dictionary<string, GameModeData>();
            if (!tag.ContainsKey("GameModeCount")) return;
            int GameModeCount = tag.GetInt("GameModeCount");
            int ModVersion = tag.GetInt("ModVersion");
            if (ModVersion >= 4)
            {
                ClayPotMagicFindPoints = (uint)tag.GetInt("ClayPotMagicFind");
                AltarMagicFindPoints = (uint)tag.GetInt("AltarMagicFind");
                OrbMagicFindPoints = (uint)tag.GetInt("ShadowOrbMagicFind");
                LifeCrystalMagicPoints = (uint)tag.GetInt("LifeCrystalMagicFind");
                LifeFruitMagicPoints = (uint)tag.GetInt("LifeFruitMagicFind");
            }
            if(ModVersion >= 5)
            {
                Only1HPMode = tag.GetBool("1HPMode");
            }
            for (int g = 0; g < GameModeCount; g++)
            {
                string GameModeKey = tag.GetString("GameModeKey_" + g);
                GameModeData gameMode = new GameModeData();
                gameMode.GameModeID = GameModeKey;
                gameMode.Load(tag, GameModeKey, ModVersion);
                GameModes.Add(GameModeKey, gameMode);
            }
        }

        public override void ModifyDrawLayers(List<PlayerLayer> layers)
        {

        }
    }
}
