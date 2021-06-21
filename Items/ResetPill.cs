using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using Terraria.ModLoader;

namespace NExperience.Items
{
    public class ResetPill : ModItem
    {
        public override void SetStaticDefaults()
        {
            Tooltip.SetDefault("Resets the status points invested.");
        }

        public override void SetDefaults()
        {
            item.useStyle = 2;
            item.UseSound = Terraria.ID.SoundID.Item2;
            item.useTurn = false;
            item.useAnimation = 17;
            item.useTime = 17;
            item.width = 24;
            item.height = 24;
            item.maxStack = 30;
            item.consumable = true;
            item.value = Item.sellPrice(0, 0, 2, 50);
        }

        public override bool UseItem(Player player)
        {
            player.GetModPlayer<PlayerMod>().GetGameModeInfo.ResetPointsInvested();
            Main.NewText("Status points invested were resetted.");
            return true;
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(Terraria.ID.ItemID.Damselfish, 5);
            recipe.AddIngredient(Terraria.ID.ItemID.NeonTetra, 5);
            recipe.AddIngredient(Terraria.ID.ItemID.Stinkfish, 5);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
}
