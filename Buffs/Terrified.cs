using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using Terraria.ModLoader;

namespace NExperience.Buffs
{
    public class Terrified : ModBuff
    {
        public override void SetDefaults()
        {
            DisplayName.SetDefault("Terrified");
            Description.SetDefault("Fear is difficulting your escape.");
            Main.debuff[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.moveSpeed *= 0.33f;
            player.rocketTime = 0;
            player.wingTime = 0;
            player.meleeDamage -= 0.3f;
            player.rangedDamage -= 0.3f;
            player.magicDamage -= 0.3f;
            player.minionDamage -= 0.3f;
            player.statDefense -= 10;
            player.manaRegen = 0;
        }
    }
}
