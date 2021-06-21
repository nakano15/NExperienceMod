using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NExperience
{
    public class ExpReceivedPopText
    {
        public ExpSource Source = ExpSource.Other;
        public int ExpValue = 0, LatestExpValue = 0;
        public float ExpPercentage = 0, LatestExpPercentage = 0;
        public ushort TextDuration = 0;
        public const ushort MaxDuration = 300;
        public bool Overflow = false;

        public ExpReceivedPopText(ExpSource source, int Value, PlayerMod player)
        {
            TextDuration = MaxDuration;
            Source = source;
            ExpValue = LatestExpValue = Value;
            ExpPercentage = LatestExpPercentage = (float)Value * 100 / player.GetGameModeInfo.MaxExp;
        }

        public void UpdateExp(int Value, PlayerMod player)
        {
            TextDuration = MaxDuration;
            if (!Overflow)
            {
                try
                {
                    ExpValue = checked(ExpValue + Value);
                }
                catch
                {
                    Overflow = true;
                    return;
                }
                LatestExpPercentage = Value;
                float Percentage = (float)Value * 100 / player.GetGameModeInfo.MaxExp;
                ExpPercentage += Percentage;
                LatestExpPercentage = Percentage;
            }
        }

        public string GetText
        {
            get
            {
                string Result = "";
                switch (Source)
                {
                    case ExpSource.MobKill:
                        Result = "Creature ";
                        break;
                    case ExpSource.Digging:
                        Result = "Destruction ";
                        break;
                    case ExpSource.Crafting:
                        Result = "Crafting ";
                        break;
                    case ExpSource.Fishing:
                        Result = "Fishing ";
                        break;
                    case ExpSource.Quest:
                        Result = "Quest ";
                        break;
                    case ExpSource.Event:
                        Result = "Event ";
                        break;
                    case ExpSource.Invasion:
                        Result = "Invasion ";
                        break;
                    case ExpSource.Extractinator:
                        Result = "Scavenging ";
                        break;
                    case ExpSource.AttackExp:
                        Result = "Combat ";
                        break;
                    case ExpSource.Arcade:
                        Result = "Arcade Dungeon ";
                        break;
                    default:
                        Result = "";
                        break;
                }
                Result += "Exp: ";
                if(Overflow)
                {
                    Result += "WAY OVER 2 MILLION!!";
                }
                else if (MainMod.ShowExpAsPercentage)
                {
                    float Value = (float)Math.Round(LatestExpPercentage, 2);
                    Result += (Value == 0 ? "< 0.01" : Value.ToString()) + "%";
                    Value = (float)Math.Round(ExpPercentage, 2);
                    Result += " Total: " + (Value == 0 ? "< 0.01" : Value.ToString()) + "%";
                }
                else
                {
                    Result += LatestExpValue + " Total: " + ExpValue;
                }
                return Result;
            }
        }

        public enum ExpSource : byte
        {
            Other = 0,
            MobKill,
            Digging,
            Crafting,
            Fishing,
            Quest,
            Event,
            Invasion,
            Extractinator,
            AttackExp,
            Arcade
        }
    }
}
