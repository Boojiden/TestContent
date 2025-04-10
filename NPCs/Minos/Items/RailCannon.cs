using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TestContent.Items;
using TestContent.NPCs.Minos.Projectiles.Friendly;
using TestContent.Players;
using TestContent.Utility;

namespace TestContent.NPCs.Minos.Items
{
    public class RailCannon : BasicHeldGunItem
    {
        public override void SetDefaults()
        {
            base.SetDefaults();
            Item.width = 100;
            Item.height = 34;
            Item.rare = ItemRarityID.Red;
            Item.value = Terraria.Item.buyPrice(gold: 20);
            Item.damage = 760;
            Item.DamageType = DamageClass.Magic;
            Item.useTime = 28;
            Item.useAnimation = 28;
            Item.knockBack = 1f;
            Item.shootSpeed = 0f;
            Item.shoot = ModContent.ProjectileType<RailCannonHeldProjectile>();
            Item.mana = 50;
        }

        public override bool CanUseItem(Player player)
        {
            return player.GetModPlayer<PlayerWeapons>().canUseRailCannon;
        }

        public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            var weapons = Main.LocalPlayer.GetModPlayer<PlayerWeapons>();
            if (weapons.canUseRailCannon)
            {
                return;
            }

            float time = 1f - GameplayUtils.GetTimeFromInts(weapons.railCannonCooldown, weapons.RAILCANNONMAXCOOLDOWN);

            var back = ModContent.Request<Texture2D>("TestContent/NPCs/Minos/Extras/RailCannonUIBack");
            var front = ModContent.Request<Texture2D>("TestContent/NPCs/Minos/Extras/RailCannonUIFront");

            var drawPos = position;

            Vector2 barOrigin = back.Value.Size() / 2f;
            Rectangle crop = new Rectangle(0, 0, back.Value.Width, (int)(back.Value.Height * time));

            float addedScale = 2f;

            spriteBatch.Draw(back.Value, drawPos, null, Color.White, 0f, barOrigin, scale * addedScale, SpriteEffects.None, 0f);
            spriteBatch.Draw(front.Value, drawPos, crop, Color.White, 0f, barOrigin, scale * addedScale, SpriteEffects.None, 0f);
        }
    }
}
