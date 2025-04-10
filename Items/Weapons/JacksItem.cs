using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using TestContent.Buffs.Minions;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using TestContent.Players;
using TestContent.Projectiles.Weapons.Gambling.Prehardmode;
using TestContent.Projectiles.Weapons.Gambling.Hardmode;

namespace TestContent.Items.Weapons
{
    public class JacksItem: ModItem
    {
        public override void SetStaticDefaults()
        {
            ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true;
            ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;

            ItemID.Sets.StaffMinionSlotsRequired[Type] = 0f;
        }

        public override void SetDefaults()
        {
            Item.damage = 8;
            Item.DamageType = DamageClass.Summon;
            Item.mana = 5;
            Item.width = 40;
            Item.height = 38;
            Item.useTime = 36;
            Item.useAnimation = 36;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.UseSound = new Terraria.Audio.SoundStyle("TestContent/Assets/Sounds/CardDraw")
            {
                Volume = 0.8f,
                PlayOnlyIfFocused = true,
                PitchVariance = 0.3f
            };
            Item.noMelee = true;
            Item.knockBack = 1f;
            Item.value = Item.buyPrice(gold: 8);
            Item.rare = ItemRarityID.LightRed;
            Item.shoot = ModContent.ProjectileType<JackCardMinion>();
            Item.buffType = ModContent.BuffType<JacksMinionBuff>();
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            // Here you can change where the minion is spawned. Most vanilla minions spawn at the cursor position
            position = Main.MouseWorld;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            float cardType = (float)Main.rand.Next(0, 4);
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, ai1: cardType);

            var weapons = player.GetModPlayer<PlayerWeapons>();
            weapons.UpdateCardMinionCounts();
            return false;
        }
        public override bool? UseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                player.AddBuff(Item.buffType, 3600);
            }
            return true;
        }

        public override void HoldItem(Player player)
        {
            if(Main.myPlayer == player.whoAmI)
            {
                var weapons = player.GetModPlayer<PlayerWeapons>();
                Main.instance.MouseText($"Current Count: {weapons.CardMinionCount}");
            }
        }
    }
}
