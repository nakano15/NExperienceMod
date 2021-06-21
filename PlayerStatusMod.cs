using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;

namespace NExperience
{
    public class PlayerStatusMod
    {
        public float MaxHealthMult = 1f, MaxManaMult = 1f, MeleeDamageMult = 1f, RangedDamageMult = 1f, MagicDamageMult = 1f, MinionDamageMult = 1f, NeutralDamageMult = 1f, DefenseMult = 1f, SummonCountMult = 1f, MeleeSpeedMult = 1f, KnockbackMult = 1f, MoveSpeedMult = 1f, MeleeCritMult = 1f, RangedCritMult = 1f, MagicCritMult = 1f, ManaCostMult = 1f,
            MeleeDamageSum = 0f, RangedDamageSum = 0f, MagicDamageSum = 0f, MinionDamageSum = 0f, NeutralDamageSum = 0f, MeleeSpeedSum = 0f, MoveSpeedSum = 0f, LuckFactorSum = 0f, CriticalDamageSum = 0f, DodgeRate = 0f, KnockbackSum = 0;
        public float MaxHealthSum = 0, MaxManaSum = 0, DefenseSum = 0, SummonCountSum = 0, MeleeCritSum = 0, RangedCritSum = 0, MagicCritSum = 0;

        public void ApplyStatus(Player player)
        {
            PlayerMod pm = player.GetModPlayer<PlayerMod>();
            player.statLifeMax2 = (int)((player.statLifeMax2 + MaxHealthSum) * MaxHealthMult);
            if(MainMod.AllowManaBoosts)
                player.statManaMax2 = (int)((player.statManaMax2 + MaxManaSum) * MaxManaMult);
            player.meleeDamage = (player.meleeDamage + MeleeDamageSum) * MeleeDamageMult;
            player.rangedDamage = (player.rangedDamage + RangedDamageSum) * RangedDamageMult;
            player.magicDamage = (player.magicDamage + MagicDamageSum) * MagicDamageMult;
            player.minionDamage = (player.minionDamage + MinionDamageSum) * MinionDamageMult;
            player.thrownDamage = (player.thrownDamage + RangedDamageSum) * RangedDamageMult;
            pm.NeutralDamage = (pm.NeutralDamage + NeutralDamageSum) * NeutralDamageMult;
            player.meleeSpeed = (player.meleeSpeed + MeleeSpeedSum) * MeleeSpeedMult;
            player.moveSpeed = (player.moveSpeed + MoveSpeedSum) * MoveSpeedMult;
            player.maxMinions = (int)((player.maxMinions + SummonCountSum) * SummonCountMult);
            player.meleeCrit = (int)((player.meleeCrit + MeleeCritSum) * MeleeCritMult);
            player.rangedCrit = (int)((player.rangedCrit + RangedCritSum) * RangedCritMult);
            player.magicCrit = (int)((player.magicCrit + MagicCritSum) * MagicCritMult);
            player.manaCost = player.manaCost * ManaCostMult;
            player.statDefense = (int)((player.statDefense + DefenseSum) * DefenseMult);
            pm.KbMult = KnockbackMult;
            pm.KbSum = KnockbackSum;
            pm.Luck += LuckFactorSum;
            pm.CriticalDamageBonusMult += CriticalDamageSum;
            if (DodgeRate > 100) DodgeRate = 100;
            pm.DodgeRate = DodgeRate;
        }
    }
}
