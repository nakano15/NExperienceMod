using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;

namespace NExperience
{
    public class PlayerRebirthStatus
    {
        public int CurrentRebirthLevel = 0;
        public float HealthMult = 1f, DefMult = 1f, DamMult = 1f;
        public float ExpMult = 1f;
        public int RbLevelRequired = 125;

        public void Update(Player player, int RebirthLevel)
        {
            if (RebirthLevel == 0)
                return;
            if (CurrentRebirthLevel < RebirthLevel)
            {
                CurrentRebirthLevel++;
                ExpMult = (float)Math.Ceiling(ExpMult * (1f / 1.75f));
                if (ExpMult == (float)Math.Ceiling(0.000000001))
                {
                    RbLevelRequired = (int)(RbLevelRequired * 1.05f);
                }
                HealthMult *= 1.2f;
                DamMult *= 1.2f;
                DefMult *= 1.2f;
            }
            player.statLifeMax2 = (int)(player.statLifeMax2 * HealthMult);
            player.meleeDamage = player.meleeDamage * DamMult;
            player.rangedDamage = player.rangedDamage * DamMult;
            player.magicDamage = player.magicDamage * DamMult;
            player.minionDamage = player.minionDamage * DamMult;
            player.statDefense = (int)(player.statDefense * DefMult);
            if (player.statLifeMax2 < 0)
                player.statLifeMax2 = int.MaxValue;
            if (player.meleeDamage < 0)
                player.meleeDamage = 0;
            if (player.rangedDamage < 0)
                player.rangedDamage = 0;
            if (player.magicDamage < 0)
                player.magicDamage = 0;
            if (player.minionDamage < 0)
                player.minionDamage = 0;
            //if (player.statDefense < 0)
            //    player.statDefense = int.MaxValue;
        }

        public void Reset()
        {
            CurrentRebirthLevel = 0;
            HealthMult = 1f;
            DefMult = 1f;
            DamMult = 1f;
            ExpMult = 1f;
            RbLevelRequired = 125;
        }
    }
}
