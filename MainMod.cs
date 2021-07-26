using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using Terraria.UI;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Localization;
using System.IO;

namespace NExperience
{
    public class MainMod : Mod
    {
        public static string FixedGameMode { get { return WorldMod.WorldGameMode; } }
        public static bool LevelCapping = false, MobDefenseConvertToHealth = false, ShowExpAsPercentage = true, InfiniteLeveling = false, PotionsForSale = false, CapLevelOnInfiniteLeveling = false, BossesAsToughasMe = false, FullRegenOnLevelUp = false, Playthrough1dot5OnEndgame = false,
            BuffPreHardmodeEnemiesOnHardmode = false, ZombiesDropsTombstones = true, DisableModEnemies = false, LevelInfoOnScreenCorner = false, BiomeTextFades = false, ItemStatusCapper = false, AllowManaBoosts = false;
        public static bool TestMultiplayerSync = false;
        private static Dictionary<string, GameModeBase> GameModeList = new Dictionary<string, GameModeBase>();
        public static Texture2D LevelingArrow, LevelReductionArrow, StatusTextButtonTexture;
        public static int LastMinLevel, LastMaxLevel;
        public const int ModVersion = 4;
        public static int NpcProjSpawnPos = -1;
        public static float DefaultExpRate = 1f;
        public static float ExpRate = 1f;
        public static int ExpPenaltyByLevelDifference = 0;
        public static int LastHighestLeveledPlayer = 1, LastHighestLeveledNpc = 1, TempHighestLeveledPlayer = 1, TempHighestLeveledNpc = 1;
        public static int BiomeTextFadeTime = 0;
        public const int FadeDuration = 60, DisplayDuration = 180;
        public static ModPacket packet
        {
            get
            {
                if (_packet == null)
                {
                    _packet = mod.GetPacket();
                }
                return _packet;
            }
        }
        private static ModPacket _packet;
        public static void ResetPacket()
        {
            _packet = mod.GetPacket();
        }
        public static MainMod mod;
        public static bool WeekendEventUp
        {
            get
            {
                return (Date.DayOfWeek == DayOfWeek.Friday && Date.Hour > 12) || Date.DayOfWeek == DayOfWeek.Saturday || Date.DayOfWeek == DayOfWeek.Sunday;
            }
        }
        public static bool TheDayEvent
        {
            get
            {
                return Date.Day == 20 && Date.Month == 2;
            }
        }
        public static bool TerrariaBirthdayEvent
        {
            get
            {
                return Date.Day == 16 && Date.Month == 5;
            }
        }
        public static bool LastWeekendEventUp = false, LastThatDayEvent = false, LastTerrariaBirthday = false;
        public static bool WarnGameModeChange = false;
        public static Texture2D LuckyCloverTexture, VeryLuckyCloverTexture;
        public static int LastInvasionEvent = 0, LastEventWave = 0;
        public static int DeathExpPenalty = 5;
        public static int MaxExpPenaltyStack = 60;
        private static int AfkPenaltyCounter = 0;
        public static float AfkPenaltyPercent
        {
            get
            {
                return (int)(AfkPenaltyDecimal * 100);
            }
        }
        private static DateTime Date = new DateTime();

        public static float AfkPenaltyDecimal
        {
            get
            {
                if (MaxExpPenaltyStack == 0)
                    return 0;
                float Penalty = (int)(AfkPenaltyCounter * (1f / 18000)) * 0.05f; //Every 5 minutes
                if (Penalty > MaxExpPenaltyStack * 0.01f)
                    Penalty = MaxExpPenaltyStack * 0.01f;
                return Penalty;
            }
        }
        public static List<ExpReceivedPopText> ExpReceived = new List<ExpReceivedPopText>();

        public static string[] GetGameModeNames
        {
            get
            {
                List<string> Names = new List<string>();
                foreach (string key in GameModeList.Keys.ToArray())
                {
                    Names.Add(GameModeList[key].Name);
                }
                return Names.ToArray();
            }
        }

        public static string[] GetGameModeIDs
        {
            get { return GameModeList.Keys.ToArray(); }
        }

        public override void ModifyInterfaceLayers(List<Terraria.UI.GameInterfaceLayer> layers)
        {
            const int InventoryLayer = 27;
            LegacyGameInterfaceLayer lmi = new LegacyGameInterfaceLayer("NExperience: Gameplay Hud", DrawGameplayHudInterface, InterfaceScaleType.UI),
                delc = new LegacyGameInterfaceLayer("NExperience: Entity Level Interface", DrawEntityLevelCheck, InterfaceScaleType.UI),
                dgms = new LegacyGameInterfaceLayer("NExperience: Game Mode Selector", CallGameModeSelector, InterfaceScaleType.UI),
                lii = new LegacyGameInterfaceLayer("NExperience: Level Interface Info", DrawLevelInfoInterface, InterfaceScaleType.UI),
                cl = new LegacyGameInterfaceLayer("NExperience: Lucky Clovers Layer", DrawClovers, InterfaceScaleType.UI),
                api = new LegacyGameInterfaceLayer("NExperience: Afk Penalty Interface", DrawAfkPenaltyInterface, InterfaceScaleType.UI),
                deri = new LegacyGameInterfaceLayer("NExperience: Exp Received Info", DrawExpReceivedInterface, InterfaceScaleType.UI);
            layers.Insert(0, cl);
            layers.Insert(InventoryLayer, deri);
            layers.Insert(InventoryLayer, api);
            layers.Insert(InventoryLayer, lmi);
            layers.Insert(InventoryLayer, delc);
            layers.Insert(InventoryLayer, lii);
            if (GameModeSelector.Open)
            {
                layers.Insert(InventoryLayer, dgms);
            }
        }

        public bool DrawExpReceivedInterface()
        {
            Vector2 Position = new Vector2(210, Main.screenHeight * 0.6f);
            float GlobalColorPercentage = 1f;
            foreach(ExpReceivedPopText erpt in ExpReceived)
            {
                string Text = erpt.GetText;
                Color color = Color.White;
                if(erpt.TextDuration < 30)
                {
                    color *= (erpt.TextDuration * (1f / 30));
                }
                Utils.DrawBorderString(Main.spriteBatch, Text, Position, color * GlobalColorPercentage, 0.8f);
                GlobalColorPercentage -= 0.1f;
                Position.Y += 17f;
            }
            return true;
        }

        public static void UpdateExpReceivedPopText(ExpReceivedPopText.ExpSource Source, int NewExp, PlayerMod player)
        {
            for(int e = 0; e < ExpReceived.Count; e++)
            {
                if(ExpReceived[e].Source == Source)
                {
                    ExpReceived[e].UpdateExp(NewExp, player);
                    ExpReceived.OrderByDescending(x => x.TextDuration);
                    return;
                }
            }
            ExpReceivedPopText expReceivedPopText = new ExpReceivedPopText(Source, NewExp, player);
            ExpReceived.Insert(0, expReceivedPopText);
        }

        public void UpdateExpReceivedTexts()
        {
            for (int e = 0; e < ExpReceived.Count; e++)
            {
                if (ExpReceived[e].TextDuration-- <= 0)
                    ExpReceived.RemoveAt(e);
            }
        }

        public bool DrawAfkPenaltyInterface()
        {
            float PenaltyValue = AfkPenaltyPercent;
            if (PenaltyValue == 0)
                return true;
            Vector2 Position = new Vector2(Main.screenWidth * 0.5f, Main.screenHeight * 0.7f);
            float Scale = 0.4f + 0.6f * ((1f / 60) * PenaltyValue);
            Utils.DrawBorderStringBig(Main.spriteBatch, "Afk Penalty: " + PenaltyValue + "%", Position, Color.Red, 1f, 0.5f, 0.5f);
            return true;
        }

        public bool DrawLevelInfoInterface()
        {
            LevelInfoInterface.Draw();
            return true;
        }

        public static bool LuckStrike(float LuckValue, float LuckChance)
        {
            if (LuckValue == 0)
                return false;
            return Main.rand.NextFloat() < LuckValue / LuckChance;
        }

        public override void MidUpdatePlayerNPC()
        {
            NpcMod.HasNocturnalBossSpawned = false;
        }

        public override void MidUpdateDustTime()
        {
            if (WorldMod.IsDeathMode && NpcMod.HasNocturnalBossSpawned)
            {
                const double Midnight = 4.5 * 3600;
                if (!Main.dayTime && Main.time > Midnight)
                    Main.time = Midnight;
            }
        }

        public override void PostUpdateEverything()
        {
            LastHighestLeveledPlayer = TempHighestLeveledPlayer;
            LastHighestLeveledNpc = TempHighestLeveledNpc;
            TempHighestLeveledNpc = TempHighestLeveledPlayer = 1;
            ExpRate = DefaultExpRate;
            Date = DateTime.Now;
            if (!Main.gameMenu)
            {
                bool WeekendEventRunning = WeekendEventUp;
                bool ThatDayEventIsRunning = TheDayEvent;
                if (WeekendEventRunning)
                {
                    ExpRate += 0.5f;
                    if (!LastWeekendEventUp)
                    {
                        Main.NewText("Weekend exp bonus is running. Enjoy.");
                    }
                    LastWeekendEventUp = true;
                }
                else
                {
                    if (LastWeekendEventUp)
                    {
                        Main.NewText("Weekend exp bonus ended. See you next week.");
                    }
                    LastWeekendEventUp = false;
                }
                if (ThatDayEventIsRunning)
                {
                    ExpRate += 1f;
                    if (!LastThatDayEvent)
                    {
                        Main.NewText("You are receiving benefit by that day. Enjoy.");
                    }
                    LastThatDayEvent = true;
                }
                else
                {
                    if (LastThatDayEvent)
                    {
                        Main.NewText("That day has ended. Exp bonus removed.");
                    }
                    LastThatDayEvent = false;
                }
                if (TerrariaBirthdayEvent)
                {
                    ExpRate += 0.3f;
                    if (!LastTerrariaBirthday)
                    {
                        DateTime Age = DateTime.Now;
                        Age.Subtract(new DateTime(2011, 05, 16));
                        Main.NewText("Terraria turns " + Age.Year + " years old today! Enjoy the bonus exp.");
                    }
                    LastTerrariaBirthday = true;
                }
                else
                {
                    if (LastTerrariaBirthday)
                    {
                        Main.NewText("I hope you had a good time.");
                    }
                    LastTerrariaBirthday = false;
                }
            }
            CheckForInvasionEventCompletion();
            UpdateAfkPenalty();
            UpdateExpReceivedTexts();
        }
        
        public void UpdateAfkPenalty()
        {
            if (!Main.gameMenu)
            {
                if(Main.player[Main.myPlayer].controlLeft == Main.player[Main.myPlayer].releaseLeft &&
                    Main.player[Main.myPlayer].controlRight == Main.player[Main.myPlayer].releaseRight &&
                    Main.player[Main.myPlayer].controlJump == Main.player[Main.myPlayer].releaseJump &&
                    Main.player[Main.myPlayer].controlDown == Main.player[Main.myPlayer].releaseDown)
                {
                    AfkPenaltyCounter++;
                }
                else
                {
                    AfkPenaltyCounter = 0;
                }
            }
        }

        public void CheckForInvasionEventCompletion()
        {
            if (!Main.gameMenu)
            {
                if (Main.invasionType != LastInvasionEvent)
                {
                    float ExpRewardType = 0;
                    string EventType = "";
                    switch (LastInvasionEvent)
                    {
                        case Terraria.ID.InvasionID.GoblinArmy:
                            ExpRewardType = 26.5f;
                            EventType = "Goblin Army";
                            break;
                        case Terraria.ID.InvasionID.PirateInvasion:
                            ExpRewardType = 46.5f;
                            EventType = "Pirate Invasion";
                            break;
                        case Terraria.ID.InvasionID.MartianMadness:
                            ExpRewardType = 66.5f;
                            EventType = "Martian Madness";
                            break;
                        case Terraria.ID.InvasionID.SnowLegion:
                            ExpRewardType = 40f;
                            EventType = "Snow Legion";
                            break;
                    }
                    if (ExpRewardType > 0)
                    {
                        PlayerMod pm = Main.player[Main.myPlayer].GetModPlayer<PlayerMod>();
                        int LastMaxExp = pm.GetGameModeInfo.MaxExp;
                        int Exp = pm.GetExpReward(ExpRewardType, 0.1f, ExpReceivedPopText.ExpSource.Invasion, false);
                        string ExpText = (ShowExpAsPercentage ? Math.Round((float)Exp / LastMaxExp * 100, 1) + "%" : Exp.ToString());
                        if (ExpText == "0%" && ShowExpAsPercentage)
                            ExpText = "a very small amount of";
                        Main.NewText("You gained " + ExpText + " exp reward for defeating the " + EventType + ".", Color.Azure);
                    }
                }
                if(LastEventWave != NPC.waveNumber && NPC.waveNumber > 1)
                {
                    float WaveReward = 0f;
                    if (Main.pumpkinMoon)
                    {
                        WaveReward = 30f + 2f * (NPC.waveNumber - 1);
                    }
                    if (Main.snowMoon)
                    {
                        WaveReward = 40f + 0.75f * (NPC.waveNumber - 1);
                    }
                    if (Terraria.GameContent.Events.DD2Event.Ongoing)
                    {
                        switch (Terraria.GameContent.Events.DD2Event.OngoingDifficulty)
                        {
                            case 1:
                                WaveReward = 15f + 0.3f * (NPC.waveNumber - 1);
                                break;
                            case 2:
                                WaveReward = 35f + 0.25f * (NPC.waveNumber - 1);
                                break;
                            case 3:
                                WaveReward = 55f + 0.2f * (NPC.waveNumber - 1);
                                break;
                        }
                    }
                    if (WaveReward > 0)
                    {
                        PlayerMod pm = Main.player[Main.myPlayer].GetModPlayer<PlayerMod>();
                        int LastMaxExp = pm.GetGameModeInfo.MaxExp;
                        int Exp = pm.GetExpReward(WaveReward, 0.05f, ExpReceivedPopText.ExpSource.Invasion, false);
                        string ExpText = (ShowExpAsPercentage ? Math.Round((float)Exp / LastMaxExp * 100, 1) + "%" : Exp.ToString());
                        if (ExpText == "0%" && ShowExpAsPercentage)
                            ExpText = "a very small amount of";
                        Main.NewText("You gained " + ExpText + " exp reward for surviving wave " + LastEventWave + ".", Color.Azure);
                    }
                }
            }
            LastInvasionEvent = Main.invasionType;
            LastEventWave = NPC.waveNumber;
        }
        
        public override object Call(params object[] args)
        {
            if(args.Length > 0 && args[0] is string)
            {
                switch (args[0])
                {
                    case "hidehud":
                        LevelInfoInterface.Hide = true;
                        break;
                    case "showhud":
                        LevelInfoInterface.Hide = false;
                        break;
                }
            }
            return base.Call(args);
        }

        public static bool CallGameModeSelector()
        {
            GameModeSelector.UpdateAndDraw();
            return true;
        }

        public static bool DrawEntityLevelCheck()
        {
            try
            {
                Vector2 TextPosition = new Vector2(Main.mouseX, Main.mouseY - 22);
                for (int n = 0; n < 200; n++)
                {
                    NPC npc = Main.npc[n];
                    if (npc.active && Main.mouseX >= npc.position.X - Main.screenPosition.X && Main.mouseX < npc.position.X - Main.screenPosition.X + npc.width && Main.mouseY >= npc.position.Y - Main.screenPosition.Y && Main.mouseY < npc.position.Y - Main.screenPosition.Y + npc.height)
                    {
                        NpcMod npcMod = Main.npc[n].GetGlobalNPC<NpcMod>();
                        if (npcMod != null && npcMod.NpcStatus != null)
                        {
                            //Vector2 TextPosition = npc.Center - Main.screenPosition;
                            //TextPosition.Y -= npc.height * 0.5f + 22f;
                            string Text = npcMod.NpcStatus.GetLevelText();
                            Utils.DrawBorderString(Main.spriteBatch, Text, TextPosition, Color.White, 1f, 0.5f, 0.5f);
                            //return true;
                        }
                    }
                }
                for (int p = 0; p < 255; p++)
                {
                    Player player = Main.player[p];
                    if (p != Main.myPlayer && player.active && !player.dead && Main.mouseX >= player.position.X - Main.screenPosition.X && Main.mouseX < player.position.X - Main.screenPosition.X + player.width && Main.mouseY >= player.position.Y - Main.screenPosition.Y && Main.mouseY < player.position.Y - Main.screenPosition.Y + player.height)
                    {
                        PlayerMod playerMod = Main.player[p].GetModPlayer<PlayerMod>();
                        if (playerMod != null)
                        {
                            //Vector2 TextPosition = player.Center - Main.screenPosition;
                            //TextPosition.Y -= player.height * 0.5f + 22f;
                            string Text = playerMod.GetGameModeInfo.GetLevelText();
                            Utils.DrawBorderString(Main.spriteBatch, Text, TextPosition, Color.White, 1f, 0.5f, 0.5f);
                            //return true;
                        }
                    }
                }
            }
            catch { }
            return true;
        }

        private static int LastLevel = -1, LastBiomeLevel = -1;
        private static string LastLevelText = "", LastBiomeLevelText = "";

        public static bool DrawClovers()
        {
            Effects.CloverEffect.DrawClovers();
            return true;
        }

        public static bool DrawGameplayHudInterface()
        {
            Player player = Main.player[Main.myPlayer];
            bool GameModeIsFreeMode = !Main.gameMenu && player.GetModPlayer<PlayerMod>().GetGameModeInfo.Base is GameModes.FreeMode;
            if(!GameModeSelector.Open)
            {
                float Opacity = 1f, Scale = 1f, XAnchor = 0.5f;
                Vector2 ZoneLevelInfoCenterPosition = new Vector2(Main.screenWidth * 0.5f, Main.screenHeight * 0.15f);
                if (LevelInfoOnScreenCorner && !Main.playerInventory)
                {
                    ZoneLevelInfoCenterPosition = new Vector2(Main.screenWidth * 0.15f, Main.screenHeight * 0.2f);
                    Scale = 0.85f;
                    XAnchor = 0;
                }
                else
                {
                    ZoneLevelInfoCenterPosition = new Vector2(Main.screenWidth * 0.5f, Main.screenHeight * 0.15f);
                }
                string ZoneLevelText = ZoneName(player);
                int MinLv = player.GetModPlayer<PlayerMod>().BiomeMinLv, MaxLv = player.GetModPlayer<PlayerMod>().BiomeMaxLv;
                if (BiomeTextFades && !Main.playerInventory)
                {
                    if (LastMinLevel != MinLv || LastMaxLevel != MaxLv)
                    {
                        LastMinLevel = MinLv;
                        LastMaxLevel = MaxLv;
                        BiomeTextFadeTime = 0;
                    }
                    if (BiomeTextFadeTime < FadeDuration * 2 + DisplayDuration)
                        BiomeTextFadeTime++;
                    else
                        Opacity = 0;
                    if (BiomeTextFadeTime < FadeDuration)
                    {
                        Opacity = (float)BiomeTextFadeTime * (1f / FadeDuration);
                    }
                    if (BiomeTextFadeTime >= (FadeDuration + DisplayDuration) && BiomeTextFadeTime < FadeDuration * 2 + DisplayDuration)
                    {
                        Opacity = 1f - (float)(BiomeTextFadeTime - (FadeDuration + DisplayDuration)) * (1f / FadeDuration);
                    }
                }
                else
                {
                    BiomeTextFadeTime = 0;
                }
                if (Opacity > 0)
                {
                    int DifferenceFactor = 10;
                    if (GameModeIsFreeMode)
                    {
                        int LevelBonus = GameModes.FreeMode.GetLevelBonus(player.Center) + GameModes.FreeMode.ProgressionIncrement();
                        DifferenceFactor = 200;
                        MinLv = LevelBonus - 100;
                        MaxLv = LevelBonus + 100;
                        //GameModes.FreeMode.RomanAlgarismMaker
                        if (LastBiomeLevel != LevelBonus)
                        {
                            LastBiomeLevel = LevelBonus;
                            LastBiomeLevelText = GameModes.FreeMode.RomanAlgarismMaker((int)(LevelBonus * 0.1) + 1);
                        }
                        ZoneLevelText += " [Rank " + LastBiomeLevelText + "]";
                    }
                    else
                    {
                        ZoneLevelText += " Lv " + "[" + MinLv + "~" + MaxLv + "]";
                    }
                    //Utils.DrawBorderStringFourWay(Main.spriteBatch, Main.fontMouseText, ZoneLevelText, ZoneLevelInfoCenterPosition.X, ZoneLevelInfoCenterPosition.Y, 
                    //    Color.White * Opacity, Color.Black * Opacity, new Vector2(XAnchor, 0.5f), Scale);
                    Color c = Color.White;
                    int LevelDifference = player.GetModPlayer<PlayerMod>().GetGameModeInfo.Level;
                    if (LevelDifference < MinLv)
                        LevelDifference -= MinLv;
                    else if (LevelDifference > MaxLv)
                        LevelDifference -= MaxLv;
                    else LevelDifference = 0;
                    if (LevelDifference <= -DifferenceFactor)
                        c = Color.Red;
                    else if (LevelDifference <= -DifferenceFactor * 0.5f)
                        c = Color.OrangeRed;
                    else if (LevelDifference >= DifferenceFactor)
                        c = Color.Cyan;
                    else if (LevelDifference >= DifferenceFactor * 0.5f)
                        c = Color.LightBlue;
                    Vector2 Dim = Utils.DrawBorderString(Main.spriteBatch, ZoneLevelText, ZoneLevelInfoCenterPosition, c * Opacity, Scale, XAnchor, 0.5f);
                }
            }
            //Player Level Infos
            string MouseOverText = "";
            GameModeData Data = player.GetModPlayer<PlayerMod>().GetGameModeInfo;
            if (!Main.playerInventory)
            {
                string CurrentLevel = Data.Level.ToString(), NextLevel = (Data.Level + 1).ToString();
                float ExpValue = 1f;
                string LevelText = "";
                if (GameModeIsFreeMode)
                {
                    LevelText = "Rank";
                    ExpValue = Data.Level % 10 * 10 + (float)Data.Exp / Data.MaxExp;
                    ExpValue *= 0.01f;
                    ExpValue = 1f - ExpValue;
                    CurrentLevel = GameModes.FreeMode.RomanAlgarismMaker((int)(Data.Level * 0.1f) + 1);
                    NextLevel = GameModes.FreeMode.RomanAlgarismMaker((int)(Data.Level * 0.1f) + 2);
                }
                else
                {
                    LevelText = "Level";
                    ExpValue = 1f - (float)Data.Exp / Data.MaxExp;
                    CurrentLevel = Data.Level.ToString();
                    NextLevel = (Data.Level + 1).ToString();
                }
                Vector2 TextPosition = new Vector2(Main.screenWidth - 4, Main.screenHeight - 30 + 4);
                float Distance = 0;
                float Padding = 0;
                if (Data.Level != Data.Level2)
                {
                    Distance = Utils.DrawBorderString(Main.spriteBatch, Data.Level2.ToString(), TextPosition, Color.White * Main.cursorAlpha, 1f, 1f).X;
                    TextPosition.X -= Distance + 16;
                    TextPosition.Y += 22 - 10;
                    Main.spriteBatch.Draw(LevelReductionArrow, TextPosition, null, Color.White, 0f, new Vector2(0, 10), 1f, (Data.Level2 > Data.Level ? SpriteEffects.FlipVertically : SpriteEffects.None), 0f);
                    TextPosition.Y -= 22 - 10;
                    Padding = Distance + 16 - 4;
                }
                Distance = Utils.DrawBorderString(Main.spriteBatch, CurrentLevel, TextPosition, Color.White * Main.cursorAlpha, 1f, 1f).X;
                TextPosition.X -= Distance + 150;
                TextPosition.Y += 10 - 4;
                bool MouseOverLevelArrow = Main.mouseX >= TextPosition.X && Main.mouseX < TextPosition.X + 150 && Main.mouseY >= TextPosition.Y && Main.mouseY < TextPosition.Y + 12;
                Main.spriteBatch.Draw(LevelingArrow, TextPosition, new Rectangle(0, 0, 150, 12), Color.White);
                TextPosition.X += 2 + (int)(146 * ExpValue);
                //TextPosition.Y += 2;
                Main.spriteBatch.Draw(LevelingArrow, TextPosition, new Rectangle(150 + 2 + (int)(146 * ExpValue), 0, 2 + (int)(146 * (1f - ExpValue)), 12), Color.Yellow * Main.cursorAlpha);
                TextPosition.X -= 2 + (int)(146 * ExpValue);
                //TextPosition.Y -= 2;
                TextPosition.Y -= 10 - 4;
                Utils.DrawBorderString(Main.spriteBatch, NextLevel, TextPosition, Color.White * Main.cursorAlpha, 1f, 1f);
                TextPosition.X = Main.screenWidth - (4 + Distance + 75) - Padding;
                TextPosition.Y -= 15;
                float AnchorX = 0.5f;
                if (MouseOverLevelArrow)
                {
                    LevelText = "";
                    if (GameModeIsFreeMode)
                    {
                        LevelText = "Next Rank: " + (Data.Level % 10) + "/10 Levels. Exp [" + Data.Exp + "/" + Data.MaxExp + " (" + Math.Round((float)Data.Exp * 100 / Data.MaxExp, 2) + "%)]";
                    }
                    else
                    {
                        LevelText = "Exp [" + Data.Exp + "/" + Data.MaxExp + "(" + Math.Round((float)Data.Exp * 100 / Data.MaxExp, 2) + "%)]";
                    }
                    AnchorX = 1f;
                    TextPosition.X += 75;
                }
                Utils.DrawBorderString(Main.spriteBatch, LevelText, TextPosition, Color.White * Main.cursorAlpha, 1f, AnchorX);
            }
            if (MouseOverText != "")
            {
                Utils.DrawBorderString(Main.spriteBatch, MouseOverText, new Vector2(Main.mouseX + 4, Main.mouseY + 4), Color.White, 1f, 0.75f);
            }
            return true;
        }

        public static bool DrawButton(string Text, Vector2 Position, Color color, float Scale = 1f, float AnchorX = 0.0f, float AnchorY = 0.0f)
        {
            Vector2 Dimension = Utils.DrawBorderString(Main.spriteBatch, Text, Position, color, Scale, AnchorX, AnchorY);
            if (Main.mouseX >= Position.X - Dimension.X * AnchorX && Main.mouseX < Position.X + Dimension.X * (1f - AnchorX) &&
                Main.mouseY >= Position.Y - Dimension.Y * AnchorY && Main.mouseY < Position.Y + (1f - Dimension.Y))
            {
                Position.X -= 2;
                Position.Y -= 2;
                Utils.DrawBorderString(Main.spriteBatch, Text, Position, Color.Yellow, Scale, AnchorX, AnchorY);
                if (Main.mouseLeft && Main.mouseLeftRelease)
                {
                    Position.X -= 2;
                    Position.Y -= 2;
                    Utils.DrawBorderString(Main.spriteBatch, Text, Position, Color.Cyan, Scale, AnchorX, AnchorY);
                    return true;
                }
            }
            return false;
        }

        public static bool InsideLihzahrdDungeon(Player player)
        {
            bool Is = false;
            int PX = (int)player.Center.X / 16, PY = (int)player.Center.Y / 16;
            if (PX >= Main.leftWorld / 16 && PX < Main.rightWorld / 16 && PY >= Main.topWorld / 16 && PY < Main.bottomWorld / 16)
            {
                if (Main.tile[PX, PY].wall == Terraria.ID.WallID.LihzahrdBrickUnsafe)
                    Is = true;
            }
            return Is;
        }

        public static string ZoneName(Player player)
        {
            if (InsideLihzahrdDungeon(player))
                return "Lihzahrd Dungeon";
            if (player.ZoneTowerNebula || player.ZoneTowerSolar || player.ZoneTowerStardust || player.ZoneTowerVortex)
                return "Celestial Invasion";
            if (player.ZoneOverworldHeight)
            {
                if (Main.pumpkinMoon)
                    return "Pumpkin Moon";
                if (Main.snowMoon)
                    return "Frost Moon";
            }
            if (player.ZoneUnderworldHeight)
                return "Underworld";
            bool Underground = player.ZoneDirtLayerHeight;
            bool Cavern = player.ZoneRockLayerHeight;
            string Affix = "", Suffix = "";
            if (player.ZoneCorrupt)
                Affix = "Corrupt";
            else if (player.ZoneCrimson)
                Affix = "Crimson";
            else if (player.ZoneHoly)
                Affix = "Hallow";
            if (player.ZoneDungeon)
            {
                Suffix = "Dungeon";
                int tx = (int)(player.Center.X * (1f / 16)), ty = (int)(player.Center.Y * (1f / 16));
                switch (Main.tile[tx, ty].wall)
                {
					//Brick
                    case 7:
                        Suffix += ": Cobalt Armory";
                        break;
                    case 8:
                        Suffix += ": Catacombs";
                        break;
                    case 9:
                        Suffix += ": Alchemy Laboratory";
                        break;
					//Slab
                    case 94:
                        Suffix += ": Forgotten Labyrinth";
                        break;
					case 98:
                        Suffix += ": Colosseum";
						break;
					case 96:
                        Suffix += ": Marble Galery";
						break;
					//Tile
                    case 95:
                        Suffix += ": Unholy Chapel";
                        break;
                    case 99:
                        Suffix += ": Clothier's Quarters";
                        break;
                    case 97:
                        Suffix += ": Devil Guest House";
                        break;
                }
            }
            else if (player.ZoneJungle)
                Suffix = "Jungle";
            else if (player.ZoneBeach)
            {
                Suffix = "Beach";
            }
            else if (player.ZoneUndergroundDesert)
            {
                Suffix = "Antlion Pit";
            }
            else if (player.ZoneDesert)
                Suffix = "Desert";
            else if (player.ZoneSnow)
            {
                if (!Cavern)
                {
                    Suffix = "Tundra";
                }
                else
                {
                    Suffix = "Ice Cave";
                }
            }
            else
            {
                if (player.GetModPlayer<PlayerMod>().ZoneGraveyard)
                    Suffix = "Graveyard";
                else if (Underground)
                    Suffix = "Underground";
                else if (Cavern)
                    Suffix = "Caverns";
                else
                    Suffix = "Forest";
            }
            if (PlayerMod.IsPlayerInInvasionPosition(player))
                Suffix += " (Contested)";
            return Affix + (Affix != "" ? " " : "") + Suffix;
        }

        public static void TriggerLuckyClovers(Vector2 Position, bool VeryLucky = false)
        {
            if (Main.netMode == 2)
                return;
            NetPlayMod.SendLuckyClovers(Position, VeryLucky);
            for(int i = 0; i < (VeryLucky ? 8 : 4); i++)
            {
                Effects.CloverEffect.AddClover(Position, VeryLucky);
            }
        }

        public override void Load()
        {
            mod = this;
            GameModeList.Clear();
            CreateDefaultGameModes();
            if (!Main.dedServ)
            {
                LevelingArrow = GetTexture("Interface/LevelArrow");
                LevelReductionArrow = GetTexture("Interface/LevelReductionArrow");
                StatusTextButtonTexture = GetTexture("Interface/StatusBtn");
                LuckyCloverTexture = GetTexture("Effects/LuckyClover");
                VeryLuckyCloverTexture = GetTexture("Effects/VeryLuckyClover");
            }
        }

        /*public override void PostSetupContent()
        {
            int[] Keys = Main.npcLifeBytes.Keys.ToArray();
            foreach (int key in Keys)
            {
                Main.npcLifeBytes[key] = 4;
            }
        }*/

        public override bool HijackSendData(int whoAmI, int msgType, int remoteClient, int ignoreClient, NetworkText text, int number, float number2, float number3, float number4, int number5, int number6, int number7)
        {
            if (msgType == 23)
            {
                int NpcID = Main.npc[number].netID;
                if (Main.npcLifeBytes.ContainsKey(NpcID))
                {
                    Main.npcLifeBytes[NpcID] = 4;
                }
                else
                {
                    Main.npcLifeBytes.Add(NpcID, 4);
                }
                if (Main.npcLifeBytes[NpcID] != 4)
                {
                    string Mes = Main.npc[NpcID].GivenOrTypeName + "'s byte value sent: " + Main.npcLifeBytes[NpcID];
                    if (Main.netMode == 1)
                        Main.NewText(Mes);
                    else
                        Console.WriteLine(Mes);
                }
            }
            return false;
        }

        public override bool HijackGetData(ref byte messageType, ref BinaryReader reader, int playerNumber)
        {
            if (messageType == 23)
            {
                long PositionBackup = reader.BaseStream.Position;
                reader.BaseStream.Position += 2 + 4 * 2 + 4 * 2 + 2 + 1 + 4 * 4;
                int NpcID = reader.ReadInt16();
                reader.BaseStream.Position = PositionBackup;
                if (Main.npcLifeBytes.ContainsKey(NpcID))
                {
                    Main.npcLifeBytes[NpcID] = 4;
                }
                else
                {
                    Main.npcLifeBytes.Add(NpcID, 4);
                }
                if (Main.npcLifeBytes[NpcID] != 4)
                {
                    string Mes = Main.npc[NpcID].GivenOrTypeName + "'s byte value received: " + Main.npcLifeBytes[NpcID];
                    if (Main.netMode == 1)
                        Main.NewText(Mes);
                    else
                        Console.WriteLine(Mes);
                }
            }
            return false;
        }

        public static void CreateDefaultGameModes()
        {
            AddGameMode(new GameModes.BasicRPG());
            AddGameMode(new GameModes.RegularRPG(0));
            AddGameMode(new GameModes.RegularRPG(1));
            AddGameMode(new GameModes.RegularRPG(2));
            AddGameMode(new GameModes.FreeMode());
            AddGameMode(new GameModes.RaidMode());
            AddGameMode(new GameModes.ClassicRPG());
        }

        public static void AddGameMode(GameModeBase gmbase)
        {
            if (GameModeList.ContainsKey(gmbase.GameModeID))
            {
                throw new Exception("The game mode id \'"+gmbase.GameModeID+"\' already exists, use a different ID.");
            }
            GameModeList.Add(gmbase.GameModeID, gmbase);
        }

        public static bool HasGameModeID(string ID)
        {
            return GameModeList.ContainsKey(ID);
        }

        public static GameModeBase GetGameMode(string ID)
        {
            if (HasGameModeID(ID))
                return GameModeList[ID];
            return null;
        }

        public static string GetGameModeIDByName(string Name)
        {
            foreach (string key in GameModeList.Keys.ToArray())
            {
                if (GameModeList[key].Name == Name)
                    return key;
            }
            return "";
        }

        public static GameModeBase GetCurrentGameMode
        {
            get { return GetGameMode(FixedGameMode); }
        }

        public static string GetGameModeNameByID(string ID)
        {
            if (ID == null) ID = "";
            if (GameModeList.ContainsKey(ID))
                return GameModeList[ID].Name;
            return "";
        }

        public override void HandlePacket(System.IO.BinaryReader reader, int whoAmI)
        {
            NetPlayMod.ReceivedMessages(reader, whoAmI);
        }
    }
}
