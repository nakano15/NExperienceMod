using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Config.UI;
using Terraria.UI;

namespace NExperience
{
    [Label("Personal Configuration")]
    public class ClientConfigMod : ModConfig
    {
        public override ConfigScope Mode
        {
            get { return ConfigScope.ClientSide; }
        }

        [Label("Show exp as percentage?")]
        [Tooltip("Leaving this on, gives you a better information about how much your character improves in combat.")]
        public bool ShowExpAsPercentage { get { return MainMod.ShowExpAsPercentage; } set { MainMod.ShowExpAsPercentage = value; } }

        [Label("Biome level persists on screen?")]
        [Tooltip("Disabling this, the biome level info will appear on the screen when the level of the biomes is changed, and disappear after some time.")]
        [DefaultValue(true)]
        public bool BiomeLevelPersistOnScreen { get { return !MainMod.BiomeTextFades; } set { MainMod.BiomeTextFades = !value; } }

        [Label("Display biome level on the left corner?")]
        [Tooltip("Makes the biome level appear on the left side of the screen.")]
        [DefaultValue(false)]
        public bool LevelInfoOnScreenCorner { get { return MainMod.LevelInfoOnScreenCorner; } set { MainMod.LevelInfoOnScreenCorner = value; } }
    }

    [Label("Global Gameplay Rules")]
    public class ConfigMod : ModConfig
    {
        public override ConfigScope Mode
        {
            get { return ConfigScope.ServerSide; }
        }

        public override void OnLoaded()
        {

        }

        [Label("Enable multiplayer testing?")]
        [Tooltip("Enable mod data sync on multiplayer.")]
        [DefaultValue(true)]
        public bool TestMultiplayer { get { return MainMod.TestMultiplayerSync; } set { MainMod.TestMultiplayerSync = value; } }

        [Label("Biome Level Capper")]
        [Tooltip("If your character level is higher than the biome maximum level, or toughest npc nearby, the level will be scaled down.")]
        public bool AddBiomeLevelCapper { get { return MainMod.LevelCapping; } set { MainMod.LevelCapping = value; } }

        [Label("Set Everything To My Level")]
        [Tooltip("Makes all npcs in the world have the level scaled to your character.\nIf Biome Level capper is on, will be downscaled based on biome.\nOn multiplayer, It will instead take the level of the closest player.")]
        public bool SetEverythingToMyLevel { get { return MainMod.EverythingHasMyLevel; } set { MainMod.EverythingHasMyLevel = value; } }

        [Label("Status Capper")]
        [Tooltip("Works with the Biome Level Capper. Adds a cap to item damage and defense depending on the level.")]
        public bool AddStatusLevelCapper { get { return MainMod.ItemStatusCapper; } set { MainMod.ItemStatusCapper = value; } }

        [Label("Allow Mana Boost?")]
        [Tooltip("Enable this only if you're using a mod that overrides Terraria vanilla mana cap. This option makes the status give bonus to the maximum mana of your character.")]
        public bool AllowManaBoosts { get { return MainMod.AllowManaBoosts; } set { MainMod.AllowManaBoosts = value; } }

        [Label("Infinite Leveling")]
        [Tooltip("Not really infinite, but this option will remove the level cap from the game mode. It's effect will not remove the Biome Level Capper effect, but points spent in the status will affect the scaled down points invested.")]
        public bool InfiniteLeveling { get { return MainMod.InfiniteLeveling; } set { MainMod.InfiniteLeveling = value; } }

        [Label("Bring monsters to my level after the end game?")]
        [Tooltip("Upon enabling this, and after defeating the Moonlord once, all monsters will have their status level scaled to yours, but that wont change their exp given.")]
        public bool Playthrough1dot5 { get { return MainMod.Playthrough1dot5OnEndgame; } set { MainMod.Playthrough1dot5OnEndgame = value; } }

        [Label("Cap level to max game mode level if overleveled.")]
        [Tooltip("Leveling continues on Infinite Leveling, but the player status level will be capped to the game mode max level when overleveled.")]
        public bool CapLevelOnInfiniteLeveling { get { return MainMod.CapLevelOnInfiniteLeveling; } set { MainMod.CapLevelOnInfiniteLeveling = value; } }

        [Label("Give status boost to Pre-Hardmode Enemies on Hardmode.")]
        [Tooltip("Enabling this, will make the game give a status boost to Pre-Hardmode enemies when you are on a Hardmode world. Does not work on Expert worlds.")]
        public bool BuffPreHardModeEnemiesOnHardmode { get { return MainMod.BuffPreHardmodeEnemiesOnHardmode; } set { MainMod.BuffPreHardmodeEnemiesOnHardmode = value; } }
        
        [Label("Convert Enemies Defense bonus into Health?")]
        [Tooltip("Change how the leveling works, so each point of Defense a monster gains through the scaling of the game mode, be converted into health.")]
        public bool MobDefenseToHealth { get { return MainMod.MobDefenseConvertToHealth; } set { MainMod.MobDefenseConvertToHealth = value; } }

        [Label("Potions for Sale")]
        [Tooltip("Makes all potions disponible for sale.")]
        public bool PotionsForSale { get { return MainMod.PotionsForSale; } set { MainMod.PotionsForSale = value; } }

        [Label("Exp Rate")]
        [Tooltip("Changes the rate at which you receive exp from killing monsters.")]
        [Range(0f, 4f)]
        [Increment(0.05f)]
        [DrawTicks]
        [Slider]
        [DefaultValue(1f)]
        public float ExpRate { get { return MainMod.DefaultExpRate; } set { MainMod.DefaultExpRate = value; } }

        [Label("Exp Penalty by Level Difference")]
        [Tooltip("If the value is different from 0, a exp percentage change will be incurred based on the level difference between the player and the monster. Player being overleveled past exp range will gain only 1 exp, while under leveled past exp range will receive up to 2x exp.")]
        [Slider]
        [Increment(1)]
        [Range(0, 100)]
        public int ExpPenaltyRange { get { return MainMod.ExpPenaltyByLevelDifference; } set { MainMod.ExpPenaltyByLevelDifference = value; } }

        [Label("Bosses are as tough as me")]
        [Tooltip("All bosses has their level scaled to your character level, If you are facing them after defeating it once. But they will give the exp of their level.")]
        public bool BossesAsToughAsMe { get { return MainMod.BossesAsToughasMe; } set { MainMod.BossesAsToughasMe = value; } }

        [Label("Full Regen on Level Up?")]
        [Tooltip("Your Health and Mana are fully restored upon leveling up.")]
        public bool FullRegenOnLevelUp { get { return MainMod.FullRegenOnLevelUp; } set { MainMod.FullRegenOnLevelUp = value; } }

        [Label("Zombies Drops Tombstones?")]
        [Tooltip("There will be a chance where Zombies will drop Tombstones upon death.")]
        [DefaultValue(true)]
        public bool ZombiesDropsTombstonesOnDeath { get { return MainMod.ZombiesDropsTombstones; } set { MainMod.ZombiesDropsTombstones = value; } }

        [Label("Disable Mod Enemies?")]
        [Tooltip("Stops mod enemies from spawning, with the exception of the Graveyard biome enemies.")]
        [DefaultValue(false)]
        public bool DisableModEnemies { get { return MainMod.DisableModEnemies; } set { MainMod.DisableModEnemies = value; } }

        [Label("[Regular RPG] Enable Pos level 100 monster status bonus?")]
        [Tooltip("Monsters will gain bonus status past level 100. Disable this if the gameplay is way too hard for you.")]
        [DefaultValue(true)]
        public bool Past100MobBoost { get { return GameModes.RegularRPG.PosLevel100Scale; } set { GameModes.RegularRPG.PosLevel100Scale = value; } }

        [Label("[Regular RPG] Enable Pos level 100 max experience nerf?")]
        [Tooltip("This nerf was added about season 6 of N Terraria, the exp beyond this point increases logarythimically, reducing drastically the grinding to level up.")]
        [DefaultValue(true)]
        public bool Past100ExpNerf { get { return GameModes.RegularRPG.PosLevel100ExpNerf; } set { GameModes.RegularRPG.PosLevel100ExpNerf = value; } }

        [Label("[Regular RPG] Boost Bloodmoon Monsters Status?")]
        [Tooltip("By default, the bloodmoon monsters gains 50% of status boost. This can be troublesome when on hardmode, but the reward will be greater.")]
        [DefaultValue(true)]
        public bool BloodmoonBoost { get { return GameModes.RegularRPG.BloodmoonStatusBoost; } set { GameModes.RegularRPG.BloodmoonStatusBoost = value; } }

        [Label("[Free Mode] Bosses have nerfed levels")]
        [Tooltip("To avoid boss fights of being overkill, they gain a level reduction of 8 times. Only disable if you are masochist.")]
        [DefaultValue(true)]
        public bool BossNerfedLevels { get { return GameModes.FreeMode.BossesHaveNerfedLevels; } set { GameModes.FreeMode.BossesHaveNerfedLevels = value; } }

        [Label("Maximum Exp Reduction Percentage from Afk Penalty.")]
        [Tooltip("Changes the maximum amount of exp penalty players will get from being afk in the mod.")]
        [DefaultValue(60)]
        [Range(0, 200)]
        [Increment(5)]
        public int MaxExpPenaltyPercentage { get { return MainMod.MaxExpPenaltyStack; } set { MainMod.MaxExpPenaltyStack = value; } }

        [Label("Death Exp Penalty Percentage.")]
        [Tooltip("Changes how much exp percentage players will lose from dying to anything.")]
        [DefaultValue(5)]
        [Range(0, 100)]
        public int DeathExpPenaltyPercentage { get { return MainMod.DeathExpPenalty; } set { MainMod.DeathExpPenalty = value; } }

        public override void OnChanged()
        {

        }
    }
}
