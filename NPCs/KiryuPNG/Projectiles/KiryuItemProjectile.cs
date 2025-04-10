using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace TestContent.NPCs.KiryuPNG.Projectiles
{
    public abstract class KiryuItemProjectile : ModProjectile
    {
        public string projName = "";
        public int goreAmount = 0;

        public float gravity = 0.3f;
        public float maxYVelocity = 10f;
        public float rotationalForce = 3f;

        public int width = 50;
        public int height = 50;

        public float effectDrawSize = 30f;

        public bool doDefaultBehavior = true;

        public SoundStyle? deathSound;
        public int currentState
        {
            get => (int)Projectile.ai[0];
            set => Projectile.ai[0] = (float)value;
        }

        public int timeUntilCanTileCollide = 15;

        public override void SetDefaults()
        {
            Projectile.hostile = true;
            Projectile.tileCollide = true;
            SetProjectileProperties();
            Projectile.width = width;
            Projectile.height = height;
        }

        public override void OnSpawn(IEntitySource source)
        {
            SetDefaults();
        }

        public virtual void HitPlayer(Player target, Player.HurtInfo info)
        {

        }

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            if(timeUntilCanTileCollide > 0)
            {
                return false;
            }
            return true;
        }

        public virtual void AdditionalBehavior()
        {

        }

        /// <summary>
        /// Set the projectile properties in here. Do NOT use SetDefaults().
        /// You have to set projName, goreAmount, width, and height
        /// </summary>
        public virtual void SetProjectileProperties()
        {

        }

        public override bool PreKill(int timeLeft)
        {
            if (!Main.dedServ)
            {
                KillandSpawnGore();
            }
            return true;
        }

        public void KillandSpawnGore()
        {
            if (!projName.Equals(""))
            {
                for (int i = 0; i < goreAmount; i++) 
                {
                    int gore = Mod.Find<ModGore>(projName+"Gore"+(i+1)).Type;
                    var entitySource = Projectile.GetSource_Death();
                    Gore.NewGore(entitySource, Projectile.Center, new Vector2(Main.rand.Next(-6, 7), Main.rand.Next(-6, 7)), gore);
                }
            }
            if(deathSound != null)
            {
                SoundEngine.PlaySound(deathSound, Projectile.Center);
            }
        }

        public override void AI()
        {
            if (doDefaultBehavior)
            {
                if (Projectile.velocity.Y <= maxYVelocity)
                {
                    Projectile.velocity.Y += gravity;
                }
                Projectile.rotation += rotationalForce;
                if (timeUntilCanTileCollide > 0)
                {
                    timeUntilCanTileCollide--;
                }
            }
            AdditionalBehavior();
        }

        public override bool PreDraw(ref Color lightColor)
        {
            var ballTexture = TextureAssets.Extra[91].Value;
            float scale = (Projectile.Hitbox.Width / (float)ballTexture.Height) * effectDrawSize;
            var rect = new Rectangle(0, 0, ballTexture.Width, ballTexture.Height);
            Vector2 origin = rect.Size() / 2;
            origin.Y -= rect.Height / 4;
            Color col = KiryuPNG.stateInfos[currentState].stateColor;
            col.A = 30;
            col *= Projectile.Opacity;

            var pos = Projectile.Center;// + new Vector2(0, -50f);

            Main.spriteBatch.Draw(ballTexture, pos - Main.screenPosition, rect, col,
                Projectile.velocity.ToRotation() + (float)Math.PI/2f, origin, scale, SpriteEffects.None, 0f);

            for (int i = 0; i < 3; i++)
            {
                float time = (float)(Main.timeForVisualEffects + (15 * i)) % 30 / 30f;
                float newScale = scale * time;
                float newOpacity = Projectile.Opacity * (1f - time);
                col = (col * 8) * newOpacity;
                Main.spriteBatch.Draw(ballTexture, pos - Main.screenPosition, rect, col,
                    Projectile.velocity.ToRotation() + (float)Math.PI / 2f, origin, newScale, SpriteEffects.None, 0f); 
            }
            return true;
        }

    }
}
