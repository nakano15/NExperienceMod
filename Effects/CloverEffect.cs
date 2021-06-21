using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace NExperience.Effects
{
    public class CloverEffect
    {
        public static List<CloverEffect> CloverList = new List<CloverEffect>();
        public Vector2 Position = Vector2.Zero, Velocity = Vector2.Zero;
        public float Opacity = 0, RotationSpeed = 0f, Rotation = 0f, Scale = 1f;
        public bool VeryLucky = false;

        public static void AddClover(Vector2 Position, bool VeryLucky = false)
        {
            CloverList.Add(new CloverEffect(Position, VeryLucky));
        }

        public static void DrawClovers()
        {
            for(int c = 0; c < CloverList.Count; c++)
            {
                CloverList[c].Draw();
                if (CloverList[c].Opacity <= 0)
                    CloverList.RemoveAt(c);
            }
        }

        public CloverEffect(Vector2 Position, bool VeryLucky = false)
        {
            float MoveRotation = Main.rand.NextFloat() * 6.28319f;
            this.Position = Position;
            Velocity = new Vector2((float)Math.Cos(MoveRotation), (float)Math.Sin(MoveRotation)) * (VeryLucky ? 8 : 6);
            Rotation = Main.rand.NextFloat() * 6.28319f;
            RotationSpeed = 0.436332f * Main.rand.NextFloat();
            Scale = VeryLucky ? (0.8f + Main.rand.NextFloat() * 0.05f) : (0.65f + Main.rand.NextFloat() * 0.05f);
            this.VeryLucky = VeryLucky;
            if (Main.rand.NextDouble() < 0.5f)
                RotationSpeed *= -1;
            Opacity = 0f;
        }

        public void Draw()
        {
            Velocity *= 0.95f;
            if (Velocity.Length() < 1f)
            {
                Opacity -= 0.05f;
                if (Opacity <= 0)
                    return;
            }
            else
            {
                Opacity += 0.1f;
                if (Opacity > 1f)
                    Opacity = 1f;
            }
            Rotation += RotationSpeed;
            RotationSpeed *= 0.95f;
            Position += Velocity;
            Texture2D Texture = VeryLucky ? MainMod.VeryLuckyCloverTexture : MainMod.LuckyCloverTexture;
            Main.spriteBatch.Draw(Texture, Position - Main.screenPosition, null, Color.White * Opacity, Rotation, new Vector2(Texture.Width, Texture.Height) * 0.5f, Scale, SpriteEffects.None, 0f);
        }
    }
}
