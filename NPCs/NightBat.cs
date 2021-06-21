using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace NExperience.NPCs
{
    public class NightBat : ModNPC
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Night Bat");
            Main.npcFrameCount[npc.type] = Main.npcFrameCount[NPCID.CaveBat];
        }

        public override void SetDefaults()
        {
            npc.npcSlots = 0.5f;
            npc.scale = 0.7f;
            npc.width = 15;
            npc.height = 12;
            npc.damage = 7;
            npc.defense = 1;
            npc.lifeMax = 11;
            npc.HitSound = SoundID.NPCHit1;
            npc.knockBackResist = 0.56f;
            npc.DeathSound = SoundID.NPCDeath4;
            npc.noGravity = true;
            npc.aiStyle = 14;
            npc.value = 63f;
            npc.scale = 0.85f;
            aiType = animationType = NPCID.CaveBat;
        }
    }
}
