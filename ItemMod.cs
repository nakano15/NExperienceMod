using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using Terraria.ModLoader;

namespace NExperience
{
    public class ItemMod : GlobalItem
    {
        public override bool InstancePerEntity => base.InstancePerEntity;
        //public List<ItemStatusMod> StatusMods = new List<ItemStatusMod>();

        public override bool OnPickup(Item item, Player player)
        {
            if (player.statLife < player.statLifeMax2 && (item.type == Terraria.ID.ItemID.Heart || item.type == Terraria.ID.ItemID.CandyApple || item.type == Terraria.ID.ItemID.CandyCane))
            {
                int HealthChange = (int)(player.GetModPlayer<PlayerMod>().GetGameModeInfo.HealthChangePercentage * 20) - 20;
                float LuckFactor = player.GetModPlayer<PlayerMod>().Luck;
                if (MainMod.LuckStrike(LuckFactor, 60000))
                {
                    HealthChange += 300;
                    CombatText.NewText(player.getRect(), Microsoft.Xna.Framework.Color.Green, "Very Lucky!", true);
                    MainMod.TriggerLuckyClovers(player.Center, true);
                }
                else if (MainMod.LuckStrike(LuckFactor, 3000))
                {
                    HealthChange += 80;
                    CombatText.NewText(player.getRect(), Microsoft.Xna.Framework.Color.Green, "Lucky!", true);
                    MainMod.TriggerLuckyClovers(player.Center, false);
                }
                if (HealthChange > 0)
                {
                    player.statLife += HealthChange;
                    if (player.statLife > player.statLifeMax2)
                        player.statLife = player.statLifeMax2;
                    CombatText.NewText(player.getRect(), CombatText.HealLife, "+" + HealthChange);
                }
            }
            if (player.statMana < player.statManaMax2 && (item.type == Terraria.ID.ItemID.Star || item.type == Terraria.ID.ItemID.SoulCake || item.type == Terraria.ID.ItemID.SugarPlum))
            {
                int ManaChange = (int)(player.GetModPlayer<PlayerMod>().GetGameModeInfo.ManaChangePercentage * 100) - 100;
                if (ManaChange > 0)
                {
                    player.statMana += ManaChange;
                    if (player.statMana > player.statManaMax2)
                        player.statMana = player.statManaMax2;
                    CombatText.NewText(player.getRect(), CombatText.HealMana, "+" + ManaChange);
                }
            }
            return true;
        }

        public override void ExtractinatorUse(int extractType, ref int resultType, ref int resultStack)
        {
            foreach (int p in PlayerMod.GetPlayerTeamMates(Main.player[Main.myPlayer]))
            {
                Main.player[p].GetModPlayer<PlayerMod>().GetExpReward(5, 0.005f * resultStack, ExpReceivedPopText.ExpSource.Extractinator);
            }
        }

        public override void OnCraft(Item item, Recipe recipe)
        {
            float ValueStack = item.value * item.stack;
            foreach(Item i in recipe.requiredItem)
            {
                ValueStack += (i.value * i.stack) / 2;
            }
            if (ValueStack > 0 && (item.type < Terraria.ID.ItemID.CopperCoin || item.type > Terraria.ID.ItemID.PlatinumCoin))
            {
                foreach (int p in PlayerMod.GetPlayerTeamMates(Main.player[Main.myPlayer]))
                {
                    Main.player[p].GetModPlayer<PlayerMod>().GetExpReward(5, 0.05f * (ValueStack / 1000000), ExpReceivedPopText.ExpSource.Crafting);
                }
            }
        }

        public override void AddRecipes()
        {
            //T1
            GetBarTransmutationResult(Terraria.ID.ItemID.CobaltBar, Terraria.ID.ItemID.PalladiumBar, mod).AddRecipe();
            GetBarTransmutationResult(Terraria.ID.ItemID.PalladiumBar, Terraria.ID.ItemID.CobaltBar, mod).AddRecipe();
            //T2
            GetBarTransmutationResult(Terraria.ID.ItemID.MythrilBar, Terraria.ID.ItemID.OrichalcumBar, mod).AddRecipe();
            GetBarTransmutationResult(Terraria.ID.ItemID.OrichalcumBar, Terraria.ID.ItemID.MythrilBar, mod).AddRecipe();
            //T3
            GetBarTransmutationResult(Terraria.ID.ItemID.AdamantiteBar, Terraria.ID.ItemID.TitaniumBar, mod).AddRecipe();
            GetBarTransmutationResult(Terraria.ID.ItemID.TitaniumBar, Terraria.ID.ItemID.AdamantiteBar, mod).AddRecipe();
        }

        private ModRecipe GetBarTransmutationResult(int ResultItem, int RequiredItem, Mod mod)
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.SetResult(ResultItem);
            recipe.requiredItem[0].SetDefaults(RequiredItem);
            recipe.requiredItem[0].stack = 2;
            recipe.requiredItem[1].SetDefaults(Terraria.ID.ItemID.PixieDust);
            recipe.requiredTile[0] = Terraria.ID.TileID.AlchemyTable;
            return recipe;
        }

        public override void CaughtFishStack(int type, ref int stack)
        {
            foreach (int p in PlayerMod.GetPlayerTeamMates(Main.player[Main.myPlayer]))
            {
                PlayerMod pm = Main.player[p].GetModPlayer<PlayerMod>();
                float Level = (float)pm.BiomeMinLv / 4;
                float ExpReward = 0.05f;
                if (type == Main.anglerQuest)
                {
                    ExpReward = 0.15f;
                }
                else
                {
                    if (MainMod.LuckStrike(pm.Luck, 45000))
                    {
                        stack *= 8;
                        CombatText.NewText(Main.player[p].getRect(), Microsoft.Xna.Framework.Color.Green, "Very Lucky!", true);
                        MainMod.TriggerLuckyClovers(pm.player.Center, true);
                    }
                    else if (MainMod.LuckStrike(pm.Luck, 9000))
                    {
                        stack *= 2;
                        CombatText.NewText(Main.player[p].getRect(), Microsoft.Xna.Framework.Color.Green, "Lucky!", true);
                        MainMod.TriggerLuckyClovers(pm.player.Center, false);
                    }
                }
                if (stack > 1)
                    ExpReward += 0.02f * (stack - 1);
                pm.GetExpReward(Level, ExpReward, ExpReceivedPopText.ExpSource.Fishing);
            }
        }
    }
}
