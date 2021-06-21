using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace NExperience.NPCs
{
    public class SkullicAssassin : ModNPC
    {
        private const int MaskID = NPCID.Skeleton;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Skullic Assassin");
            Main.npcFrameCount[npc.type] = Main.npcFrameCount[MaskID];
        }

        public override void SetDefaults()
        {
            npc.width = 18;
            npc.height = 40;
            npc.HitSound = SoundID.NPCHit2;
            npc.DeathSound = SoundID.NPCDeath2;
            npc.aiStyle = 3;
            npc.damage = 16;
            npc.defense = 8;
            npc.lifeMax = 52;
            npc.knockBackResist = 0.5f;
            npc.value = 75f;
            aiType = animationType = MaskID;
        }
    }
}
