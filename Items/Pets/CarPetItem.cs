using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using Terraria.Enums;
using Terraria.Audio;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;

namespace TestContent.Items.Pets
{
    public class CarPetItem : ModItem
    {

        
        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.ZephyrFish); // Copy the Defaults of the Zephyr Fish Item.
            Item.UseSound = null;
            Item.shoot = ModContent.ProjectileType<CarPetProjectile>(); // "Shoot" your pet projectile.
            Item.buffType = ModContent.BuffType<CarPetBuff>(); // Apply buff upon usage of the Item.
            Item.value = Item.sellPrice(silver: 1);
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                player.AddBuff(Item.buffType, 3600);
            }
            //Main.NewText("WHAT");
            if (Main.rand.NextFloat() < 0.01f)
            {
                if (Main.netMode == NetmodeID.SinglePlayer)
                {
                    int who = Projectile.NewProjectile(player.GetSource_ItemUse(Item), player.position, Vector2.Zero, ModContent.ProjectileType<CarRare>(), 9999, 1, Main.myPlayer);
                    SoundEngine.PlaySound(CarRare.Horn, player.position);
                }
                else if (Main.netMode == NetmodeID.Server)
                {
                    int who = Projectile.NewProjectile(player.GetSource_ItemUse(Item), player.position, Vector2.Zero, ModContent.ProjectileType<CarRare>(), 9999, 1, Main.myPlayer);
                    NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, who);
                    ModPacket packet = Mod.GetPacket();

                    packet.Write((byte)0);
                    packet.Send(0);
                }
            }
            else
            {
                int sound = Main.rand.Next(1, 4);
                if (Main.netMode == NetmodeID.Server)
                {
                    ModPacket packet = Mod.GetPacket();
                    packet.Write((byte)1);
                    packet.Write((byte)sound);
                    packet.Write((int)player.whoAmI);
                    packet.Send();
                }
                else if (Main.netMode == NetmodeID.SinglePlayer)
                {
                    switch (sound)
                    {
                        case 1:
                            SoundEngine.PlaySound(CarPetProjectile.Car1, player.position);
                            break;
                        case 2:
                            SoundEngine.PlaySound(CarPetProjectile.Car2, player.position);
                            break;
                        case 3:
                            SoundEngine.PlaySound(CarPetProjectile.Car3, player.position);
                            break;
                    }
                }
            }
            return true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            
            Projectile.NewProjectile(player.GetSource_ItemUse(Item), player.position, Vector2.Zero, Item.shoot, 0, 0, player.whoAmI);
            return false;
        }

        // Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddRecipeGroup(RecipeGroupID.IronBar, 1)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}
