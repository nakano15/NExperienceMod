using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using Terraria.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace NExperience
{
    public class LevelInfoInterface
    {
        public static bool Open = false;
        public const int DimensionX = 520, DimensionY = 240;
        private static bool IsFreeMode = false;
        private static int Scroll = 0;
        private static Color BackgroundColor = new Color(63, 63, 116);
        public static bool Hide = false;

        public static void Draw()
        {
            if (!Main.playerInventory)
            {
                if (Open)
                    Open = false;
                return;
            }
            if (Hide)
                return;
            Vector2 Position = new Vector2((Main.screenWidth - DimensionX) * 0.5f, Main.screenHeight - DimensionY);
            GameModeData Data = Main.player[Main.myPlayer].GetModPlayer<PlayerMod>().GetGameModeInfo;
            string MouseText = "";
            if (Open)
            {
                Main.spriteBatch.Draw(Main.blackTileTexture, new Rectangle((int)Position.X - 2, (int)Position.Y - 2, DimensionX + 4, DimensionY + 2), Color.Black);
                Main.spriteBatch.Draw(Main.blackTileTexture, new Rectangle((int)Position.X, (int)Position.Y, DimensionX, DimensionY), BackgroundColor);
                Main.player[Main.myPlayer].mouseInterface = Main.mouseX >= Position.X - 2 && Main.mouseX < Position.X + DimensionX + 2 &&
                    Main.mouseY >= Position.Y - 2 && Main.mouseY < Position.Y + DimensionY;
                IsFreeMode = !Main.gameMenu && Data.Base is GameModes.FreeMode;
                DrawLevelAndExp(ref Position, Data);
                DrawStatus(ref Position, ref MouseText, Data);
            }
            else
            {
                Vector2 NewPosition = Position;
                NewPosition.X += DimensionX - 48;
                string LevelText = Data.Base.LevelText(Data);
                NewPosition.Y = Main.screenHeight - 2 * 24f - 32;
                Utils.DrawBorderString(Main.spriteBatch, LevelText, NewPosition, Color.White, 1f, 0.5f);
                NewPosition.Y += 24f;
                LevelText = "Experience [" + Data.Exp + "/" + Data.MaxExp + " (" + Math.Round((float)Data.Exp / Data.MaxExp * 100, 2) + "%)]";
                Utils.DrawBorderString(Main.spriteBatch, LevelText, NewPosition, Color.White, 1f, 0.5f);
            }
            Vector2 OpenBtnPosition = Position;
            if (Open)
            {
                OpenBtnPosition.Y = Main.screenHeight - DimensionY - 32;
            }
            else
            {
                OpenBtnPosition.Y = Main.screenHeight - 32;
            }
            OpenBtnPosition.X += DimensionX - 96;
            if (Main.mouseX >= OpenBtnPosition.X && Main.mouseX < OpenBtnPosition.X + 96 &&
                Main.mouseY >= OpenBtnPosition.Y && Main.mouseY < OpenBtnPosition.Y + 32)
            {
                Main.player[Main.myPlayer].mouseInterface = true;
                if (Main.mouseLeft && Main.mouseLeftRelease)
                    Open = !Open;
            }
            else
            {
            }
            Main.spriteBatch.Draw(MainMod.StatusTextButtonTexture, OpenBtnPosition, Color.White);
            if (MouseText != "")
            {
                Utils.DrawBorderString(Main.spriteBatch, MouseText, new Vector2(Main.mouseX + 16, Main.mouseY + 16), Color.White);
            }
        }

        public static void DrawLevelAndExp(ref Vector2 Position, GameModeData data)
        {
            Vector2 NewPosition = Position;
            NewPosition.X += DimensionX * 0.5f;
            string Text = data.Base.LevelText(data);
            Utils.DrawBorderString(Main.spriteBatch, Text, NewPosition, Color.White, 1.1f, 0.5f);
            Position.Y += 30f;
            NewPosition.Y += 30 - 4;
            if (data.Level >= data.Base.MaxLevel && !MainMod.InfiniteLeveling)
            {
                Text = "Max Level";
            }
            else
            {
                Text = "Exp [" + data.Exp + "/" + data.MaxExp + "(" + Math.Round((float)data.Exp / data.MaxExp * 100, 2) + "%)]";
            }
            Utils.DrawBorderString(Main.spriteBatch, Text, NewPosition, Color.White, 0.95f, 0.5f);
            Position.Y += 18;
        }

        public static void DrawStatus(ref Vector2 Position, ref string MouseText, GameModeData data)
        {

            {
                Vector2 StatusTextPosition = Position;
                StatusTextPosition.Y -= 4;
                StatusTextPosition.X += DimensionX * 0.5f;
                Utils.DrawBorderString(Main.spriteBatch, "Status Points [" + data.StatusPoints + "]", StatusTextPosition, Color.White, 0.9f, 0.5f);
                Position.Y += 28;
            }
            float TotalStatusRows = data.Base.Status.Count * 0.5f;
            float MaxStatusRows = (Main.screenHeight - Position.Y) * (1f / 20) - 1;
            int PointsMult = 1;
            if (Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftShift))
                PointsMult = 10;
            bool HasPointsSpent = false;
            for (int y = 0; y < MaxStatusRows; y++)
            {
                for (int x = 0; x < 2; x++)
                {
                    byte index = (byte)(x + (y + Scroll) * 2);
                    if (index >= data.Base.Status.Count)
                        continue;
                    Vector2 StatusPosition = Position;
                    if (x == 1)
                        StatusPosition.X += DimensionX * 0.75f;
                    else
                        StatusPosition.X += DimensionX * 0.25f;
                    StatusPosition.Y -= 4;
                    StatusPosition.Y += 25 * y;
                    GameModeBase.StatusInfo statusbase = data.Base.Status[index];
                    int PointsChange = data.GetPointDifference(index), PointsSpent = data.GetPointsInvested(index), GetPointToSpend = data.GetPointsToSpend(index);
                    if (GetPointToSpend > 0)
                        HasPointsSpent = true;
                    string Text = statusbase.Name + " (" + PointsSpent + (PointsChange >= 0 ? "+" : "") + PointsChange + "+" + GetPointToSpend + "=" + data.GetPointsUnderEffect(index) + ")";
                    const float TextScale = 0.75f;
                    Vector2 Dim = Utils.DrawBorderString(Main.spriteBatch, Text, StatusPosition, Color.White, TextScale, 0.5f);
                    if (Main.mouseX >= StatusPosition.X - Dim.X * 0.5f && Main.mouseX < StatusPosition.X + Dim.X * 0.5f &&
                        Main.mouseY >= StatusPosition.Y + 4 && Main.mouseY < StatusPosition.Y + Dim.Y)
                    {
                        MouseText = statusbase.Description;
                    }
                    StatusPosition.X += Dim.X * 0.5f;
                    bool HadStatusPoint = false;
                    if (data.StatusPoints > 0)
                    {
                        HadStatusPoint = true;
                        Text = "[+"+PointsMult+"]";
                        Dim = Utils.DrawBorderString(Main.spriteBatch, Text, StatusPosition, Color.White, TextScale);
                        if (Main.mouseX >= StatusPosition.X && Main.mouseX < StatusPosition.X + Dim.X && Main.mouseY >= StatusPosition.Y + 4 && Main.mouseY < StatusPosition.Y + 4 + Dim.Y)
                        {
                            Utils.DrawBorderString(Main.spriteBatch, Text, StatusPosition, Color.Yellow, TextScale);
                            if (Main.mouseLeft && Main.mouseLeftRelease)
                            {
                                data.ChangePointsToSpend(index, PointsMult);
                            }
                        }
                        StatusPosition.X += Dim.X;
                    }
                    if (GetPointToSpend > 0)
                    {
                        if (HadStatusPoint)
                        {
                            StatusPosition.X += Utils.DrawBorderString(Main.spriteBatch, "\\", StatusPosition, Color.White, TextScale).X;
                        }
                        Text = "[-" + PointsMult + "]";
                        Dim = Utils.DrawBorderString(Main.spriteBatch, Text, StatusPosition, Color.White, TextScale);
                        if (Main.mouseX >= StatusPosition.X && Main.mouseX < StatusPosition.X + Dim.X && Main.mouseY >= StatusPosition.Y + 4 && Main.mouseY < StatusPosition.Y + Dim.Y)
                        {
                            Utils.DrawBorderString(Main.spriteBatch, Text, StatusPosition, Color.Yellow, TextScale);
                            if (Main.mouseLeft && Main.mouseLeftRelease)
                            {
                                data.ChangePointsToSpend(index, -PointsMult);
                            }
                        }
                    }
                }
            }
            Position.Y = Main.screenHeight - 24;
            if (HasPointsSpent)
            {
                Vector2 InvestPointsPos = Position;
                InvestPointsPos.X += DimensionX * 0.25f;
                Vector2 InvestPointsDim = Utils.DrawBorderString(Main.spriteBatch, "Spend Points", InvestPointsPos, Color.White, 1f, 0.5f);
                if (Main.mouseX >= InvestPointsPos.X - InvestPointsDim.X * 0.5f && Main.mouseX < InvestPointsPos.X + InvestPointsDim.X * 0.5 &&
                    Main.mouseY >= InvestPointsPos.Y + 4 && Main.mouseY < InvestPointsPos.Y + InvestPointsDim.Y)
                {
                    Utils.DrawBorderString(Main.spriteBatch, "Spend Points", InvestPointsPos, Color.Yellow, 1f, 0.5f);
                    if (Main.mouseLeft && Main.mouseLeftRelease)
                    {
                        data.InvestPoints();
                        NetPlayMod.SendPlayerStatus(Main.myPlayer, -1, Main.myPlayer);
                    }
                }
                InvestPointsPos.X += DimensionX * 0.5f;
                InvestPointsDim = Utils.DrawBorderString(Main.spriteBatch, "Clear Points", InvestPointsPos, Color.White, 1f, 0.5f);
                if (Main.mouseX >= InvestPointsPos.X - InvestPointsDim.X * 0.5f && Main.mouseX < InvestPointsPos.X + InvestPointsDim.X * 0.5 &&
                    Main.mouseY >= InvestPointsPos.Y + 4 && Main.mouseY < InvestPointsPos.Y + InvestPointsDim.Y)
                {
                    Utils.DrawBorderString(Main.spriteBatch, "Clear Points", InvestPointsPos, Color.Yellow, 1f, 0.5f);
                    if (Main.mouseLeft && Main.mouseLeftRelease)
                    {
                        data.ResetPointsToSpend();
                    }
                }
            }
            else
            {
                Vector2 GameModeButtonPos = Position;
                GameModeButtonPos.X += DimensionX * 0.5f;
                Vector2 InvestPointsDim = Utils.DrawBorderString(Main.spriteBatch, "Change Game Mode", GameModeButtonPos, Color.White, 1f, 0.5f);
                if (Main.mouseX >= GameModeButtonPos.X - InvestPointsDim.X * 0.5f && Main.mouseX < GameModeButtonPos.X + InvestPointsDim.X * 0.5 &&
                    Main.mouseY >= GameModeButtonPos.Y + 4 && Main.mouseY < GameModeButtonPos.Y + InvestPointsDim.Y)
                {
                    Utils.DrawBorderString(Main.spriteBatch, "Change Game Mode", GameModeButtonPos, Color.Yellow, 1f, 0.5f);
                    if (Main.mouseLeft && Main.mouseLeftRelease)
                    {
                        GameModeSelector.OpenSelector();
                        Open = false;
                    }
                }
            }
        }
    }
}
