using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace NExperience
{
    public class GameModeSelector
    {
        public static bool Open = false;
        public static int SelectedGameMode = -1, ScrollY = 0;
        public static string[] GameModeIds = new string[0], GameModeTexts = new string[0];
        public static Vector2 WindowPosition = Vector2.Zero;
        public static int Width = 0, Height = 0;

        public static void OpenSelector()
        {
            Open = true;
            GameModeIds = MainMod.GetGameModeIDs;
            GameModeTexts = MainMod.GetGameModeNames;
            for (int i = 0; i < GameModeIds.Length; i++)
            {
                if (GameModeIds[i] == MainMod.FixedGameMode)
                    SelectedGameMode = i;
            }
            Width = (int)(Main.screenWidth * 0.5f);
            Height = (int)(Main.screenHeight * 0.5f);
            if (Width < 640) Width = 640;
            if (Height < 480) Height = 480;
            WindowPosition = new Vector2(Main.screenWidth - Width, Main.screenHeight - Height) * 0.5f;
            Main.playerInventory = false;
        }

        public static void UpdateAndDraw()
        {
            Vector2 DrawPosition = WindowPosition;
            Main.spriteBatch.Draw(Main.blackTileTexture, new Rectangle((int)DrawPosition.X - 2, (int)DrawPosition.Y - 2, Width + 4, Height + 4), Color.Black);
            if (Main.mouseX >= DrawPosition.X - 2 && Main.mouseX < DrawPosition.X + Width + 2 && Main.mouseY >= DrawPosition.Y - 2 && Main.mouseY < DrawPosition.Y + Height + 2)
                Main.player[Main.myPlayer].mouseInterface = true;
            Main.spriteBatch.Draw(Main.blackTileTexture, new Rectangle((int)DrawPosition.X, (int)DrawPosition.Y, Width, Height), Color.Gray);
            DrawPosition.X += 4;
            DrawPosition.Y += 4;
            const int ListWidth = 164;
            Main.spriteBatch.Draw(Main.blackTileTexture, new Rectangle((int)DrawPosition.X, (int)DrawPosition.Y, ListWidth, Height - 8), Color.LightSlateGray);
            int MaxYOptions = (int)((float)(DrawPosition.Y * 2 - 8) / 24);
            for (int i = 0; i < MaxYOptions; i++)
            {
                int index = i + ScrollY;
                if (index >= GameModeIds.Length)
                    break;
                Vector2 Position = DrawPosition;
                Position.Y += i * 24;
                string Text = "Option " + index;
                Text = GameModeTexts[index];
                Color c = (index == SelectedGameMode ? Color.Yellow : Color.White);
                if (DrawTextButton(Text, Position, c))
                {
                    SelectedGameMode = index;
                }
            }
            DrawPosition.X += ListWidth + 4;
            if (SelectedGameMode > -1)
            {
                GameModeBase gmb = MainMod.GetGameMode(GameModeIds[SelectedGameMode]);
                if (gmb == null)
                {
                    SelectedGameMode = -1;
                    return;
                }
                DrawText(gmb.Name, DrawPosition, Color.White, 1.2f);
                DrawPosition.Y += 28f;
                DrawPosition.Y +=  DrawText(gmb.Description, DrawPosition, Color.White).Y;
                DrawText("Max Level: " + gmb.MaxLevel, DrawPosition, Color.White);
                DrawPosition.Y += 24;
                DrawText(gmb.AllowLevelCapping ? "Allows Level Capping" : "Unallows Level Capping", DrawPosition, Color.White);
                DrawPosition.Y += 28;
                PlayerMod pm = Main.player[Main.myPlayer].GetModPlayer<PlayerMod>();
                if (!pm.HasGameModeData(gmb.GameModeID))
                {
                    DrawText("You've never played this game mode.", DrawPosition, Color.White);
                }
                else
                {
                    GameModeData gmd = pm.GetGameModeData(gmb.GameModeID);
                    DrawText("Level reached: " + gmd.Level, DrawPosition, Color.White);
                    DrawPosition.Y += 24;
                }
            }
            else
            {
                DrawPosition.X += (Width - 128) * 0.5f;
                DrawPosition.Y += (Height - 8) * 0.5f;
                Utils.DrawBorderString(Main.spriteBatch, "No Game Mode Selected", DrawPosition, Color.White, 1.5f, 0.5f, 0.5f);
            }
            {
                Vector2 SetButtonPosition = WindowPosition;
                SetButtonPosition.Y += Height - 28;
                if (GameModeIds[SelectedGameMode] != MainMod.FixedGameMode || WorldMod.WorldGameMode != GameModeIds[SelectedGameMode])
                {
                    SetButtonPosition.X += (Width + 128) * 0.5f;
                    if (DrawTextButton("Activate", SetButtonPosition, Color.White, 0.5f))
                    {
                        Open = false;
                        if (Main.netMode == 0)
                        {
                            WorldMod.WorldGameMode = GameModeIds[SelectedGameMode];
                            Main.NewText("Game mode changed to '" + MainMod.GetGameMode(WorldMod.WorldGameMode).Name + "'.");
                        }
                        else
                        {
                            NetPlayMod.ChangeGameMode(GameModeIds[SelectedGameMode]);
                        }
                    }
                }
                SetButtonPosition.X = WindowPosition.X + Width - 4;
                if (DrawTextButton("Close", SetButtonPosition, Color.White, 1f))
                {
                    Open = false;
                }
                if(SelectedGameMode > -1)
                {
                    Vector2 WikiButtonPosition = SetButtonPosition;
                    WikiButtonPosition.Y -= 30;
                    if(DrawTextButton("Wiki", WikiButtonPosition, Color.White, 1f))
                    {
                        GameModeBase gmb = MainMod.GetGameMode(GameModeIds[SelectedGameMode]);
                        string PageID = gmb.WikiPageID;
                        if (PageID == "")
                            PageID = gmb.Name;
                        PageID = "https://nakano15-mods.fandom.com/wiki/" + PageID;
                        System.Diagnostics.Process.Start(PageID);
                    }
                }
                SetButtonPosition.X = WindowPosition.X + ListWidth + 4;
                if (DrawTextButton("Dinok Mode [" + (WorldMod.IsDeathMode ? "ON" : "OFF") + "]", SetButtonPosition, Color.White, 0))
                {
                    Open = false;
                    if (Main.netMode == 0)
                    {
                        Main.NewText("Dinok Mode is " + (!WorldMod.IsDeathMode ?
                            "ON! Your character level wont pass Level 1." :
                            "OFF! Your character level will no longer be capped to 1."));
                        WorldMod.IsDeathMode = !WorldMod.IsDeathMode;
                        Main.PlaySound(Terraria.ID.SoundID.Roar, Main.player[Main.myPlayer].Center, 0);
                    }
                    else
                    {
                        NetPlayMod.SendDinokModeSwitch(!WorldMod.IsDeathMode);
                    }
                }
            }
        }

        public static Vector2 DrawText(string Text, Vector2 Position, Color color, float Scale = 1f, float anchorx = 0f)
        {
            return Utils.DrawBorderString(Main.spriteBatch, Text, Position, color, Scale, anchorx);
        }

        public static bool DrawTextButton(string Text, Vector2 Position, Color color, float anchorx = 0)
        {
            bool Clicked = false;
            Vector2 Bounds = Utils.DrawBorderString(Main.spriteBatch, Text, Position, Color.Transparent, 1f, anchorx);
            if (Main.mouseX >= Position.X - (Bounds.X * anchorx) && Main.mouseX < Position.X + (Bounds.X * (1f - anchorx)) && Main.mouseY >= Position.Y && Main.mouseY < Position.Y + Bounds.Y)
            {
                color = Color.Cyan;
                if (Main.mouseLeft && Main.mouseLeftRelease)
                {
                    Clicked = true;
                }
            }
            Utils.DrawBorderString(Main.spriteBatch, Text, Position, color, 1f, anchorx);
            return Clicked;
        }
    }
}
