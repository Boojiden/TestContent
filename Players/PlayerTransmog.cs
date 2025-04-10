using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using TestContent.Items.Transmog;

namespace TestContent.Players
{
    public class PlayerTransmog : ModPlayer
    {
        public int lastHeldItem;

        /// <summary>
        /// For dictating which items can transform<br/>
        /// Key: item ID of an item which can be transformed<br/>
        /// Value: item ID of the item which it transforms into
        /// </summary>
        public static Dictionary<int, int> transmogs;

        public override void Load()
        {
            transmogs = new Dictionary<int, int>();
        }

        public override void OnEnterWorld()
        {
            transmogs[ItemID.ManaCrystal] = ModContent.ItemType<TransmogStar>();
            transmogs[ItemID.FrozenTurtleShell] = ModContent.ItemType<TransmogShell>();
        }

        public override void PostUpdate()
        {
            /*if (Main.mouseItem.netID != lastHeldItem)
            {
                if(lastHeldItem == ModContent.ItemType<TransmogItem>() && transmogs.ContainsKey(Main.mouseItem.netID))
                {
                    Main.NewText("erm");
                    int newItem = transmogs[Main.mouseItem.netID];
                    Main.mouseItem.ChangeItemType(newItem);
                }
                lastHeldItem = Main.mouseItem.netID;
            }*/
        }

        public override bool HoverSlot(Item[] inventory, int context, int slot)
        {
            if(Main.mouseItem.netID == ModContent.ItemType<TransmogItem>())
            {
                int netID = inventory[slot].netID;
                if (transmogs.ContainsKey(inventory[slot].netID) && PlayerInput.Triggers.JustPressed.MouseRight)
                {
                    int newItem = transmogs[netID];
                    int amount = inventory[slot].stack;
                    inventory[slot].ChangeItemType(newItem);
                    inventory[slot].stack = amount;
                    SoundEngine.PlaySound(SoundID.Item122, Player.Center);
                }
            }
            return base.HoverSlot(inventory, context, slot);
        }
    }
}
