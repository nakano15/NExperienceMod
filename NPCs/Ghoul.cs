using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace NExperience.NPCs
{
    public class Ghoul : ModNPC
    {
        private const int MaskID = NPCID.Zombie;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Ghoul");
            Main.npcFrameCount[npc.type] = Main.npcFrameCount[MaskID];
        }

        public override void SetDefaults()
        {
            npc.width = 18;
            npc.height = 40;
            npc.HitSound = SoundID.NPCHit1;
            npc.DeathSound = SoundID.NPCDeath2;
            npc.aiStyle = 3;
            npc.defense = 30;
            npc.damage = 55;
            npc.lifeMax = 420;
            npc.value = 420f;
            npc.knockBackResist = 0.1f;
            aiType = animationType = MaskID;
        }
    }
}
