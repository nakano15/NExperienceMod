using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace NExperience.NPCs
{
    public class TenrohEripmav : ModNPC
    {
        private const int MaskID = 176;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Tenroh Eripmav");
            Main.npcFrameCount[npc.type] = 3;
        }

        public override void SetDefaults()
        {
            npc.width = 34;
            npc.height = 32;
            npc.HitSound = SoundID.NPCHit1;
            npc.DeathSound = SoundID.NPCDeath1;
            npc.noTileCollide = true;
            npc.noGravity = true;
            npc.buffImmune[20] = true;
            npc.aiStyle = 5;
            npc.defense = 35;
            npc.damage = 82;
            npc.lifeMax = 3000;
            npc.value = 420f;
            npc.knockBackResist = 0.3f;
            aiType = animationType = MaskID;
        }
    }
}
