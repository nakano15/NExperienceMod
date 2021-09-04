using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using Terraria.ModLoader;

namespace NExperience
{
    public class RecipeMod : GlobalRecipe
    {
        public override void OnCraft(Item item, Recipe recipe)
        {
            float LuckValue = Main.player[Main.myPlayer].GetModPlayer<PlayerMod>().Luck;
            if (MainMod.LuckStrike(LuckValue, 30000))
            {
                MainMod.TriggerLuckyClovers(Main.player[Main.myPlayer].Center, true);
                bool HasPrefix = true;
                if (item.melee)
                {
                    item.Prefix(Terraria.ID.PrefixID.Legendary);
                }
                else if (item.ranged)
                {
                    item.Prefix(Terraria.ID.PrefixID.Unreal);
                }
                else if (item.magic)
                {
                    item.Prefix(Terraria.ID.PrefixID.Mythical);
                }
                else if (item.summon)
                {
                    item.Prefix(Terraria.ID.PrefixID.Ruthless);
                }
                else
                {
                    HasPrefix = false;
                }
                if (!HasPrefix)
                {
                    item.stack += 7;
                    Player player = Main.player[Main.myPlayer];
                    while (item.stack > item.maxStack)
                    {
                        int ToDiscount = item.maxStack - item.stack;
                        if (ToDiscount > item.maxStack)
                            ToDiscount = item.maxStack;
                        Item.NewItem(player.getRect(), item.type, ToDiscount);
                        item.stack -= ToDiscount;
                    }
                }
            }
            else if (MainMod.LuckStrike(LuckValue, 6000))
            {
                MainMod.TriggerLuckyClovers(Main.player[Main.myPlayer].Center, false);
                bool HasPrefix = true;
                {
                    int[] BestMods = new int[0];
                    if (item.melee)
                    {
                        BestMods = new int[]
                        {
                            Terraria.ID.PrefixID.Legendary,
                            Terraria.ID.PrefixID.Savage,
                            Terraria.ID.PrefixID.Sharp,
                            Terraria.ID.PrefixID.Dangerous
                        };
                    }
                    else if (item.ranged)
                    {
                        BestMods = new int[]
                        {
                            Terraria.ID.PrefixID.Unreal,
                            Terraria.ID.PrefixID.Deadly,
                            Terraria.ID.PrefixID.Rapid,
                            Terraria.ID.PrefixID.Dangerous
                        };
                    }
                    else if (item.magic)
                    {
                        BestMods = new int[]
                        {
                            Terraria.ID.PrefixID.Mythical,
                            Terraria.ID.PrefixID.Masterful,
                            Terraria.ID.PrefixID.Mystic,
                            Terraria.ID.PrefixID.Adept
                        };
                    }
                    else if (item.summon)
                    {
                        BestMods = new int[]
                        {
                            Terraria.ID.PrefixID.Godly,
                            Terraria.ID.PrefixID.Superior,
                            Terraria.ID.PrefixID.Demonic,
                            Terraria.ID.PrefixID.Hurtful
                        };
                    }
                    else
                    {
                        HasPrefix = false;
                    }
                    if (BestMods.Length > 0)
                    {
                        item.Prefix(BestMods[Main.rand.Next(BestMods.Length)]);
                    }
                }
                if (!HasPrefix)
                {
                    item.stack++;
                    Player player = Main.player[Main.myPlayer];
                    while (item.stack > item.maxStack)
                    {
                        int ToDiscount = item.maxStack - item.stack;
                        if (ToDiscount > item.maxStack)
                            ToDiscount = item.maxStack;
                        Item.NewItem(player.getRect(), item.type, ToDiscount);
                        item.stack -= ToDiscount;
                    }
                }
            }
        }
    }
}
