using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace NExperience.NPCs
{
    public class SkullicFighter : ModNPC
    {
        private const int MaskID = 77;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Skullic Fighter");
            Main.npcFrameCount[npc.type] = Main.npcFrameCount[MaskID];
        }

        public override void SetDefaults()
        {
            npc.width = 18;
            npc.height = 40;
            npc.HitSound = SoundID.NPCHit2;
            npc.DeathSound = SoundID.NPCDeath2;
            npc.aiStyle = 3;
            npc.defense = 25;
            npc.damage = 85;
            npc.lifeMax = 320;
            npc.value = 420f;
            npc.knockBackResist = 0.1f;
            aiType = animationType = MaskID;
        }
    }
}
