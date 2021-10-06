using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using Terraria.ModLoader;

namespace NExperience
{
    public class WorldMod : ModWorld
    {
        public static bool IsDeathMode = false;
        public static bool InGraveyardRegion = false;
        public static string WorldGameMode = "";
        public static int WorldRebirthLevel = 0, LastRebirthLevelCalc = 0;
        private static float EnemyHealth = 1f, EnemyDamage = 1f, EnemyDefense = 1f, EnemyKnockbackRes = 1f, EnemyCoins = 1f;

        public static float EnemyHealthBonus { get { return EnemyHealth * 0.1f; } }
        public static float EnemyDamageBonus { get { return EnemyDamage * 0.1f; } }
        public static float EnemyDefenseBonus { get { return EnemyDefense * 0.1f; } }

        public static void ResetLevelCalc()
        {
            LastRebirthLevelCalc = 0;
            EnemyHealth = 1f;
            EnemyDamage = 1f;
            EnemyDefense = 1f;
            EnemyKnockbackRes = 1f;
            EnemyCoins = 1f;
        }

        public static void RebirthWorld()
        {
            WorldRebirthLevel++;
            NPC.downedAncientCultist = NPC.downedBoss1 = NPC.downedBoss2 = NPC.downedBoss3 = NPC.downedChristmasIceQueen = NPC.downedChristmasSantank = NPC.downedChristmasTree =
                NPC.downedClown = NPC.downedFishron = NPC.downedFrost = NPC.downedGoblins = NPC.downedGolemBoss = NPC.downedHalloweenKing = NPC.downedHalloweenTree =
                NPC.downedMartians = NPC.downedMechBoss1 = NPC.downedMechBoss2 = NPC.downedMechBoss3 = NPC.downedMechBossAny = NPC.downedMoonlord = NPC.downedPirates =
                NPC.downedPlantBoss = NPC.downedQueenBee = NPC.downedSlimeKing = NPC.downedTowerNebula = NPC.downedTowerSolar = NPC.downedTowerStardust = NPC.downedTowerVortex =
                Main.hardMode = false;
            Main.NewText("The World has been Rebirth.");
        }
        
        public static void UpdateWorldMobRebirthStatus()
        {
            if (LastRebirthLevelCalc < WorldRebirthLevel)
            {
                LastRebirthLevelCalc++;
                EnemyHealth *= 1.225f;
                EnemyDamage *= 1.225f;
                EnemyDefense *= 1.225f;
                EnemyKnockbackRes += 0.05f;
                EnemyCoins += 0.1f;
            }
        }

        public override void Initialize()
        {
            if (WorldGameMode == "" || MainMod.GetGameMode(WorldGameMode) == null)
            {
                WorldGameMode = MainMod.GetGameModeIDs[0];
            }
            TileMod.PlayerPlacedTiles.Clear();
            MainMod.WarnGameModeChange = true;
            //IsDeathMode = false;
            ResetLevelCalc();
        }

        public override void PreUpdate()
        {
            UpdateWorldMobRebirthStatus();
        }

        public override void TileCountsAvailable(int[] tileCounts)
        {
            InGraveyardRegion = tileCounts[Terraria.ID.TileID.Tombstones] * 4 >= 48;
        }

        public override Terraria.ModLoader.IO.TagCompound Save()
        {
            Terraria.ModLoader.IO.TagCompound tag = new Terraria.ModLoader.IO.TagCompound();
            tag.Add("WorldVersion", MainMod.ModVersion);
            tag.Add("GameModeID", WorldGameMode);
            tag.Add("DeathMode", IsDeathMode);
            return tag;
        }

        public override void Load(Terraria.ModLoader.IO.TagCompound tag)
        {
            if (!tag.ContainsKey("WorldVersion"))
            {
                return;
            }
            int Version = tag.GetInt("WorldVersion");
            WorldGameMode = tag.GetString("GameModeID");
            if(Version > 1)
                IsDeathMode = tag.GetBool("DeathMode");
        }
    }
}
