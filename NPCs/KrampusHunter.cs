using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace NExperience.NPCs
{
    public class KrampusHunter : ModNPC
    {
        private const int MaskID = NPCID.Krampus;
        const float DistanceFromTargetToShowUp = 180;
        public float DistancePercentage = 0;
        public int LockedTarget = -1;
        public bool BaggedPlayer = false;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Krampus Kidnapper");
            Main.npcFrameCount[npc.type] = Main.npcFrameCount[MaskID];
        }

        public override void SetDefaults()
        {
            aiType = animationType = MaskID;
            npc.width = 18;
            npc.height = 90;
            npc.aiStyle = 3;
            npc.damage = 100;
            if (!Main.hardMode)
            {
                npc.damage = 80;
                npc.defense = 10;
                npc.lifeMax = 1000;
            }
            else
            {
                npc.damage = 100;
                npc.defense = 20;
                npc.lifeMax = 2000;
            }
            npc.HitSound = SoundID.NPCHit1;
            npc.DeathSound = SoundID.NPCDeath1;
            npc.knockBackResist = 0.1f;
            npc.value = 3000f;
            npc.npcSlots = 1.75f;
        }

        public override bool PreAI()
        {
            if(LockedTarget == -1)
            {
                npc.TargetClosest();
                LockedTarget = npc.target;
            }
            return base.PreAI();
        }

        public override void PostAI()
        {
            if (BaggedPlayer)
                DistancePercentage = 1f;
            else
            {
                float Distance = (npc.Center - Main.player[npc.target].Center).Length();
                DistancePercentage = 1f + -Distance / 180f;
                if (DistancePercentage < -1)
                    DistancePercentage = -1;
            }
            if (BaggedPlayer)
            {
                Player target = Main.player[npc.target];
                if (!target.dead)
                {
                    npc.active = false;
                    target.legPosition = Vector2.Zero;
                }
                //56, 40
                target.headPosition = target.bodyPosition = new Vector2(0, 99999);
                Vector2 LegsPosition = new Vector2((56 + 2) - 41, 40 - 4);
                LegsPosition.X = npc.position.X + npc.width * 0.5f - LegsPosition.X * npc.direction - target.Center.X;
                LegsPosition.Y = npc.position.Y + npc.height - 134 + LegsPosition.Y - target.Center.Y;
                target.legPosition = LegsPosition;
                target.legRotation = 3.926991f * npc.direction;
                target.immuneAlpha = 0;
                target.legVelocity = target.bodyVelocity = target.headVelocity = Vector2.Zero;
            }
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            if(target.whoAmI == LockedTarget && damage > 0)
            {
                target.AddBuff(ModContent.BuffType<Buffs.Terrified>(), 10 * 60);
            }
        }

        public override void ModifyHitPlayer(Player target, ref int damage, ref bool crit)
        {
            if (target.whoAmI == LockedTarget)
            {
                if (!target.HasBuff(ModContent.BuffType<Buffs.Terrified>()))
                {
                    damage = 20;
                }
                else
                {
                    float ResultingHealthPercentage = (float)(target.statLife - damage) / target.statLifeMax2;
                    if (ResultingHealthPercentage < 0.2f)
                    {
                        damage = 1;
                        string DeathMessage = "";
                        switch (Main.rand.Next(4))
                        {
                            default:
                                DeathMessage = " was captured by Krampus.";
                                break;
                            case 1:
                                DeathMessage = " cordially went to visit Krampus cave.";
                                break;
                            case 2:
                                DeathMessage = " lost the battle against Krampus.";
                                break;
                            case 3:
                                DeathMessage = " got a free ride on Krampus bag.";
                                break;
                            case 4:
                                DeathMessage = " will fix the hunger problem Krampus had.";
                                break;
                        }
                        target.KillMe(Terraria.DataStructures.PlayerDeathReason.ByCustomReason(target.name + DeathMessage), 9999, npc.direction);
                        BaggedPlayer = true;
                    }
                }
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color drawColor)
        {
            if (!Main.NPCLoaded[MaskID])
                Main.instance.LoadNPC(MaskID);
            Texture2D texture = Main.npcTexture[MaskID];
            Vector2 DrawPosition = new Vector2(npc.position.X + npc.width * 0.5f, npc.position.Y + npc.height + 2) - Main.screenPosition;
            drawColor *= (DistancePercentage < 0 ? 0 : DistancePercentage);
            if (DistancePercentage >= 0)
            {
                drawColor.A = 255;
            }
            else
            {
                drawColor.A = (byte)(255 * (1f + DistancePercentage));
            }
            spriteBatch.Draw(texture, DrawPosition, npc.frame, drawColor, 0f, new Vector2(npc.frame.Width * 0.5f, npc.frame.Height), npc.scale, (npc.direction < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally), 0);
            return false;
        }

        public override void NPCLoot()
        {
            if (Main.rand.NextFloat() < 0.6)
            {
                Item.NewItem(npc.getRect(), ItemID.Coal, Main.rand.Next(2, 5));
            }
            if (Main.rand.NextFloat() < 0.4)
            {
                Item.NewItem(npc.getRect(), ItemID.Present, Main.rand.Next(3, 6));
            }
            if (Main.rand.NextFloat() < 0.1)
            {
                Item.NewItem(npc.getRect(), ItemID.ChristmasHook);
            }
            if (Main.rand.NextFloat() < 0.05)
            {
                Item.NewItem(npc.getRect(), ItemID.FestiveWings);
            }
            if (Main.rand.NextFloat() < 0.03)
            {
                Item.NewItem(npc.getRect(), ItemID.ReindeerBells);
            }
            if (Main.rand.NextFloat() < 0.01)
            {
                Item.NewItem(npc.getRect(), ItemID.BabyGrinchMischiefWhistle);
            }
            if (Main.rand.NextFloat() < 0.01)
            {
                Item.NewItem(npc.getRect(), ItemID.NaughtyPresent);
            }
            if (Main.invasionType <= 0 && Main.rand.NextFloat() < 0.01)
            {
                Main.StartInvasion(InvasionID.SnowLegion);
            }
            if (!BaggedPlayer)
            {
                MainMod.SendChatMessage("You feel at ease now.", Color.Cyan);
				NpcMod.CanSpawnKrampus = false;
            }
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if(NpcMod.CanSpawnKrampus && !spawnInfo.water && !spawnInfo.playerInTown)
            {
                return 1f / 150;
            }
            return base.SpawnChance(spawnInfo);
        }
    }
}
