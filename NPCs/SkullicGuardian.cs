using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace NExperience.NPCs
{
    public class SkullicGuardian : ModNPC
    {
        private const int MaskID = 77;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Skullic Guardian");
            Main.npcFrameCount[npc.type] = Main.npcFrameCount[MaskID];
        }

        public override void SetDefaults()
        {
            npc.width = 18;
            npc.height = 40;
            npc.HitSound = SoundID.NPCHit2;
            npc.DeathSound = SoundID.NPCDeath2;
            npc.aiStyle = 3;
            npc.defense = 52;
            npc.damage = 69;
            npc.lifeMax = 460;
            npc.value = 420f;
            npc.knockBackResist = 0.03f;
            aiType = animationType = MaskID;
        }
    }
}
