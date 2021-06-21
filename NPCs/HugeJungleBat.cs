using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace NExperience.NPCs
{
    public class HugeJungleBat : ModNPC
    {
        private const int MaskID = NPCID.GiantBat;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Huge Jungle Bat");
            Main.npcFrameCount[npc.type] = Main.npcFrameCount[MaskID];
        }

        public override void SetDefaults()
        {
            npc.width = 26;
            npc.height = 20;
            npc.HitSound = SoundID.NPCHit1;
            npc.DeathSound = SoundID.NPCDeath4;
            npc.noGravity = true;
            npc.aiStyle = 14;
            npc.defense = 32;
            npc.damage = 55;
            npc.lifeMax = 415;
            npc.value = 420f;
            npc.knockBackResist = 0.3f;
            aiType = animationType = MaskID;
        }
    }
}
