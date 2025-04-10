using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using TestContent.Buffs;

namespace TestContent.Projectiles.Weapons.Transmog
{
    public class StarBuffContactDamage : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            //Main.projPet[Projectile.type] = true;
            ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;
        }

        public int realDamage = 60;
        public override void SetDefaults()
        {
            Projectile.width = 64;
            Projectile.height = 64;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.ownerHitCheck = true;
            Projectile.noEnchantmentVisuals = true;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Generic;
        }

        private bool CheckActive(Player owner)
        {
            if (owner.dead || !owner.active)
            {
                owner.ClearBuff(ModContent.BuffType<StarBuff>());

                return false;
            }

            if (owner.HasBuff(ModContent.BuffType<StarBuff>()))
            {
                Projectile.timeLeft = 2;
            }

            return true;
        }
        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            Projectile.Center = owner.Center;
            Projectile.damage = realDamage;
            CheckActive(owner);

            float goal = owner.velocity.ToRotation() + (float)Math.PI / 2f;
            if (Math.Abs(Projectile.rotation - goal) > 2)
            {
                Projectile.rotation = goal;
            }
            else
            {
                Projectile.rotation = MathHelper.Lerp(Projectile.rotation, goal, 0.2f);
            }
            //Main.NewText(goal);
            if (!Main.dedServ)
            {
                int dust = Dust.NewDust(Projectile.position, 75, 75, DustID.GemDiamond, Scale: 3f);
                Main.dust[dust].shader = GameShaders.Armor.GetShaderFromItemId(ItemID.HallowBossDye);
                Main.dust[dust].noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Player owner = Main.player[Projectile.owner];
            var dashColor = Color.White;
            var ballTexture = TextureAssets.Extra[91].Value;
            float scale = owner.velocity.Length() / 30f * 10f;
            //Main.NewText(scale);
            var rect = new Rectangle(0, 0, ballTexture.Width, ballTexture.Height);
            Vector2 origin = rect.Size() / 2;
            origin.Y -= rect.Height / 4;
            dashColor.A = 30;
            int intended = Main.CurrentDrawnEntityShader;
            Main.instance.PrepareDrawnEntityDrawing(Entity, GameShaders.Armor.GetShaderIdFromItemId(ItemID.HallowBossDye), null);
            Main.EntitySpriteDraw(ballTexture, owner.Center - Main.screenPosition, rect, dashColor,
            Projectile.rotation, origin, scale, SpriteEffects.None, 0f);
            Main.instance.PrepareDrawnEntityDrawing(Entity, intended, null);



            for (int i = 0; i < 3; i++)
            {
                float time = (float)(Main.timeForVisualEffects + 15 * i) % 30 / 30f;
                float newScale = scale * time;
                float newOpacity = 1f * (1f - time);
                dashColor = dashColor * 8 * newOpacity;
                Main.spriteBatch.Draw(ballTexture, owner.Center - Main.screenPosition, rect, dashColor,
                    Projectile.rotation, origin, newScale, SpriteEffects.None, 0f);
            }
            return false;
        }
    }
}
