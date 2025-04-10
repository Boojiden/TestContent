using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace TestContent.Projectiles.Weapons.Gambling.Prehardmode
{
    public class DeathCoinProjectile : ModProjectile
    {

        public bool Triggered
        {
            get => Projectile.ai[0] == 1f;
            set => Projectile.ai[0] = value ? 1f : 0f;
        }

        public bool Heads
        {
            get => Projectile.ai[1] == 1f;
            set => Projectile.ai[1] = value ? 1f : 0f;
        }

        public int TriggeredTimer => (int)Projectile.localAI[0];

        public int triggeredMaxTime = 90;

        public Asset<Texture2D> glyph;

        public SoundStyle win, lose;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 9;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = 1;
            Projectile.stopsDealingDamageAfterPenetrateHits = true;
            glyph = ModContent.Request<Texture2D>("TestContent/Projectiles/Weapons/Gambling/Prehardmode/DeathCoinGlyph");

            win = new SoundStyle("TestContent/Assets/Sounds/WinFlip")
            {
                Volume = 0.6f,
                PitchVariance = 0.3f,
                PlayOnlyIfFocused = true
            };
            lose = new SoundStyle("TestContent/Assets/Sounds/LoseFlip")
            {
                Volume = 0.6f,
                PitchVariance = 0.3f,
                PlayOnlyIfFocused = true
            };
        }

        public override bool? CanDamage()
        {
            return Projectile.damage != 0;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            TriggerCoin();
            base.OnHitNPC(target, hit, damageDone);
        }

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            fallThrough = false;
            return base.TileCollideStyle(ref width, ref height, ref fallThrough, ref hitboxCenterFrac);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            TriggerCoin();
            return false;
        }

        public void TriggerCoin()
        {
            Projectile.damage = 0;
            Triggered = true;
            Projectile.velocity = Vector2.Zero;
            Projectile.timeLeft = triggeredMaxTime;
        }

        public void DoCoinEffects()
        {
            Projectile.alpha = 255;
            if (Heads)
            {
                if (Main.myPlayer == Projectile.owner)
                {
                    Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<DeathCoinExplosion>(), (int)(Projectile.originalDamage * 1.25), Projectile.knockBack);
                }
                SoundEngine.PlaySound(win, Projectile.Center);
            }
            else
            {
                Player owner = Main.player[Projectile.owner];
                SoundEngine.PlaySound(lose, Projectile.Center);
                if (!Main.dedServ)
                {
                    float num = 1f / 80f;
                    for (float num2 = 0f; num2 < 1f; num2 += num)
                    {
                        Dust.NewDustPerfect(Vector2.Lerp(Projectile.Center, owner.Center, num2), DustID.Clentaminator_Red).velocity *= 0.3f;
                    }
                    for (int i = 0; i < 15; i++)
                    {
                        var rand = Main.rand.NextFloat(10f, 12f);
                        var circRand = Main.rand.NextVector2Circular(rand, rand);
                        int dust = Dust.NewDust(Projectile.Center, 2, 2, DustID.Clentaminator_Red, circRand.X, circRand.Y);
                        Main.dust[dust].noGravity = true;
                    }
                }
                owner.Hurt(PlayerDeathReason.ByCustomReason($"{owner.name} paid the ultimate price"), owner.statLifeMax2 / 8, 1, armorPenetration: float.MaxValue);
            }
        }

        public override void AI()
        {
            Projectile.spriteDirection = Projectile.direction;
            if (!Triggered)
            {
                Projectile.timeLeft = 2;
                Projectile.velocity.Y += 0.3f;

                if (Projectile.frameCounter++ >= 10)
                {
                    Projectile.frameCounter = 0;
                    Projectile.frame = (Projectile.frame + 1) % Main.projFrames[Type];
                }
                Projectile.damage = 1;

                if (Projectile.frameCounter % 5 == 0 && !Main.dedServ)
                {
                    int dust1 = Dust.NewDust(Projectile.Center, 0, 0, DustID.PlatinumCoin);
                    Main.dust[dust1].noGravity = true;
                    int dust2 = Dust.NewDust(Projectile.Center, 0, 0, DustID.Shadowflame);
                    Main.dust[dust2].noGravity = true;
                }
            }
            else
            {
                Projectile.frame = 0;
                if (Projectile.timeLeft == triggeredMaxTime / 2)
                {
                    DoCoinEffects();
                }
            }

        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Triggered)
            {
                var texture = glyph.Value;
                int glyphFrame = Heads ? 1 : 0;
                Rectangle textureRect = new Rectangle(0, 0 + glyphFrame * texture.Height / 2, texture.Width, texture.Height / 2);
                Vector2 origin = textureRect.Size() / 2 + new Vector2(-4, -1);
                Vector2 pos = Projectile.Center;
                float time = 1f - Projectile.timeLeft / (float)triggeredMaxTime;
                float doubleTime = time * 2;
                float lerp = GlyphRaiseLerp(time);
                float height = 60 * lerp;
                pos.Y -= height;
                Color col = Color.White;
                col.A = 0;
                if (doubleTime <= 1f)
                {
                    col *= lerp;
                }
                else
                {
                    col *= 1f - GlyphRaiseLerp(doubleTime - 1f);
                }
                Main.spriteBatch.Draw(texture, pos - Main.screenPosition, textureRect, col, 0f, origin, lerp, SpriteEffects.None, 0f);
            }

            return true;
        }

        public float GlyphRaiseLerp(float time)
        {
            if (time > 1f)
            {
                return 1f;
            }
            if (time < 0f)
            {
                return 0f;
            }
            return 1f - (float)Math.Pow(2, -10 * time);
        }
    }
}
