using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.Audio;
using System.IO;
using System.Numerics;

namespace TestContent.Items.Pets
{
    public class CarPetProjectile : ModProjectile
    {
        public static SoundStyle Car1, Car2, Car3, CarAnguish, CarThrowUp;

        public int timeUntilDialogue;
        public int maxWaitTime = 14400;
        public int minWaitTime = 10800;

        public int timeUntilSpawnItem;
        public int spawnMinWait = 15000;
        public int spawnMaxWait = 22000;

        public const int maxDialogue = 21;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1;
            Main.projPet[Projectile.type] = true;

            // This code is needed to customize the vanity pet display in the player select screen. Quick explanation:
            // * It uses fluent API syntax, just like Recipe
            // * You start with ProjectileID.Sets.SimpleLoop, specifying the start and end frames as well as the speed, and optionally if it should animate from the end after reaching the end, effectively "bouncing"
            // * To stop the animation if the player is not highlighted/is standing, as done by most grounded pets, add a .WhenNotSelected(0, 0) (you can customize it just like SimpleLoop)
            // * To set offset and direction, use .WithOffset(x, y) and .WithSpriteDirection(-1)
            // * To further customize the behavior and animation of the pet (as its AI does not run), you have access to a few vanilla presets in DelegateMethods.CharacterPreview to use via .WithCode(). You can also make your own, showcased in MinionBossPetProjectile
            ProjectileID.Sets.CharacterPreviewAnimations[Projectile.type] = ProjectileID.Sets.SimpleLoop(0, Main.projFrames[Projectile.type], 6)
                .WithOffset(-10, -20f)
                .WithSpriteDirection(-1)
                .WithCode(DelegateMethods.CharacterPreview.Float);
        }

        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.ZephyrFish); // Copy the stats of the Zephyr Fish
            Projectile.scale = 0.8f;
            AIType = ProjectileID.ZephyrFish; // Mimic as the Zephyr Fish during AI.
            Car1 = new SoundStyle("TestContent/Assets/Sounds/Car1")
            {
                Volume = 0.8f
            };
            Car2 = new SoundStyle("TestContent/Assets/Sounds/Car2")
            {
                Volume = 0.8f
            };
            Car3 = new SoundStyle("TestContent/Assets/Sounds/Car3")
            {
                Volume = 0.8f
            };
            CarAnguish = new SoundStyle("TestContent/Assets/Sounds/CarAnguish")
            {
                Volume = 0.8f
            };
            CarThrowUp = new SoundStyle("TestContent/Assets/Sounds/CarThrowUp")
            {
                Volume = 0.8f
            };
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(timeUntilDialogue);
            writer.Write(timeUntilSpawnItem);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            timeUntilDialogue = reader.ReadInt32();
            timeUntilSpawnItem = reader.ReadInt32();
        }


        public override void OnSpawn(IEntitySource source)
        {
            timeUntilDialogue = Main.rand.Next(minWaitTime, maxWaitTime);
            timeUntilSpawnItem = Main.rand.Next(spawnMinWait, spawnMaxWait);
        }

        public override void OnKill(int timeLeft)
        {
            Player player = Main.player[Projectile.owner];
            if (Main.rand.NextFloat() < 0.01f)
            {
                SoundEngine.PlaySound(CarAnguish, player.position);
            }
        }

        public override bool PreAI()
        {
            Player player = Main.player[Projectile.owner];
            //player.QuickSpawnItem
            player.zephyrfish = false; // Relic from AIType

            return true;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            // Keep the projectile from disappearing as long as the player isn't dead and has the pet buff.
            if (!player.dead && player.HasBuff(ModContent.BuffType<CarPetBuff>()))
            {
                Projectile.timeLeft = 2;
            }

            timeUntilDialogue--;
            timeUntilSpawnItem--;
            //Main.NewText(timeUntilDialogue);
            if (timeUntilDialogue < 0)
            {
                int line = Main.rand.Next(1, maxDialogue + 1);
                SoundStyle dialogue = new SoundStyle("TestContent/Assets/Sounds/DumbCat/CarTalk" + line)
                {
                    Volume = 0.8f
                };
                SoundEngine.PlaySound(dialogue, player.Center);
                timeUntilDialogue = Main.rand.Next(minWaitTime, maxWaitTime);
                Projectile.netUpdate = true;
            }

            if (timeUntilSpawnItem < 0 && !Main.dedServ)
            {
                SoundEngine.PlaySound(CarThrowUp, player.Center);
                if(Main.myPlayer == Projectile.owner)
                {
                    int num = Item.NewItem(Projectile.GetSource_FromThis(), Projectile.position, ModContent.ItemType<Artifacting>());
                    if (Main.netMode == NetmodeID.MultiplayerClient)
                    {
                        NetMessage.SendData(MessageID.SyncItem, -1, -1, null, num, 1f);
                    }
                }
                timeUntilSpawnItem = Main.rand.Next(spawnMinWait, spawnMaxWait);
                Projectile.netUpdate = true;
            }
            /*
            if (Main.netMode != NetmodeID.Server && Main.myPlayer == Projectile.owner)
            {
                
            }
            */
        }
    }
}
