using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using Terraria.ModLoader;

namespace NExperience
{
    public class ProjMod : GlobalProjectile
    {
        public int SpawnNPC = -1;
        public float LastDamagePercentage = 1f;
        public override bool InstancePerEntity
        {
            get
            {
                return true;
            }
        }

        public override void SetDefaults(Projectile projectile)
        {
            SpawnNPC = -1;
            LastDamagePercentage = 1f;
            if (MainMod.NpcProjSpawnPos > -1)// && projectile.type != Terraria.ID.ProjectileID.Nail)
            {
                NPC npc = Main.npc[MainMod.NpcProjSpawnPos];
                SpawnNPC = npc.whoAmI;
                if (projectile.damage != npc.damage)
                {
                    LastDamagePercentage = Main.npc[MainMod.NpcProjSpawnPos].GetGlobalNPC<NpcMod>().NpcStatus.ProjDamageMult;
                }
            }
            else
            {
                if(!Main.gameMenu && !projectile.hostile && projectile.friendly && !projectile.melee && !projectile.ranged && !projectile.magic && !projectile.minion && !projectile.thrown)
                {
                    int Owner = Main.netMode == 0 ? Main.myPlayer : projectile.owner;
                    LastDamagePercentage = Main.player[Owner].GetModPlayer<PlayerMod>().NeutralDamage;
                }
            }
        }
        
        public override bool PreAI(Projectile projectile)
        {
            if (LastDamagePercentage != 1f)
            {
                int LastDamage = projectile.damage;
                projectile.damage = (int)(projectile.damage * LastDamagePercentage);
                LastDamagePercentage = 1f;
            }
            return base.PreAI(projectile);
        }
    }
}
