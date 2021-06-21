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

        public static void ResetBrokenTilesList()
        {
            RecentlyBrokenTiles.Clear();
        }

        public override void KillTile(int i, int j, int type, ref bool fail, ref bool effectOnly, ref bool noItem)
        {
            if (fail)
                return;
            if (RecentlyBrokenTiles.Contains(type))
                return;
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
            const float ExpShareDistance = 600f;
            foreach(int p in PlayerMod.GetPlayerTeamMates(Main.player[Main.myPlayer]))
            {
                if(Main.player[p].active && !Main.player[p].dead && Math.Abs(Main.player[p].Center.X - Position.X) < ExpShareDistance &&
                    Math.Abs(Main.player[p].Center.Y - Position.Y) < ExpShareDistance)
                {
                    PlayerMod pm = Main.player[p].GetModPlayer<PlayerMod>();
                    int Exp = pm.GetGameModeInfo.Base.GetDigExp(type);
                    if(Exp > 0)
                        pm.GetExp(Exp, ExpReceivedPopText.ExpSource.Digging, false);
                }
            }
        }
    }
}
