using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace NExperience.NPCs
{
    public class FallenMummy : ModNPC
    {
        private const int MaskID = NPCID.Mummy;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Fallen Mummy");
            Main.npcFrameCount[npc.type] = Main.npcFrameCount[MaskID];
        }

        public override void SetDefaults()
        {
            npc.width = 18;
            npc.height = 40;
            npc.HitSound = SoundID.NPCHit1;
            npc.DeathSound = SoundID.NPCDeath2;
            npc.aiStyle = 3;
            npc.damage = 11;
            npc.defense = 10;
            npc.lifeMax = 72;
            npc.knockBackResist = 0.5f;
            npc.value = 115f;
            aiType = animationType = MaskID;
        }
    }
}
