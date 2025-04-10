using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace TestContent.Projectiles.Weapons.Gambling.Prehardmode
{
    public class MistFiner : ModProjectile
    {
        public bool doSpawnParticles = true;
        public Vector2 targetPos
        {
            get
            {
                return new Vector2(Projectile.ai[0], Projectile.ai[1]);
            }
            set
            {
                Projectile.ai[0] = value.X;
                Projectile.ai[1] = value.Y;
            }
        }

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 6;
        }

        public float length = 20f;
        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.aiStyle = -1;
            Projectile.noEnchantmentVisuals = true;
        }

        public override bool? CanDamage()
        {
            return Projectile.frame == 2 || Projectile.frame == 3;
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (Main.expertMode)
            {
                if (target.type >= NPCID.EaterofWorldsHead && target.type <= NPCID.EaterofWorldsTail)
                {
                    modifiers.FinalDamage /= 5;
                }
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            var dir = (targetPos - Projectile.position).SafeNormalize(Vector2.UnitY);
            Vector2 end = Projectile.position + dir * 240f;
            //int dustID = DustID.Adamantite;
            //Dust.NewDustPerfect(Projectile.position, dustID);
            //Dust.NewDustPerfect(end, dustID);
            //Main.NewText($"{targetPos} {Projectile.position}");
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.position, end);
        }

        public override void AI()
        {
            var dir = (targetPos - Projectile.position).SafeNormalize(Vector2.UnitY);
            if (doSpawnParticles && !Main.dedServ)
            {
                for (int i = 0; i < 20; i++)
                {
                    var rand = Main.rand.NextFloat(10f, 12f);
                    var circRand = Main.rand.NextVector2Circular(rand, rand);
                    int dust = Dust.NewDust(Projectile.position, 2, 2, DustID.GiantCursedSkullBolt, circRand.X, circRand.Y);
                    Main.dust[dust].noGravity = true;
                }
                var dustThrow = dir * 30f;
                for (int i = 0; i < 20; i++)
                {
                    var rand = dustThrow * Main.rand.NextFloat();
                    int dust = Dust.NewDust(Projectile.position, 2, 2, DustID.GiantCursedSkullBolt, rand.X, rand.Y, Scale: 1.5f);
                    Main.dust[dust].noGravity = true;
                }
                doSpawnParticles = false;
            }
            Projectile.velocity = Vector2.Zero;

            Projectile.rotation = dir.ToRotation();

            if (Projectile.frameCounter++ > 5)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = Projectile.frame + 1;
                if (Projectile.frame > Main.projFrames[Type])
                {
                    Projectile.Kill();
                }
            }

            if ((bool)CanDamage() && Main.myPlayer == Projectile.owner)
            {
                Player player = Main.player[Projectile.owner];
                if ((bool)Colliding(Projectile.Hitbox, player.Hitbox))
                {
                    var pronoun = player.Male ? "his" : "her";
                    player.Hurt(PlayerDeathReason.ByCustomReason(player.name + " split " + pronoun + " own flesh"), Projectile.damage * 2, Projectile.direction);
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            int frameHeight = texture.Height / Main.projFrames[Projectile.type];
            int startY = frameHeight * Projectile.frame;
            Rectangle sourceRectangle = new Rectangle(1, startY, texture.Width, frameHeight);
            Vector2 origin = sourceRectangle.Size() / 2;

            var dir = (targetPos - Projectile.position).SafeNormalize(Vector2.UnitY);
            Vector2 visualOffset = dir * 240f;

            origin.X = Projectile.spriteDirection == 1 ? sourceRectangle.Width - 30 : 30;
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition + visualOffset,
            sourceRectangle, lightColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
    }
}
