using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace NExperience
{
    public class NpcMod : GlobalNPC
    {
        public static bool HasNocturnalBossSpawned = false;
        public GameModeData NpcStatus = new GameModeData();

        public override bool InstancePerEntity
        {
            get
            {
                return true;
            }
        }

        public override bool CloneNewInstances
        {
            get
            {
                return false;
            }
        }

        public override bool PreAI(NPC npc)
        {
            if (npc.type == Terraria.ID.NPCID.EyeofCthulhu || npc.type == Terraria.ID.NPCID.SkeletronHead || npc.type == Terraria.ID.NPCID.Retinazer ||
                npc.type == Terraria.ID.NPCID.Spazmatism || npc.type == Terraria.ID.NPCID.TheDestroyer || npc.type == Terraria.ID.NPCID.SkeletronPrime)
                HasNocturnalBossSpawned = true;
            MainMod.NpcProjSpawnPos = npc.whoAmI;
            npc.damage = npc.defDamage;
            npc.defense = npc.defDefense;
            return true;
        }

        public override void PostAI(NPC npc)
        {
            MainMod.NpcProjSpawnPos = -1;
            try
            {
                NpcStatus.UpdateNpc(npc);
            }
            catch (Exception ex) { }
        }

        public override void SetupShop(int type, Chest shop, ref int nextSlot)
        {
            if (MainMod.PotionsForSale)
            {
                if (type == Terraria.ID.NPCID.Merchant)
                {
                    shop.item[nextSlot].SetDefaults(Terraria.ID.ItemID.HealingPotion);
                    nextSlot++;
                    shop.item[nextSlot].SetDefaults(Terraria.ID.ItemID.GreaterHealingPotion);
                    nextSlot++;
                    shop.item[nextSlot].SetDefaults(Terraria.ID.ItemID.SuperHealingPotion);
                    shop.item[nextSlot].value *= 10;
                    nextSlot++;
                }
            }
        }

        public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
        {
            if (player.GetModPlayer<PlayerMod>().ZoneGraveyard)
            {
                spawnRate = (int)(spawnRate * 0.5f);
                maxSpawns *= 2;
            }
            if (spawnRate < 1) spawnRate = 1;
        }

        public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.invasion || spawnInfo.lihzahrd || spawnInfo.spiderCave || spawnInfo.marble || spawnInfo.granite)
                return;
            if (spawnInfo.player.GetModPlayer<PlayerMod>().ZoneGraveyard)
            {
                pool.Clear();
                if (Main.hardMode)
                {
                    pool.Add(ModContent.NPCType<NPCs.SkullicGuardian>(), 0.333f);
                    pool.Add(ModContent.NPCType<NPCs.SkullicFighter>(), 0.5f);
                    pool.Add(ModContent.NPCType<NPCs.Ghoul>(), 1);
                }
                else
                {
                    pool.Add(ModContent.NPCType<NPCs.SkullicAssassin>(), 0.3333f);
                    pool.Add(Terraria.ID.NPCID.Zombie, 1);
                }
            }
            else if (!MainMod.DisableModEnemies && !spawnInfo.playerInTown && !Main.bloodMoon && !Main.snowMoon && !Main.pumpkinMoon && !Main.dayTime && (spawnInfo.player.ZoneOverworldHeight || spawnInfo.player.ZoneSkyHeight))
            {
                if (Main.hardMode) pool.Add(ModContent.NPCType<NPCs.Ghoul>(), 0.333f);
                pool.Add(ModContent.NPCType<NPCs.SkullicAssassin>(), 0.2f);
                pool.Add(ModContent.NPCType<NPCs.NightBat>(), 0.142f);
            }
            if (MainMod.DisableModEnemies)
                return;
            if (!spawnInfo.playerInTown || Main.bloodMoon)
            {
                if (spawnInfo.player.ZoneUnderworldHeight)
                {
                    if (Main.hardMode)
                    {
                        pool.Add(ModContent.NPCType<NPCs.HighDemon>(), 0.333f);
                        pool.Add(ModContent.NPCType<NPCs.HighVoodooDemon>(), 0.08333f);
                    }
                    pool.Add(ModContent.NPCType<NPCs.HellBunny>(), 0.2f);
                }
                if (Main.hardMode)
                {
                    if (spawnInfo.player.ZoneJungle && !spawnInfo.lihzahrd && spawnInfo.player.ZoneRockLayerHeight)
                    {
                        pool.Add(ModContent.NPCType<NPCs.HugeJungleBat>(), 0.25f);
                        if (NPC.plantBoss == -1)
                            pool.Add(ModContent.NPCType<NPCs.TenrohEripmav>(), 0.0001f);
                    }
                }
            }
        }

        public void TombstoneGenerator(NPC npc, int plr)
        {
            bool CreateTombstone = false;
            switch (npc.type)
            {
                case 3:
                case 132:
                case 161:
                case 186:
                case 187:
                case 188:
                case 189:
                case 200:
                case 223:
                case 254:
                case 255:
                case 319:
                case 320:
                case 321:
                    CreateTombstone = true;
                    break;
            }
            if (Main.netMode != 1 && CreateTombstone && plr != -1 && !Main.player[plr].GetModPlayer<PlayerMod>().ZoneGraveyard && Main.rand.Next(5) == 0)
            {
                float num = (float)Main.rand.Next(-35, 35 + 1) * 0.1f;
                int num2 = Projectile.NewProjectile(npc.position.X + npc.width * 0.5f, npc.position.Y, (float)Main.rand.Next(10, 30) * 0.1f * (float)npc.direction + num, (float)Main.rand.Next(-40, -20) * 0.1f, 43, 0, 0f, Main.myPlayer);
                switch (Main.rand.Next(75))
                {
                    case 0:
                        Main.projectile[num2].miscText = "A zombie perished here, better remove this tombstone soon.";
                        break;
                    case 1:
                        Main.projectile[num2].miscText = "The following text has been received by fax and instantly written here.";
                        break;
                    case 2:
                        Main.projectile[num2].miscText = "Still dead.";
                        break;
                    case 3:
                        Main.projectile[num2].miscText = "\"This can't get worse....\", he were wrong.";
                        break;
                    case 4:
                        Main.projectile[num2].miscText = "\"Look,the wall of flesh!!!\" was his last words.";
                        break;
                    case 5:
                        Main.projectile[num2].miscText = "\"Don't push that thing from the grenade,or we will...\" were said before the boom.";
                        break;
                    case 6:
                        Main.projectile[num2].miscText = "She learned that no one can use a slime as a cushion.";
                        break;
                    case 7:
                        Main.projectile[num2].miscText = "\"A button! I'll press it.\" made him turn into more than 150 pieces.";
                        break;
                    case 8:
                        Main.projectile[num2].miscText = "\"You will die today Zombie!!!\" he said before getting his own tombstone.";
                        break;
                    case 9:
                        Main.projectile[num2].miscText = "\"Eye of Cthulhu is easy.\" she said. \"AAAAAAHHHHHH!!!\" she also said.";
                        break;
                    case 10:
                        Main.projectile[num2].miscText = "His charity act made Eater of Worlds lose " + Main.rand.Next(10, 95 + 1) + "% of hunger.";
                        break;
                    case 11:
                        Main.projectile[num2].miscText = "She tried to pet a unicorn.";
                        break;
                    case 12:
                        Main.projectile[num2].miscText = "\"I will not listen what a Crazy Old Man have to say\", you should have listened.";
                        break;
                    case 13:
                        Main.projectile[num2].miscText = "We found " + Main.rand.Next(10) + "/10 pieces of him. If you find the other pieces, please call 555-" + Main.rand.Next(10).ToString() + Main.rand.Next(10).ToString() + Main.rand.Next(10).ToString() + Main.rand.Next(10).ToString() + ".";
                        break;
                    case 14:
                        Main.projectile[num2].miscText = "He learned that can't fight against the Wall of Flesh using a Tyrfing.";
                        break;
                    case 15:
                        Main.projectile[num2].miscText = "CoolShadow should have used his coins on insurance...";
                        break;
                    case 16:
                        Main.projectile[num2].miscText = "robflop couldn't slap faster...";
                        break;
                    case 17:
                        Main.projectile[num2].miscText = "Nakano found the chest, but not that pressure plate.";
                        break;
                    case 18:
                        Main.projectile[num2].miscText = "She thought it was a Spider super hero going to save her from the Crimson.";
                        break;
                    case 19:
                        Main.projectile[num2].miscText = "He needed a hug, a Zombie provided that.";
                        break;
                    case 20:
                        Main.projectile[num2].miscText = "That adventurer discovered Face Monster's breath.";
                        break;
                    case 21:
                        Main.projectile[num2].miscText = "He asked for brains and got bullets instead.";
                        break;
                    case 22:
                        Main.projectile[num2].miscText = "He didn't counted on Brain of Cthulhu's cleverness.";
                        break;
                    case 23:
                        Main.projectile[num2].miscText = "She turned into Plantera's gum.";
                        break;
                    case 24:
                        Main.projectile[num2].miscText = "Everybody knows the cake is a lie, why he tried to eat it?";
                        break;
                    case 25:
                        Main.projectile[num2].miscText = "Do not do the Victory Dance when you beat Eye of Cthulhu's first part.";
                        break;
                    case 26:
                        Main.projectile[num2].miscText = "This one got stinged by Queen Bee. Wonder where.";
                        break;
                    case 27:
                        Main.projectile[num2].miscText = "Feeling fat? Do like her! Use Golem's stomp as solution to that problem.";
                        break;
                    case 28:
                        Main.projectile[num2].miscText = "The more, the better.... For THEM.";
                        break;
                    case 29:
                        Main.projectile[num2].miscText = "He thought they were dead. But they were not.";
                        break;
                    case 30:
                        Main.projectile[num2].miscText = "\"I don't need potions!\" he said before fighting Plantera.";
                        break;
                    case 31:
                        Main.projectile[num2].miscText = "\"Let's move Plantera to the surface, will be easier to kill.\" she said before knowing that things changed.";
                        break;
                    case 32:
                        Main.projectile[num2].miscText = "\"I have enough time to kill skeletron prime!\" he said before dawn.";
                        break;
                    case 33:
                        Main.projectile[num2].miscText = "They though he could defeat the Dungeon Guardian with a Copper Shortsword.";
                        break;
                    case 34:
                        Main.projectile[num2].miscText = "He tried to be like Yrimir.";
                        break;
                    case 35:
                        Main.projectile[num2].miscText = "Poor guy. He forgot the laws of physics.";
                        break;
                    case 36:
                        Main.projectile[num2].miscText = "The Groom now got a Bride.";
                        break;
                    case 37:
                        Main.projectile[num2].miscText = "Self confident Terrarian VS Crimera, who won?";
                        break;
                    case 38:
                        Main.projectile[num2].miscText = "Too bad she will not be able to reveal Murasame's secret...";
                        break;
                    case 39:
                        Main.projectile[num2].miscText = "The fight against the Dragon ended up on 1x0... To the Dragon.";
                        break;
                    case 40:
                        Main.projectile[num2].miscText = "He tried to explore the castle... They tried to tell him to not do that...";
                        break;
                    case 41:
                        Main.projectile[num2].miscText = "Is this tombstone really necessary?";
                        break;
                    case 42:
                        Main.projectile[num2].miscText = "The Red Beast avenged his friend's death.";
                        break;
                    case 43:
                        Main.projectile[num2].miscText = "This one died because tried to ride a Unicorn.";
                        break;
                    case 44:
                        Main.projectile[num2].miscText = "She thought that there were one, on truth, there were two.";
                        break;
                    case 45:
                        Main.projectile[num2].miscText = "Someone caused this.";
                        break;
                    case 46:
                        Main.projectile[num2].miscText = "Yeah... He had to say \"Dead Branch is useless, i'll toss this away\" when were near the water.";
                        break;
                    case 47:
                        Main.projectile[num2].miscText = "A Solar Eclipse made this party interesting.";
                        break;
                    case 48:
                        Main.projectile[num2].miscText = "Someone once told me to not leave the character afk while equipping a Ocean Shield. I should have heard her.";
                        break;
                    case 49:
                        Main.projectile[num2].miscText = "\"Fishing is not dangerous.\", he said before discovering the Truffle Worm's usefullness.";
                        break;
                    case 50:
                        Main.projectile[num2].miscText = "She discovered that the cool realm is not so cool at all.";
                        break;
                    case 51:
                        {
                            string s = "there is a Zombie wanting your brain";
                            switch (Main.rand.Next(10))
                            {
                                case 0:
                                    s = "there are " + (WorldGen.crimson ? "Crimera" : "Eater of Souls") + " wanting your " + (WorldGen.crimson ? "blood" : "soul");
                                    break;
                                case 1:
                                    s = "a Terrarian digging a hellevator";
                                    break;
                                case 2:
                                    s = "the Wall of Flesh is doing a hell ride";
                                    break;
                                case 3:
                                    s = "a Slime devouring a Bunny";
                                    break;
                                case 4:
                                    s = "someone is falling off a cliff";
                                    break;
                                case 5:
                                    s = "a Dungeon Guardian killed a Terrarian";
                                    break;
                                case 6:
                                    s = "the Groom is on a marriage";
                                    break;
                                case 7:
                                    s = "the Guide is derping around";
                                    break;
                                case 8:
                                    s = "the Goblins are playing poker";
                                    break;
                                case 9:
                                    s = "the Developers finished working on Terraria version 1.4";
                                    break;
                            }
                            Main.projectile[num2].miscText = "While you are reading this, " + s + ".";
                        }
                        break;
                    case 52:
                        Main.projectile[num2].miscText = "Well... At least she got a free ride on Wyvern Airlines...";
                        break;
                    case 53:
                        Main.projectile[num2].miscText = "Keep thinking that landmines does not works.";
                        break;
                    case 54:
                        Main.projectile[num2].miscText = "And the luck number of this night is.... " + Main.rand.Next(100, 1000) + "!";
                        break;
                    case 55:
                        Main.projectile[num2].miscText = "This is what MashiroSora got because her summon upgrade were on the Underworld.";
                        break;
                    case 56:
                        Main.projectile[num2].miscText = "SonicPivot fell on the trolling that his internet were going to fall, and tossed his hardcore character inside the pre-skeletron Dungeon.";
                        break;
                    case 57:
                        Main.projectile[num2].miscText = "This one got tired of playing with Skulliton.";
                        break;
                    case 58:
                        Main.projectile[num2].miscText = "Here will lie " + Main.player[plr].name + ", ";
                        if (Main.player[plr].statLife <= Main.player[plr].statLifeMax2 * 0.5)
                        {
                            Main.projectile[num2].miscText += "if does not watch out for traps.";
                        }
                        else if (Main.player[plr].statLife <= Main.player[plr].statLifeMax2 * 0.25)
                        {
                            Main.projectile[num2].miscText += "if wont stop being hugged by zombies.";
                        }
                        else if (Main.player[plr].statLife <= Main.player[plr].statLifeMax2 * 0.10)
                        {
                            Main.projectile[num2].miscText += "if get touched by slimes.";
                        }
                        else
                        {
                            Main.projectile[num2].miscText += "if afk grind.";
                        }
                        break;
                    case 59:
                        Main.projectile[num2].miscText = "Should not spam potions during boss fights.";
                        break;
                    case 60:
                        Main.projectile[num2].miscText = "If you are reading this, then input this on the chat and then hit Enter: ";
                        string code = "";
                        for (int i = 0; i < 6; i++)
                        {
                            int words = Main.rand.Next(16);
                            switch (words)
                            {
                                case 0:
                                    code += "F";
                                    break;
                                case 1:
                                    code += "E";
                                    break;
                                case 2:
                                    code += "D";
                                    break;
                                case 3:
                                    code += "C";
                                    break;
                                case 4:
                                    code += "B";
                                    break;
                                case 5:
                                    code += "A";
                                    break;
                                case 6:
                                    code += "9";
                                    break;
                                case 7:
                                    code += "8";
                                    break;
                                case 8:
                                    code += "7";
                                    break;
                                case 9:
                                    code += "6";
                                    break;
                                case 10:
                                    code += "5";
                                    break;
                                case 11:
                                    code += "4";
                                    break;
                                case 12:
                                    code += "3";
                                    break;
                                case 13:
                                    code += "2";
                                    break;
                                case 14:
                                    code += "1";
                                    break;
                                case 15:
                                    code += "0";
                                    break;
                            }
                        }
                        Main.projectile[num2].miscText += code;
                        break;
                    case 61:
                        Main.projectile[num2].miscText = "It's no good having a Bunny Avenger for Easter.";
                        break;
                    case 62:
                        Main.projectile[num2].miscText = "Were too close to level up...";
                        break;
                    case 63:
                        Main.projectile[num2].miscText = "His obsidian generation machine broke.";
                        break;
                    case 64:
                        Main.projectile[num2].miscText = "I does not know what to say about this person, I only remember of yelling at him before running wild in a cavern.";
                        break;
                    case 65:
                        Main.projectile[num2].miscText = (Main.rand.Next(2) == 0 ? "He" : "She") + " did not survive N Terraria mod bugs...";
                        break;
                    case 66:
                        Main.projectile[num2].miscText = "The last thing told to him, was \"Do not recklessly charge on the boss.\".";
                        break;
                    case 67:
                        Main.projectile[num2].miscText = "She were playing with the Dungeon Shackles, but didn't expected it to lock itself.";
                        break;
                    case 68:
                        Main.projectile[num2].miscText = "His Lavender Tower structure got way too realist.";
                        break;
                    case 69:
                        Main.projectile[num2].miscText = "Nakano should not have tried to organize the inventory while under water..";
                        break;
                    case 70:
                        Main.projectile[num2].miscText = "\"We can escape the Wall of Flesh by using the Magic Mirror!\", now you need to find the tombstones of his friends.";
                        break;
                    case 71:
                        Main.projectile[num2].miscText = "\"We'll be alright if It doesn't use the laser.\", then the Moon Lord used the laser.";
                        break;
                    case 72:
                        Main.projectile[num2].miscText = "She said \"I thought it was a stone.\" before being smashed by a boulder.";
                        break;
                    case 73:
                        Main.projectile[num2].miscText = "\"I'm using Cloud in a Bottle. I can break my fall before reach the floor.\", but he didn't counted on his bad timing.";
                        break;
                    default:
                        Main.projectile[num2].miscText = "A dead person lies here.";
                        break;
                }
            }
        }

        public static bool IsPreHardmodeMonster(NPC npc)
        {
            if (Main.expertMode)
                return false;
            int Type = npc.type, NetID = npc.netID;
            if (Type == 4 || Type == 5 || (Type >= 13 && Type <= 15) || Type == 266 || Type == 267 || Type == 50 || Type == 222 || Type == 35 || Type == 36)
                return true;
            if (Type == 26 || Type == 29 || Type == 27 || Type == 28 || Type == 111)
                return true;
            if ((Type == 1 && (NetID == 1 || NetID == -3 || NetID == -4 || NetID == -5 || NetID == -6 || NetID == -7 || NetID == -8 || NetID == -9 || NetID == -10 )) || Type == 147 || Type == 537 || Type == 184 ||
                Type == 204 || Type == 16 || Type == 59 || Type == 71 || Type == 535 || Type == 225 || Type == Terraria.ID.NPCID.SlimeRibbonYellow || Type == Terraria.ID.NPCID.SlimeRibbonWhite || 
                Type == Terraria.ID.NPCID.SlimeRibbonGreen || Type == Terraria.ID.NPCID.SlimeRibbonRed || Type == Terraria.ID.NPCID.BunnySlimed)
                return true;
            return Type == 31 || Type == 257 || Type == 69 || Type == 508 || Type == 509 || Type == 210 || Type == 211 || Type == 72 || Type == 239 || Type == 240 || Type == 63 || Type == 64 ||
                (Type >= 39 && Type <= 41) || Type == 49 || Type == 217 || Type == 67 || Type == 494 || Type == 495 || Type == 173 || Type == 34 || Type == 218 || Type == 32 ||
                Type == 62 || Type == 2 || (Type >= 190 && Type <= 194) || Type == 317 || Type == 318 || (Type >= 7 && Type <= 9) || Type == 3 || Type == 430 || Type == 432 || 
            Type == 433 || Type == 434 || Type == 435 || Type == 436|| Type == 132 || (Type >= 186 && Type <= 189) || Type == 223 || Type == 161 || Type == 254 || Type == 255 || 
            Type == 431 || Type == 53 || Type == 536 || Type == 489 || (Type >= 319 && Type <= 321) || Type == 331 || Type == 332 || Type == 71 || Type == 6 || Type == 181 || 
            Type == 24 || Type == 259 || Type == 496 || Type == 497 || (Type >= 10 && Type <= 12) || Type == 73 || Type == 483 || Type == 482 || Type == 48 || Type == 60 || 
            Type == 481 || Type == 20 || (Type >= 231 && Type <= 235) || Type == 150 || Type == 51 || Type == 219 || Type == 43 || Type == 23 || Type == 258 || Type == 195 || 
            Type == 196 || Type == 58 || Type == 301 || (Type >= 498 && Type <= 506) || Type == 220 || Type == 65 || Type == 21 || (Type >= 201 && Type <= 203) || (Type >= 449 && Type <= 452) ||
            (Type >= 322 && Type <= 324) || Type == 56 || Type == 185 || Type == 70 || Type == 221 || Type == 45 || (Type >= 513 && Type <= 515) || Type == 44 || Type == 167 || 
            Type == 66 || Type == 61 || Type == 164 || Type == 165 || Type == 33 || Type == 25 || Type == 30 || Type == 546;
        }

        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            if (projectile.type == Terraria.ID.ProjectileID.Boulder || projectile.type == Terraria.ID.ProjectileID.PoisonDartTrap || projectile.type == Terraria.ID.ProjectileID.FlamesTrap ||
                projectile.type == Terraria.ID.ProjectileID.FlamethrowerTrap || projectile.type == Terraria.ID.ProjectileID.SpikyBallTrap || projectile.type == Terraria.ID.ProjectileID.SpearTrap ||
                projectile.type == Terraria.ID.ProjectileID.RocketFireworkBlue || projectile.type == Terraria.ID.ProjectileID.RocketFireworkGreen || projectile.type == Terraria.ID.ProjectileID.RocketFireworkRed ||
                projectile.type == Terraria.ID.ProjectileID.RocketFireworkYellow || projectile.type == Terraria.ID.ProjectileID.SandBallFalling || projectile.type == Terraria.ID.ProjectileID.PearlSandBallFalling ||
                projectile.type == Terraria.ID.ProjectileID.MudBall || projectile.type == Terraria.ID.ProjectileID.AshBallFalling || projectile.type == Terraria.ID.ProjectileID.CrimsandBallFalling ||
                projectile.type == Terraria.ID.ProjectileID.EbonsandBallFalling || projectile.type == Terraria.ID.ProjectileID.SiltBall || projectile.type == Terraria.ID.ProjectileID.SlushBall)
            {
                damage = (int)(damage * npc.GetGlobalNPC<NpcMod>().NpcStatus.HealthChangePercentage);
            }
        }

        public override void SetDefaults(NPC npc)
        {
            //NpcStatus = new GameModeData();
            bool ResetHealthValues = true;
            if (MainMod.NpcProjSpawnPos > -1)
            {
                NPC parentNpc = Main.npc[MainMod.NpcProjSpawnPos];
                if (parentNpc == npc)
                {
                    //if (npc.type == 239 || npc.type == 240 || npc.type == 164 || npc.type == 165 || npc.type == 236 || npc.type == 237 || npc.type == 163 || npc.type == 238 || npc.type == 199) //Spidahs
                    {
                        ResetHealthValues = false;
                    }
                }
            }
            if (ResetHealthValues)
                NpcStatus.LastFrameHealth = NpcStatus.LastFrameMaxHealth = -1;
        }

        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            damage = (int)(damage * NpcStatus.HealthChangePercentage); //Test later
        }

        public byte[] GetPlayersToDivideReward(NPC npc)
        {
            List<byte> PlayersToGiveExp = new List<byte>();
            for (byte p = 0; p < 255; p++)
            {
                if (Main.player[p].active && npc.playerInteraction[p])
                {
                    //if(!PlayersToGiveExp.Contains(p))
                    //    PlayersToGiveExp.Add(p);
                    foreach(byte p2 in PlayerMod.GetPlayerTeamMates(Main.player[p], 1200 + 40, 1200 + 56))
                    {
                        if(!PlayersToGiveExp.Contains(p2))
                        {
                            PlayersToGiveExp.Add(p2);
                        }
                    }
                    /*for (byte p2 = 0; p2 < 255; p2++)
                    {
                        if (p2 != p && !PlayersToGiveExp.Contains(p2) &&
                            Main.player[p2].active && !Main.player[p2].dead && (
                            (Main.player[p2].team == 0 && Main.player[p].team == 0 && !Main.player[p].hostile && !Main.player[p2].hostile) ||
                            Main.player[p2].team == Main.player[p].team) &&
                            Math.Abs(Main.player[p2].Center.X - Main.player[p].Center.X) < 1200 + 40 &&
                            Math.Abs(Main.player[p2].Center.Y - Main.player[p].Center.Y) < 1200 + 56)
                        {
                            PlayersToGiveExp.Add(p2);
                        }
                    }*/
                }
            }
            return PlayersToGiveExp.ToArray();
        }

        private static byte NpcLuckRewardValue = 0;

        public override bool CheckDead(NPC npc)
        {
            NpcLuckRewardValue = 0;
            float LuckFactor = GetLuckFactorFromAttackers(npc);
            if (LuckFactor > 0)
            {
                if (MainMod.LuckStrike(LuckFactor, 40000))
                {
                    NpcLuckRewardValue = 2;
                    MainMod.TriggerLuckyClovers(npc.Center, true);
                    CombatText.NewText(npc.getRect(), Color.Green, "Very Luck!!", true);
                }
                else if (MainMod.LuckStrike(LuckFactor, 6000))
                {
                    NpcLuckRewardValue = 1;
                    CombatText.NewText(npc.getRect(), Color.Green, "Lucky!", true);
                    MainMod.TriggerLuckyClovers(npc.Center, false);
                }
                CheckingLootRate = false;
            }
            GiveExpToPlayers(npc);
            return base.CheckDead(npc);
        }

        public void GiveExpToPlayers(NPC npc)
        {
            if (Main.netMode == 1)
                return;
            byte[] PlayersToGiveExp = GetPlayersToDivideReward(npc);
            if (PlayersToGiveExp.Length > 0)
            {
                int ExpReward = NpcStatus.Exp;
                float ExpRate = 1f;
                if (PlayersToGiveExp.Length > 1)
                {
                    ExpRate = 1f / PlayersToGiveExp.Length + (PlayersToGiveExp.Length - 1 * 0.1f);
                }
                switch (NpcLuckRewardValue)
                {
                    case 1:
                        ExpRate *= 2;
                        break;
                    case 2:
                        ExpRate *= 8;
                        break;
                }
                ExpReward = (int)(ExpReward * ExpRate);
                if (ExpReward > 0)
                {
                    foreach (byte pos in PlayersToGiveExp)
                    {
                        PlayerMod p = Main.player[pos].GetModPlayer<PlayerMod>();
                        //Console.WriteLine("Sending exp to " + Main.player[pos].name);
                        int MyExp = ExpReward;
                        float ExpMult = MainMod.ExpRate + p.ExpBonus;
                        if (MainMod.ExpPenaltyByLevelDifference > 0)
                        {
                            int LevelDifference = p.GetGameModeInfo.Level - NpcStatus.Level;
                            if (LevelDifference > MainMod.ExpPenaltyByLevelDifference)
                                LevelDifference = MainMod.ExpPenaltyByLevelDifference;
                            if (LevelDifference < -MainMod.ExpPenaltyByLevelDifference)
                                LevelDifference = -MainMod.ExpPenaltyByLevelDifference;
                            float ExpMod = 1f - (1f / MainMod.ExpPenaltyByLevelDifference) * LevelDifference;
                            if (ExpMod < 0) ExpMod = 0;
                            if (ExpMod > 2) ExpMod = 2;
                            MyExp = (int)(MyExp * ExpMod);
                            if (MyExp == 0)
                                MyExp = 1;
                        }
                        p.GetExp(MyExp, ExpReceivedPopText.ExpSource.MobKill, p.player.whoAmI == Main.myPlayer, npc.getRect());
                    }
                }
            }
        }

        public float GetLuckFactorFromAttackers(NPC npc)
        {
            float LuckStack = 0;
            byte[] Players = GetPlayersToDivideReward(npc);
            foreach (byte p in Players)
            {
                PlayerMod pm = Main.player[p].GetModPlayer<PlayerMod>();
                LuckStack += pm.Luck;
            }
            if(Players.Length > 1)
            {
                LuckStack = LuckStack / Players.Length + LuckStack * 0.1f * (Players.Length - 1);
            }
            return LuckStack;
        }

        private static bool CheckingLootRate = false;

        public override bool PreNPCLoot(NPC npc)
        {
            if (CheckingLootRate)
                return true;
            if (!base.PreNPCLoot(npc))
                return false;
            float LuckFactor = GetLuckFactorFromAttackers(npc);
            if (LuckFactor > 0)
            {
                CheckingLootRate = true;
                if (NpcLuckRewardValue == 2)
                {
                    for (int i = 0; i < 7; i++)
                    {
                        npc.NPCLoot();
                    }
                }
                else if (NpcLuckRewardValue == 1)
                {
                    npc.NPCLoot();
                }
                CheckingLootRate = false;
            }
            return true;
        }

        public override void NPCLoot(NPC npc)
        {
            //GiveExpToPlayers();
            if (npc.type == ModContent.NPCType<NPCs.HighVoodooDemon>())
            {
                Item.NewItem(npc.getRect(), Terraria.ID.ItemID.GuideVoodooDoll);
            }
            if (MainMod.ZombiesDropsTombstones)
                TombstoneGenerator(npc, npc.target);
            NpcStatus.GameModeID = "";
        }
    }
}
