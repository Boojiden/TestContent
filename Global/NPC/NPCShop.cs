using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TestContent.Items.Ammo;
using TestContent.Items.Consumables;
using TestContent.Items.Placeables.Furniture;
using TestContent.Items.Placeables.Furniture.Paintings;
using TestContent.Items.Weapons;

namespace TestContent.Global.NPC
{
    public class NPCShop : GlobalNPC
    {
        public override void ModifyShop(Terraria.ModLoader.NPCShop shop)
        {
            if(shop.NpcType == NPCID.Merchant)
            {
                shop.Add(new Item(ModContent.ItemType<CokeAndFriesHalo>()), Condition.Hardmode);
            }

            if(shop.NpcType == NPCID.Painter)
            {
                shop.Add(new Item(ModContent.ItemType<TheGreenOneItem>()), Condition.BloodMoon);
            }

            if(shop.NpcType == NPCID.GoblinTinkerer)
            {
                shop.Add(new Item(ModContent.ItemType<SlotMachine>()), Condition.TimeNight);
            }

            if(shop.NpcType == NPCID.ArmsDealer)
            {
                shop.Add(new Item(ModContent.ItemType<ChipAmmo>()), Condition.PlayerCarriesItem(ModContent.ItemType<RouletteGun>()));
            }
        }
    }
}
