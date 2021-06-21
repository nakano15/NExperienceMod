using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace NExperience
{
    public class GameModeBase
    {
        public static List<float> EquipmentTierStatusPercentage = new List<float>();
        public string GameModeID = "", Name = "", Description = "", WikiPageID = "";
        public int InitialLevel = 1;
        public int MaxLevel = 150;
        public double StatusPointsPerLevel = 1;
        public int InitialStatusPoints = 0;
        public int DefenseToHealthConversionValue = 2;
        public bool AllowLevelCapping = true;
        public delegate int ExpFormulaDel(int Level, GameModeData gmd);
        public delegate void PlayerStatusDel(int Level, int UncappedLevel, Dictionary<byte, int> PointsInvested, out PlayerStatusMod mod);
        public delegate void NpcStatusDel(NPC npc, GameModeData Data);
        public delegate void BiomeLevelRulesDel(Player player, GameModeData Data, out int MinLevel, out int MaxLevel);
        public delegate int MobSpawnLevelDel(NPC npc);
        public delegate string LevelTextDel(GameModeData gmd);
        public List<StatusInfo> Status = new List<StatusInfo>();

        public class StatusInfo
        {
            public string Name = "", Description = "";
            public int MaxPoints = -1, InitialPoints = 0;
        }

        public virtual int ExpFormula(int level, GameModeData gmd) { return level * 100; }

        public void PlayerStatus(int Level, int UncappedLevel, Dictionary<byte, int> PointsInvested, out PlayerStatusMod mod) {
            PlayerStatus(Level, UncappedLevel, PointsInvested, PointsInvested, out mod);
        }

        public virtual void PlayerStatus(int Level, int UncappedLevel, Dictionary<byte, int> PointsCapped, Dictionary<byte, int> PointsInvested, out PlayerStatusMod mod) { mod = new PlayerStatusMod(); }

        public virtual void NpcStatus(NPC npc, GameModeData Data) { }

        public virtual void BiomeLevelRules(Player player, GameModeData Data, out int MinLevel, out int MaxLevel) { MinLevel = 1; MaxLevel = 2; }

        public virtual int MobSpawnLevel(NPC npc) { return -1; }

        public virtual int PlayerUpscaleLevel(NPC npc, Player player)
        {
            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Level">From 0 to 100, about the part of the leveling which the exp reward is given. 100 is literally end game.</param>
        /// <param name="Difficulty">How difficult is that task for the grade. This determines how many percents of exp bar will be given.</param>
        /// <param name="PlayerLevel">The level of the player who the reward will be given to.</param>
        /// <returns></returns>
        public virtual int GetExpReward(float Level, float Difficulty, GameModeData gmd)
        {
            if (Level > MaxLevel)
                Level = MaxLevel;
            if (Level < 0)
                Level = 0;
            return (int)(ExpFormula((int)(Level / 100 * MaxLevel), gmd) * Difficulty);
        }

        public virtual int GetDigExp(int TileID)
        {
            return 0;
        }

        public virtual string LevelText(GameModeData gmd)
        {
            string Text = "Level: " + gmd.Level;
            if (gmd.Level2 != gmd.Level)
                Text += " -> " + gmd.Level2;
            return Text;
        }

        public virtual int LevelDamageScale(int Level)
        {
            return 0;
        }

        public virtual int LevelDefenseScale(int Level)
        {
            return 0;
        }

        public virtual int AttackExp(GameModeData playerdata, int Damage, bool Critical, int OponentLevel)
        {
            int Exp = 0;

            return Exp;
        }

        public const int MaxExpPossible = int.MaxValue;
    }
}
