using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using rail;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.ModLoader;
using TestContent.Effects.IK;
using TestContent.NPCs.Minos.Dusts;
using TestContent.Utility;

namespace TestContent.NPCs.Minos.Projectiles.Friendly
{
    public class KnuckleBlasterHandProjectile : ModProjectile
    {
        public enum FistStage
        {
            Idle,
            Swing,
            BlowUp
        }

        public int SWINGTIME = 10;
        public int SWINGWINDOWLOWER = 30;
        public int SWINGWINDOWUPPER = 35;
        public int SWINGPUTBACK = 80;
        public int BLOWUPTIME = 70;
        public int BLOWUPRELOAD = 110;
        public int BLOWUPPUTBACK = 140;

        public float idleDistance = 30f;
        public float outstretchedDistance = 100f;
        public float blowUpOffset = 30f;

        public float armOffset = 8f;
        public float armVOffset = 2f;
        public float shoulderOffset = 10f;
        public override string Texture => "TestContent/NPCs/Minos/Extras/KnuckleHand";

        public SoundStyle BlowUpSound => new SoundStyle(ModUtils.GetSoundFileLocation("RocketExplosion"))
        {
            Volume = 0.4f,
            PlayOnlyIfFocused = true,
            PitchVariance = 0.5f,
            MaxInstances = 3
        };
        public SoundStyle PunchSound => new SoundStyle(ModUtils.GetSoundFileLocation("KnucklePunch"))
        {
            Volume = 0.4f,
            PlayOnlyIfFocused = true,
            PitchVariance = 0.5f,
            MaxInstances = 3
        };
        public SoundStyle ReloadSound => new SoundStyle(ModUtils.GetSoundFileLocation("KnuckleReload"))
        {
            Volume = 0.4f,
            PlayOnlyIfFocused = true,
            PitchVariance = 0.5f,
            MaxInstances = 3
        };

        public Vector2 nonOwnerMousePosition;
        public Vector2 shoulderPosition;

        public LimbCollection Limbs = new(new CyclicCoordinateDescentUpdateRule(0.85f, MathHelper.Pi*2), 36f, 34f);

        public int Timer
        {
            get
            {
                return (int)Projectile.ai[0];
            }
            set
            {
                Projectile.ai[0] = (float)value;
            }
        }

        public FistStage Stage
        {
            get
            {
                return (FistStage)Projectile.ai[1];
            }
            set
            {
                if(Main.myPlayer == Projectile.owner)
                {
                    Projectile.ai[1] = (float)value;
                    Timer = 0;
                    Projectile.netUpdate = true;
                }
            }
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.penetrate = -1;
            Projectile.ignoreWater = true;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override bool? CanDamage()
        {
            return Stage == FistStage.Swing && Timer < SWINGTIME;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.WriteVector2(nonOwnerMousePosition);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            nonOwnerMousePosition = reader.ReadVector2();
        }

        public override void AI()
        {
            Player Owner = Main.player[Projectile.owner];
            bool inUse = Stage != FistStage.Idle;
            Owner.itemAnimation = inUse ? 2 : 0;
            if (!Owner.active || Owner.dead || Owner.noItems || Owner.CCed)
            {
                Projectile.Kill();
                return;
            }

            Vector2 mouse;
            if(Main.myPlayer == Projectile.owner)
            {
                mouse = Main.MouseWorld;
                nonOwnerMousePosition = mouse;
            }
            else
            {
                mouse = nonOwnerMousePosition;
            }
            UpdatePlayerVisuals(Owner, out Vector2 rrp, mouse);
            if(Main.myPlayer == Projectile.owner)
            {
                switch (Stage)
                {
                    case FistStage.Idle:
                        if (PlayerInput.Triggers.Current.MouseLeft)
                        {
                            Stage = FistStage.Swing;
                        }
                        break;
                    case FistStage.Swing:
                        if (PlayerInput.Triggers.Current.MouseLeft && Timer > SWINGWINDOWLOWER && Timer < SWINGWINDOWUPPER)
                        {
                            Stage = FistStage.BlowUp;
                            Projectile.NewProjectile(Projectile.InheritSource(Entity), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<KnuckleBlasterExplosion>(), Projectile.damage, Projectile.knockBack);
                        }
                        break;
                    case FistStage.BlowUp:
                        break;
                }
            }

            switch (Stage) 
            {
                case FistStage.Idle:
                    MoveFistToPosition(rrp, mouse);
                    break;
                case FistStage.Swing:
                    DoSwingAnimation(rrp, mouse);
                    Timer++;
                    break;
                case FistStage.BlowUp:
                    DoBlowUpAnimation(Owner, rrp, mouse);
                    Timer++;
                    break;
            }

            DoIKUpdates(Owner, rrp, mouse);

            Projectile.timeLeft = 2;
            //Main.NewText($"{Projectile.Center}: {Stage.ToString()}: {Timer}");
            //Main.NewText(Projectile.damage);
        }

        private void DoIKUpdates(Player Owner, Vector2 rrp, Vector2 mouse)
        {
            Vector2 start = rrp;
            Vector2 end = Projectile.Center - Owner.velocity - (rrp.GetVectorPointingTo(mouse) * armOffset) + (rrp.GetVectorPointingTo(mouse).RotatedBy(Owner.direction * MathHelper.PiOver2) * armVOffset);

            var dir = start.GetVectorPointingTo(end);

            var midpoint = Vector2.Lerp(start, end, 0.5f);
            var pole = midpoint + dir.RotatedBy(Owner.direction * MathHelper.PiOver2) * 15f;

            float rot = (float)Limbs.Limbs[0].Rotation;
            if ((pole - start).AngleBetween(pole - (start + rot.ToRotationVector2() * (float)Limbs.Limbs[0].Length)) < MathHelper.PiOver2)
            {
                Limbs.Limbs[0].Rotation = start.GetVectorPointingTo(pole).ToRotation();
            }

            Limbs.Update(start, end, pole);
        }

        private void UpdatePlayerVisuals(Player Owner, out Vector2 rrp, Vector2 mouse)
        {
            rrp = Owner.RotatedRelativePoint(Owner.MountedCenter, addGfxOffY: true);
            //Owner.heldProj = Projectile.whoAmI;
            var OwnerDir = Owner.MountedCenter.GetVectorPointingTo(mouse);
            var ProjDir = Owner.MountedCenter.GetVectorPointingTo(Projectile.Center);
            Owner.direction = Math.Sign(OwnerDir.X) == 0 ? 1 : Math.Sign(OwnerDir.X);
            Projectile.rotation = ProjDir.ToRotation();
            rrp = Owner.RotatedRelativePoint(Owner.MountedCenter, addGfxOffY: true) + new Vector2(Owner.direction * shoulderOffset, 0f);
            shoulderPosition = rrp;
        }

        private void DoBlowUpAnimation(Player Owner, Vector2 rrp, Vector2 mouse)
        {
            var idle = rrp + rrp.GetVectorPointingTo(mouse) * idleDistance;
            var outstretched = rrp + rrp.GetVectorPointingTo(mouse) * outstretchedDistance;
            var midpoint = rrp + rrp.GetVectorPointingTo(mouse) * MathHelper.Lerp(idleDistance, outstretchedDistance, 0.5f);
            var upper = idle + rrp.GetVectorPointingTo(mouse).RotatedBy(-MathHelper.PiOver2 * Owner.direction) * blowUpOffset;
            if (!Main.dedServ)
            {
                if (Timer == 0)
                {
                    SoundEngine.PlaySound(BlowUpSound, Projectile.Center);
                }
                if (Timer == BLOWUPTIME)
                {
                    SoundEngine.PlaySound(ReloadSound, Projectile.Center);
                    for (int i = 0; i < 2; i++)
                    {
                        Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<ShellDust>(), (Projectile.Center.GetVectorPointingTo(rrp).RotatedBy(Owner.direction * MathHelper.PiOver2) * 10f).RotatedByRandom(MathHelper.Pi / 16f));
                    }
                }
            }
            if(Timer < BLOWUPTIME)
            {
                Projectile.Center = Vector2.Lerp(outstretched, upper, MathHelper.Hermite(0f, 10f, 1f, 0f, GameplayUtils.GetTimeFromInts(Timer, BLOWUPTIME)));
            }
            else if (Timer < BLOWUPRELOAD) 
            {
                Projectile.Center = Vector2.Lerp(upper, idle, MathHelper.Hermite(0f, 6.7f, 0f, 0f, GameplayUtils.GetTimeFromInts(Timer - BLOWUPTIME, BLOWUPRELOAD - BLOWUPTIME)));
            }
            else
            {
                Projectile.Center = Vector2.Lerp(upper, idle, MathHelper.SmoothStep(0f, 1f, GameplayUtils.GetTimeFromInts(Timer - BLOWUPRELOAD, BLOWUPPUTBACK - BLOWUPRELOAD)));
            }

            if(Timer >= BLOWUPPUTBACK)
            {
                Stage = FistStage.Idle;
                Projectile.ResetLocalNPCHitImmunity();
            }
        }
        private void DoSwingAnimation(Vector2 rrp, Vector2 mouse)
        {
            Vector2 initpos = rrp + rrp.GetVectorPointingTo(mouse) * idleDistance;
            Vector2 targetPos = rrp + rrp.GetVectorPointingTo(mouse) * outstretchedDistance;
            if (!Main.dedServ)
            {
                if(Timer == 0)
                {
                    SoundEngine.PlaySound(PunchSound, Projectile.Center);
                }
            }
            if (Timer < SWINGTIME)
            {
                Projectile.Center = Vector2.Lerp(initpos, targetPos, MathHelper.Hermite(0f, 3f, 1f, 0f, GameplayUtils.GetTimeFromInts(Timer, SWINGTIME)));
            }
            else
            {
                Projectile.Center = Vector2.Lerp(targetPos, initpos, GameplayUtils.GetTimeFromInts(Timer - SWINGTIME, SWINGPUTBACK - SWINGTIME));
            }
            if (Timer >= SWINGPUTBACK)
            {
                Stage = FistStage.Idle;
                Projectile.ResetLocalNPCHitImmunity();
            }
        }

        private void MoveFistToPosition(Vector2 rrp, Vector2 mouse)
        {
            Vector2 targetPos;
            targetPos = rrp + rrp.GetVectorPointingTo(mouse) * idleDistance;
            Projectile.Center = targetPos;

        }

        public override bool PreDraw(ref Color lightColor)
        {
            Player Owner = Main.player[Projectile.owner];
            Vector2 mouse;
            if (Main.myPlayer == Projectile.owner)
            {
                mouse = Main.MouseWorld;
                nonOwnerMousePosition = mouse;
            }
            else
            {
                mouse = nonOwnerMousePosition;
            }

            var shoulder = ModContent.Request<Texture2D>("TestContent/NPCs/Minos/Extras/KnuckleShoulder");
            var shoulderPos = shoulderPosition - Main.screenPosition;
            Color shoulderColor = Lighting.GetColor(Limbs.ConnectPoint.ToTileCoordinates());
            var shoulderOrigin = shoulder.Value.Size() / 2f;
            Main.EntitySpriteDraw(shoulder.Value, shoulderPos, null, shoulderColor, 0f, shoulderOrigin, Projectile.scale, Owner.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally);

            for (int i = 0; i < Limbs.Limbs.Length; i++)
            {
                Asset<Texture2D> armTexture;
                if(i == 1)
                {
                    armTexture = ModContent.Request<Texture2D>("TestContent/NPCs/Minos/Extras/KnuckleUpperArm");
                }
                else
                {
                    armTexture = ModContent.Request<Texture2D>("TestContent/NPCs/Minos/Extras/KnuckleForeArm");
                }

                Color armColor = Lighting.GetColor(Limbs[i].ConnectPoint.ToTileCoordinates());
                var armPos = Limbs[i].MidPoint - Main.screenPosition;
                var armOrigin = armTexture.Size() / 2f;
                if(i == 0)
                {
                    armOrigin.Y += 4f * Owner.direction;
                }
                Main.EntitySpriteDraw(armTexture.Value, armPos, null, armColor, (float)Limbs[i].Rotation, armOrigin, Projectile.scale, Owner.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically);
                //Main.EntitySpriteDraw(TextureAssets.MagicPixel.Value, armPos, new Rectangle(0, 0, 1, 1), Color.White, 0f, new Vector2(0.5f, 0.5f), 1f, SpriteEffects.None);
                //Main.NewText($"{i} : {Limbs[i].ConnectPoint}");
            }

            var jointTexture = ModContent.Request<Texture2D>("TestContent/NPCs/Minos/Extras/KnuckleHand");
            var jointPos = Limbs.EndPoint - Main.screenPosition;
            var jointOrigin = jointTexture.Value.Size() / 2f;
            jointOrigin.X -= ((float)jointTexture.Value.Width / 4f);

            Main.EntitySpriteDraw(jointTexture.Value, jointPos, null, lightColor, Projectile.rotation, jointOrigin, Projectile.scale, Owner.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically);

            //TestContent.CenteredProjectileDraw(Projectile, lightColor, effects: Owner.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically);

            
            return false;
        }
    }
}
