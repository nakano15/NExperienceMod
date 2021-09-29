using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace NExperience
{
    public class TileMod : GlobalTile
    {
        private static List<int> RecentlyBrokenTiles = new List<int>();
        public static List<Point> PlayerPlacedTiles = new List<Point>();

        public static void ResetBrokenTilesList()
        {
            RecentlyBrokenTiles.Clear();
        }

        public override void PlaceInWorld(int i, int j, Item item)
        {
            if(item.value > 0)
                PlayerPlacedTiles.Add(new Point(i, j));
        }

        public override void KillTile(int i, int j, int type, ref bool fail, ref bool effectOnly, ref bool noItem)
        {
            if (fail)
                return;
            if (RecentlyBrokenTiles.Contains(type))
                return;
            if (PlayerPlacedTiles.Contains(new Point(i, j)))
            {
                PlayerPlacedTiles.Remove(new Point(i, j));
                return;
            }
            if (type != Terraria.ID.TileID.MushroomTrees &&
                type != Terraria.ID.TileID.PalmTree &&
                type != Terraria.ID.TileID.Trees &&
                type != Terraria.ID.TileID.CrimsonVines &&
                type != Terraria.ID.TileID.HallowedVines &&
                type != Terraria.ID.TileID.JungleVines &&
                type != Terraria.ID.TileID.Vines)
            {
                RecentlyBrokenTiles.Add(type);
            }
            Vector2 Position = new Vector2(i * 16 + 8, j * 16 + 8);
            Player NearestToTile = Main.player[Main.myPlayer];
            if(Main.netMode > 0)
            {
                float NearestDistance = float.MaxValue;
                for(int p = 0; p < 255; p++)
                {
                    if(Main.player[p].active && !Main.player[p].dead)
                    {
                        float Distance = Main.player[p].Distance(Position);
                        if(Distance < NearestDistance)
                        {
                            NearestDistance = Distance;
                            NearestToTile = Main.player[p];
                        }
                    }
                }
            }
            foreach (int p in PlayerMod.GetPlayerTeamMates(NearestToTile))
            {
                PlayerMod pm = Main.player[p].GetModPlayer<PlayerMod>();
				if(pm == null)
					continue;
                int Exp = pm.GetGameModeInfo.Base.GetDigExp(type);
                if (Exp > 0)
                    pm.GetExp(Exp, ExpReceivedPopText.ExpSource.Digging, false);
                switch (type)
                {
                    case Terraria.ID.TileID.Pots:
                        pm.ClayPotMagicFindPoints++;
                        //CombatText.NewText(new Rectangle(i * 16, j * 16, 8, 8), Color.Green, "Luck Up!", true);
                        break;
                    case Terraria.ID.TileID.DemonAltar:
                        pm.AltarMagicFindPoints++;
                        //CombatText.NewText(new Rectangle(i * 16, j * 16, 8, 8), Color.Green, "Luck Up!", true);
                        break;
                    case Terraria.ID.TileID.ShadowOrbs:
                        pm.OrbMagicFindPoints++;
                        //CombatText.NewText(new Rectangle(i * 16, j * 16, 8, 8), Color.Green, "Luck Up!", true);
                        break;
                    case Terraria.ID.TileID.Heart:
                        pm.LifeCrystalMagicPoints++;
                        //CombatText.NewText(new Rectangle(i * 16, j * 16, 8, 8), Color.Green, "Luck Up!", true);
                        break;
                }
            }
        }
    }
}
