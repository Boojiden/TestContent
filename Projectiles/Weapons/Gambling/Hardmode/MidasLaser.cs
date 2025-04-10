using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using XPT.Core.Audio.MP3Sharp.Decoding.Decoders.LayerIII;

namespace TestContent.Projectiles.Weapons.Gambling.Hardmode
{
    public class MidasLaser : ModProjectile
    {

        public float lineLength = 42;
        public float lineWidth = 6;

        public int timer = 0;
        public override void SetDefaults()
        {
            Projectile.width = 2; // The width of projectile hitbox
            Projectile.height = 2; // The height of projectile hitbox
            Projectile.friendly = true; // Can the projectile deal damage to enemies?
            Projectile.hostile = false; // Can the projectile deal damage to the player?
            Projectile.DamageType = DamageClass.Ranged; // Is the projectile shoot by a ranged weapon?
            Projectile.penetrate = 1; // How many monsters the projectile can penetrate. (OnTileCollide below also decrements penetrate for bounces as well)
            Projectile.timeLeft = 600; // The live time for the projectile (60 = 1 second, so 600 is 10 seconds)
            Projectile.alpha = 0; // The transparency of the projectile, 255 for completely transparent. (aiStyle 1 quickly fades the projectile in) Make sure to delete this if you aren't using an aiStyle that fades in. You'll wonder why your projectile is invisible.
            //Projectile.light = 0.25f;
            Projectile.ignoreWater = true; // Does the projectile's speed be influenced by water?
            Projectile.tileCollide = true; // Can the projectile collide with tiles?
            Projectile.extraUpdates = 2; // Set to above 0 if you want the projectile to update multiple time in a frame
            // Act exactly like default Bullet
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            /*
            double rot = (double)Projectile.rotation;// - Math.PI/2;
            Vector2 start = Projectile.position;// + new Vector2((float)Math.Cos(rot), (float)Math.Sin(rot)) * (Projectile.height / 2) * Projectile.scale;
            rot = (double)Projectile.rotation;
            Vector2 end = start + new Vector2((float)Math.Cos(rot), (float)Math.Sin(rot)) * (Projectile.width) * Projectile.scale;

            //Dust.NewDustPerfect(start, DustID.GemRuby, Vector2.Zero).noGravity = true;
            //Dust.NewDustPerfect(end, DustID.GemSapphire, Vector2.Zero).noGravity = true;

            */
            float colpoint = 0f;
            return Collision.CheckAABBvAABBCollision(targetHitbox.TopLeft(), targetHitbox.Size(), projHitbox.TopLeft(), projHitbox.Size());
        }

        public override void OnSpawn(IEntitySource source)
        {
            base.OnSpawn(source);
        }

        public override bool PreAI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            Vector3 color = Color.Yellow.ToVector3();
            Lighting.AddLight(Projectile.Center, color);

            if (!Main.dedServ)
            {
                timer++;
                if (timer % 10 == 0)
                {
                    Dust.NewDust(Projectile.position, 0, 0, DustID.GoldCoin);
                    timer = 1;
                }
            }
            return false;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            var texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            var effects = Projectile.spriteDirection == 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            var offset = new Vector2(texture.Width, texture.Height);

            if (Projectile.spriteDirection == 0)
            {
                offset *= -1;
            }

            Main.EntitySpriteDraw(texture, Projectile.position - Main.screenPosition, null, Color.White, Projectile.rotation, offset, Projectile.scale, effects);
            return false;
        }
    }
}
