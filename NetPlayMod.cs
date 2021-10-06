using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace NExperience
{
    public class NetPlayMod
    {
        public static ModPacket packet { get { return MainMod.packet; } }

        public static void ResetPacket()
        {
            MainMod.ResetPacket();
        }

        public static void ReceivedMessages(System.IO.BinaryReader reader, int Me)
        {
            MessageType msgType = (MessageType)reader.ReadByte();
            switch (msgType)
            {
                case MessageType.SendPlayerLevel:
                    {
                        int player = reader.ReadByte();
                        string GameModeID = reader.ReadString();
                        int Level = reader.ReadInt32();
                        if (player == Main.myPlayer)
                            return;
                        GameModeData gamemode = Main.player[player].GetModPlayer<PlayerMod>().GetGameModeData(GameModeID);
                        gamemode.Level = Level;
                        gamemode.Level2 = -999;
                        gamemode.RecalcStatus = true;
                        if(Main.netMode == 2)
                        {
                            SendPlayerLevel(player, -1, player);
                        }
                    }
                    break;
                case MessageType.SendPlayerStatus:
                    {
                        int player = reader.ReadByte();
                        string GameModeID = reader.ReadString();
                        Dictionary<byte, int> StatusPoints = new Dictionary<byte, int>();
                        GameModeData gmd = Main.player[player].GetModPlayer<PlayerMod>().GetGameModeData(GameModeID);
                        byte TotalStatusToSync = reader.ReadByte();
                        while (StatusPoints.Count < TotalStatusToSync)
                        {
                            StatusPoints.Add(reader.ReadByte(), reader.ReadInt32());
                        }
                        if (player == Main.myPlayer)
                            return;
                        gmd.PointsSpent = StatusPoints;
                        gmd.RecalcStatus = true;
                        if (Main.netMode == 2)
                        {
                            SendPlayerStatus(player, -1, player);
                        }
                    }
                    break;
                case MessageType.ReceiveExp:
                    {
                        int PlayerID = (int)reader.ReadByte();
                        int Exp = reader.ReadInt32();
                        ExpReceivedPopText.ExpSource source = (ExpReceivedPopText.ExpSource)reader.ReadByte();
                        if (Main.netMode == 1)
                        {
                            if(PlayerID == Main.myPlayer)
                                Main.player[Main.myPlayer].GetModPlayer<PlayerMod>().GetExp(Exp, source, true, null);
                        }
                        else if (Main.netMode == 2)
                            SendExpToPlayer(PlayerID, Exp, source, Me);
                    }
                    break;
                case MessageType.SendNpcLevel:
                    {
                        int NpcPos = reader.ReadByte();
                        int Level = reader.ReadInt32();
                        NPC npc = Main.npc[NpcPos];
                        if (npc.active)
                        {
                            NpcMod npcMod = Main.npc[NpcPos].GetGlobalNPC<NpcMod>(); //It's giving error around here...
                            npcMod.NpcStatus.Level = Level;
                            float NpcHealthValue = (npc.life == npc.lifeMax ? 1f : (float)npc.life / npc.lifeMax);
                            npcMod.NpcStatus.UpdateNpc(npc);
                            npc.life = (int)(npc.lifeMax * NpcHealthValue);
                        }
                    }
                    break;
                case MessageType.AskForGameMode:
                    {
                        int Player = reader.ReadByte();
                        if(Main.netMode == 2)
                        {
                            SendGameMode(Player);
                            SendDinokModeSwitch(WorldMod.IsDeathMode);
                        }
                    }
                    break;
                case MessageType.ChangeGameMode:
                    {
                        string GameModeID = reader.ReadString();
                        if (MainMod.HasGameModeID(GameModeID))
                        {
                            string LastGameMode = WorldMod.WorldGameMode;
                            WorldMod.WorldGameMode = GameModeID;
                            if (LastGameMode != GameModeID)
                            {
                                string T = "Game Mode set to '" + GameModeID + "'.";
                                if (Main.netMode == 1)
                                {
                                    Main.NewText(T);
                                }
                                else
                                {
                                    Console.WriteLine(T);
                                }
                            }
                            if (Main.netMode == 2)
                                ChangeGameMode(GameModeID, -1, Me);
                            else
                            {
                                SendPlayerLevel(Main.myPlayer, -1, Main.myPlayer);
                                SendPlayerStatus(Main.myPlayer, -1, Main.myPlayer);
                            }
                        }
                        else
                        {
                            string T = "Received invalid game mode id: '" + GameModeID + "'.";
                            if (Main.netMode == 1)
                            {
                                Main.NewText(T);
                            }
                            else
                            {
                                Console.WriteLine(T);
                            }
                        }
                    }
                    break;
                case MessageType.SendGameMode:
                    {
                        string GameModeID = reader.ReadString();
                        bool DeathMode = reader.ReadBoolean();
                        if (MainMod.HasGameModeID(GameModeID))
                        {
                            string LastGameMode = WorldMod.WorldGameMode;
                            WorldMod.WorldGameMode = GameModeID;
                            if (LastGameMode != WorldMod.WorldGameMode)
                            {
                                string T = "Game Mode set to '" + GameModeID + "'.";
                                if (Main.netMode == 1)
                                {
                                    Main.NewText(T);
                                }
                                else
                                {
                                    Console.WriteLine(T);
                                }
                            }
                            bool LastWasDeathMode = WorldMod.IsDeathMode;
                            WorldMod.IsDeathMode = DeathMode;
                            if (LastWasDeathMode != WorldMod.IsDeathMode)
                            {
                                string T = "Dinok Mode [" + (WorldMod.IsDeathMode ? "ON" : "OFF") + "]";
                                if (Main.netMode == 1)
                                {
                                    Main.NewText(T);
                                    Main.PlaySound(Terraria.ID.SoundID.Roar, Main.player[Main.myPlayer].Center, 0);
                                }
                                else
                                {
                                    Console.WriteLine(T);
                                }
                            }
                            if (Main.netMode == 1)
                            {
                                Main.player[Main.myPlayer].GetModPlayer<PlayerMod>().GetGameModeInfo.Level2 = -1;
                                Main.player[Main.myPlayer].GetModPlayer<PlayerMod>().GetGameModeInfo.UpdatePlayer(Main.player[Main.myPlayer]);
                                SendPlayerLevel(Main.myPlayer, -1, Main.myPlayer);
                                SendPlayerStatus(Main.myPlayer, -1, Main.myPlayer);
                            }
                        }
                        else
                        {
                            string T = "Server tried to sync invalid game mode id: '" + GameModeID + "'.";
                            if (Main.netMode == 1)
                                Main.NewText(T, Color.Red);
                            else
                                Console.WriteLine(T);
                        }
                    }
                    break;
                case MessageType.SendDinokModeSwitch:
                    {
                        bool Activate = reader.ReadBoolean();
                        bool LastWasDeathMode = WorldMod.IsDeathMode;
                        WorldMod.IsDeathMode = Activate;
                        if (LastWasDeathMode != WorldMod.IsDeathMode)
                        {
                            if (WorldMod.IsDeathMode)
                            {
                                if (Main.netMode == 1)
                                {
                                    Main.NewText("Dinok Mode [ON]", Color.Red);
                                    Main.PlaySound(Terraria.ID.SoundID.Roar, Main.player[Main.myPlayer].Center, 0);
                                }
                                else
                                {
                                    Console.WriteLine("Dinok Mode [ON]");
                                }
                            }
                            else
                            {
                                if (Main.netMode == 1)
                                {
                                    Main.NewText("Dinok Mode [OFF]", Color.Green);
                                }
                                else
                                {
                                    Console.WriteLine("Dinok Mode [OFF]");
                                }
                            }
                        }
                        if (Main.netMode == 1)
                        {
                            Main.player[Main.myPlayer].GetModPlayer<PlayerMod>().GetGameModeInfo.Level2 = 0;
                            Main.player[Main.myPlayer].GetModPlayer<PlayerMod>().GetGameModeInfo.UpdatePlayer(Main.player[Main.myPlayer]);
                            SendPlayerLevel(Main.myPlayer, -1, Main.myPlayer);
                            SendPlayerStatus(Main.myPlayer, -1, Main.myPlayer);
                        }
                        else if (Main.netMode == 2)
                        {
                            SendDinokModeSwitch(WorldMod.IsDeathMode, -1, Me);
                        }
                    }
                    break;

                case MessageType.SendTriggeredLuckyClovers:
                    {
                        Vector2 Position = Vector2.Zero;
                        Position.X = reader.ReadSingle();
                        Position.Y = reader.ReadSingle();
                        bool VeryLucky = reader.ReadBoolean();
                        MainMod.TriggerLuckyClovers(Position, VeryLucky);
                    }
                    break;

                case MessageType.SendPlayerHealth:
                    {
                        byte Player = reader.ReadByte();
                        int Health = reader.ReadInt32();
                        int MaxHealth = reader.ReadInt32();
                        if (Player == Main.myPlayer)
                            return;
                        if (Main.netMode == 2)
                            Player = (byte)Me;
                        Main.player[Player].statLife = Health;
                        Main.player[Player].statLifeMax = MaxHealth;
                        Main.player[Player].dead = Main.player[Player].statLife <= 0;
                        if(Main.netMode == 2)
                        {
                            SendPlayerHealth(Player);
                        }
                    }
                    break;

                case MessageType.SendNpcInfos:
                    {
                        if (Main.netMode != 1)
                            return;
                        short NpcPos = reader.ReadInt16();
                        int NetID = reader.ReadInt32();
                        Vector2 Position = reader.ReadVector2();
                        Vector2 Velocity = reader.ReadVector2();
                        int Target = reader.ReadInt16();
                        if (Target < 0)
                            Target = 0;
                        NPC npc = Main.npc[NpcPos];
                        if (!(npc.active = reader.ReadBoolean()))
                        {
                            return;
                        }
                        if (NetID > -1 && NetID != npc.netID)
                            npc.TransformVisuals(NetID, npc.netID);
                        if(npc.netID != NetID)
                            npc.SetDefaults(NetID);
                        if(Vector2.DistanceSquared(npc.position, Position) < 6400f)
                        {
                            npc.visualOffset = npc.position - Position;
                        }
                        npc.position = Position;
                        npc.velocity = Velocity;
                        npc.target = Target;
                        BitsByte bb = reader.ReadByte();
                        int Level = reader.ReadInt32();
                        GameModeData status = npc.GetGlobalNPC<NpcMod>().NpcStatus;
                        status.Level2 = -999;
                        status.Level = Level;
                        status.UpdateNpc(npc);
                        if (bb[7])
                            npc.life = npc.lifeMax;
                        else
                            npc.life = reader.ReadInt32();
                        npc.direction = bb[0] ? 1 : -1;
                        npc.directionY = bb[1] ? 1 : -1;
                        npc.spriteDirection = bb[6] ? 1 : -1;
                        for(int i = 0; i < NPC.maxAI; i++)
                        {
                            if (bb[2 + i])
                                npc.ai[i] = reader.ReadSingle();
                            else
                                npc.ai[i] = 0;
                        }
                        if (npc.type >= 0 && Main.npcCatchable[npc.type])
                            npc.releaseOwner = reader.ReadByte();
                        if (npc.type == 262)
                            NPC.plantBoss = NpcPos;
                        if (npc.type == 245)
                            NPC.golemBoss = NpcPos;
                        NPCLoader.ReceiveExtraAI(npc, reader);
                    }
                    break;

                case MessageType.SendMessageToServer:
                    {
                        string Message = reader.ReadString();
                        Color color = reader.ReadRGB();
                        if (Main.netMode != 2)
                            return;
                        NetMessage.SendData(25, -1, Me, Terraria.Localization.NetworkText.FromLiteral(Message),
                            color.R, color.G, color.B, color.A);
                    }
                    break;
            }
        }

        public static void SendPlayerLevel(int PlayerID, int ToWho = -1, int FromWho = -1)
        {
            if (Main.netMode == 0)
                return;
            ResetPacket();
            PlayerMod player = Main.player[PlayerID].GetModPlayer<PlayerMod>();
            MainMod.packet.Write((byte)MessageType.SendPlayerLevel);
            MainMod.packet.Write((byte)PlayerID);
            MainMod.packet.Write(player.GetGameModeInfo.GameModeID);
            MainMod.packet.Write(player.GetGameModeInfo.Level);
            MainMod.packet.Send(ToWho, FromWho);
        }

        public static void SendPlayerStatus(int PlayerID, int ToWho = -1, int FromWho = -1)
        {
            if (Main.netMode == 0)
                return;
            ResetPacket();
            PlayerMod player = Main.player[PlayerID].GetModPlayer<PlayerMod>();
            MainMod.packet.Write((byte)MessageType.SendPlayerStatus);
            MainMod.packet.Write((byte)PlayerID);
            GameModeData gamemode = player.GetGameModeInfo;
            MainMod.packet.Write(gamemode.GameModeID);
            Dictionary<byte, int> Points = gamemode.PointsSpent;
            MainMod.packet.Write((byte)Points.Count);
            foreach (byte StatusID in gamemode.PointsSpent.Keys)
            {
                MainMod.packet.Write(StatusID);
                MainMod.packet.Write(gamemode.PointsSpent[StatusID]);
            }
            MainMod.packet.Send(ToWho, FromWho);
        }

        public static void SendExpToPlayer(int PlayerID, int Exp, ExpReceivedPopText.ExpSource source, int FromWho = -1)
        {
            if (Main.netMode == 0)
                return;
            ResetPacket();
            MainMod.packet.Write((byte)MessageType.ReceiveExp);
            MainMod.packet.Write((byte)PlayerID);
            MainMod.packet.Write(Exp);
            MainMod.packet.Write((byte)source);
            MainMod.packet.Send(PlayerID, FromWho);
        }

        public static void SyncNPCLevels(int Npc, int ToWho = -1, int FromWho = -1)
        {
            if (Main.netMode == 0)
                return;
            if (Npc < 0 || Npc >= Main.npc.Length)
                return;
            ResetPacket();
            packet.Write((byte)MessageType.SendNpcLevel);
            packet.Write((byte)Npc);
            packet.Write(Main.npc[Npc].GetGlobalNPC<NpcMod>().NpcStatus.Level);
            packet.Send(ToWho, FromWho);
        }

        public static void AskForGameMode(int SendTo = -1)
        {
            if (Main.netMode == 0)
                return;
            ResetPacket();
            if (SendTo == -1) SendTo = Main.myPlayer;
            packet.Write((byte)MessageType.AskForGameMode);
            packet.Write((byte)SendTo);
            MainMod.packet.Send(-1, -1);
        }

        public static void SendGameMode(int ToWho = -1, int FromWho = -1)
        {
            if (Main.netMode == 0)
                return;
            ResetPacket();
            packet.Write((byte)MessageType.SendGameMode);
            GameModeBase gmb = MainMod.GetCurrentGameMode;
            packet.Write(gmb.GameModeID);
            packet.Write(WorldMod.IsDeathMode);
            MainMod.packet.Send(ToWho, FromWho);
        }

        public static void ChangeGameMode(string GameModeID, int ToWho = -1, int FromWho = -1)
        {
            if (Main.netMode == 0)
                return;
            ResetPacket();
            packet.Write((byte)MessageType.ChangeGameMode);
            packet.Write(GameModeID);
            MainMod.packet.Send(ToWho, FromWho);
        }

        public static void SendDinokModeSwitch(bool Activate, int FromWho = -1, int ToWho = -1)
        {
            if (Main.netMode == 0)
                return;
            ResetPacket();
            packet.Write((byte)MessageType.SendDinokModeSwitch);
            packet.Write(Activate);
            packet.Send(ToWho, FromWho);
        }

        public static void SendLuckyClovers(Vector2 Position, bool VeryLucky)
        {
            if (Main.netMode == 0)
                return;
            ResetPacket();
            packet.Write((byte)MessageType.SendTriggeredLuckyClovers);
            packet.Write(Position.X);
            packet.Write(Position.Y);
            packet.Write(VeryLucky);
            packet.Send(-1, -1);
        }

        public static void SendPlayerHealth(byte Player)
        {
            if (Main.netMode == 0)
                return;
            ResetPacket();
            packet.Write((byte)MessageType.SendPlayerHealth);
            packet.Write(Player);
            packet.Write(Main.player[Player].statLife);
            packet.Write(Main.player[Player].statLifeMax);
            packet.Send(-1, Player);
        }

        public static void SendNpcInfos(byte Npc)
        {
            if (Main.netMode == 0)
                return;
            NPC npc = Main.npc[Npc];
            ResetPacket();
            packet.Write((byte)MessageType.SendNpcInfos);
            packet.Write((short)Npc);
            packet.Write(npc.netID);
            packet.WriteVector2(npc.position);
            packet.WriteVector2(npc.velocity);
            packet.Write((short)npc.target);
            if (!npc.active)
            {
                packet.Write(false);
                packet.Send(-1, Main.myPlayer);
                return;
            }
            else
                packet.Write(true);
            BitsByte bb = 0;
            bb[0] = npc.direction > 0;
            bb[1] = npc.directionY > 0;
            bb[2] = npc.ai[0] != 0;
            bb[3] = npc.ai[1] != 0;
            bb[4] = npc.ai[2] != 0;
            bb[5] = npc.ai[3] != 0;
            bb[6] = npc.spriteDirection > 0;
            bb[7] = npc.life == npc.lifeMax;
            packet.Write(bb);
            packet.Write(npc.GetGlobalNPC<NpcMod>().NpcStatus.Level);
            if (npc.life < npc.lifeMax)
                packet.Write(npc.life);
            for(int i = 0; i < NPC.maxAI; i++)
            {
                if(npc.ai[i] != 0)
                {
                    packet.Write(npc.ai[i]);
                }
            }
            if(npc.type >= 0 && Main.npcCatchable[npc.type])
            {
                packet.Write((byte)npc.releaseOwner);
            }
            NPCLoader.SendExtraAI(npc, packet);
            packet.Send(-1, Main.myPlayer);
        }

        public static void SendMessageToServer(string Message, Color color, int IgnorePlayer = -1)
        {
            if (Main.netMode == 0)
                return;
            ResetPacket();
            packet.Write((byte)MessageType.SendNpcInfos);
            packet.Write(Message);
            packet.WriteRGB(color);
            packet.Send(-1, IgnorePlayer);
        }

        public enum MessageType
        {
            SendPlayerLevel,
            SendPlayerStatus,
            ReceiveExp,
            AskForGameMode,
            SendGameMode,
            SendNpcLevel,
            SendDinokModeSwitch,
            SendTriggeredLuckyClovers,
            ChangeGameMode,
            SendPlayerHealth,
            SendNpcInfos,
            SendMessageToServer
        }
    }
}
