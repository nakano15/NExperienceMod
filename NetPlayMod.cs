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
                        int Level = reader.ReadInt32();
                        Main.player[player].GetModPlayer<PlayerMod>().GetGameModeInfo.Level = Level;
                    }
                    break;
                case MessageType.SendPlayerStatus:
                    {
                        int player = reader.ReadByte();
                        Dictionary<byte, int> StatusPoints = new Dictionary<byte, int>();
                        GameModeData gmd = Main.player[player].GetModPlayer<PlayerMod>().GetGameModeInfo;
                        byte TotalStatusToSync = reader.ReadByte();
                        while (StatusPoints.Count < TotalStatusToSync)
                        {
                            StatusPoints.Add(reader.ReadByte(), reader.ReadInt32());
                        }
                        if (player == Main.myPlayer)
                            return;
                        gmd.PointsSpent = StatusPoints;
                        gmd.RecalcStatus = true;
                    }
                    break;
                case MessageType.ReceiveExp:
                    {
                        int Exp = reader.ReadInt32();
                        ExpReceivedPopText.ExpSource source = (ExpReceivedPopText.ExpSource)reader.ReadByte();
                        if(Main.netMode == 1)
                            Main.player[Main.myPlayer].GetModPlayer<PlayerMod>().GetExp(Exp, source, true, null);
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
                                ChangeGameMode(GameModeID);
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
                                Main.player[Main.myPlayer].GetModPlayer<PlayerMod>().GetGameModeInfo.Level2 = 0;
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
                            SendDinokModeSwitch(WorldMod.IsDeathMode);
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
            Dictionary<byte, int> Points = player.GetGameModeInfo.PointsSpent;
            MainMod.packet.Write((byte)Points.Count);
            foreach (byte StatusID in player.GetGameModeInfo.PointsSpent.Keys)
            {
                MainMod.packet.Write(StatusID);
                MainMod.packet.Write(player.GetGameModeInfo.PointsSpent[StatusID]);
            }
            MainMod.packet.Send(ToWho, FromWho);
        }

        public static void SendExpToPlayer(int PlayerID, int Exp, ExpReceivedPopText.ExpSource source, int FromWho = -1)
        {
            if (Main.netMode == 0)
                return;
            ResetPacket();
            MainMod.packet.Write((byte)MessageType.ReceiveExp);
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
            ChangeGameMode
        }
    }
}
