using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;

namespace NExperience
{
    public class ArcadeDungeon
    {
        public static int Score = 0, HiScore = 0;

        public static void UpdateNpcLevel(NPC npc, out int Level)
        {
            Level = (int)(Score * 1f / 1000);
        }

        public static void UpdateNpcStatus(NPC npc)
        {
            npc.lifeMax += (int)(Score * 0.02f);
            npc.damage += (int)(Score * 0.001f);
            npc.defense += (int)(Score * 0.001f);
        }

        public void EndArcadeDungeon()
        {
            bool HighScore = Score > HiScore;
            Main.NewText("Arcade Dungeon run is over.");
            if (HighScore)
            {
                HiScore = Score;
                Main.NewText("New Record! " + HiScore + " Points.");
            }
            else
            {
                Main.NewText("Final Score: " + Score + " Points.");
            }
            for(int p = 0; p < 255; p++)
            {
                PlayerMod pm = Main.player[p].GetModPlayer<PlayerMod>();
                pm.GetExpReward(10, Score * (HighScore ? 0.04f : 0.1f), ExpReceivedPopText.ExpSource.Arcade, false);
            }
        }

        public void OnKillMob(NPC npc)
        {
            int ScoreToGet = GetScore(npc.netID);
            Score += ScoreToGet;
            CombatText.NewText(npc.getRect(), Microsoft.Xna.Framework.Color.Gold, "+" + ScoreToGet + "P.");
        }

        public int GetScore(int Type)
        {
            switch (Type)
            {
                case -13:
                    return 25;
                case -14:
                    return 200;
                case 31:
                    return 100;
                case 32:
                    return 50;
                case 33:
                    return 5;
                case 34:
                    return 20;
                case 71:
                    return 250;
            }
            return 0;
        }
    }
}
