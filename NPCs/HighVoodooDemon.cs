using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace NExperience.NPCs
{
    public class HighVoodooDemon : ModNPC
    {
        private const int MaskID = Terraria.ID.NPCID.VoodooDemon;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("High Voodoo Demon");
            Main.npcFrameCount[npc.type] = Main.npcFrameCount[MaskID];
        }

        public override void SetDefaults()
        {
            npc.width = 28;
            npc.height = 48;
            npc.HitSound = SoundID.NPCHit21;
            npc.DeathSound = SoundID.NPCDeath24;
            npc.aiStyle = 14;
            npc.buffImmune[24] = npc.buffImmune[39] = true;
            npc.lavaImmune = true;
            npc.noGravity = true;
            npc.defense = 15;
            npc.damage = 86;
            npc.lifeMax = 760;
            npc.value = 420f;
            npc.knockBackResist = 0.6f;
            aiType = animationType = MaskID;
        }
    }
}
