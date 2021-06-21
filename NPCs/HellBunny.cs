using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace NExperience.NPCs
{
    public class HellBunny : ModNPC
    {
        private const int MaskID = NPCID.CorruptBunny;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Hell Bunny");
            Main.npcFrameCount[npc.type] = Main.npcFrameCount[MaskID];
        }

        public override void SetDefaults()
        {
            npc.width = 18;
            npc.height = 20;
            npc.damage = 34;
            npc.defense = 12;
            npc.aiStyle = 3;
            npc.lifeMax = 120;
            npc.HitSound = SoundID.NPCHit1;
            npc.knockBackResist = 0.78f;
            npc.DeathSound = SoundID.NPCDeath1;
            npc.value = 210f;
            npc.scale = 1.1f;
            aiType = animationType = MaskID;
        }
    }
}
