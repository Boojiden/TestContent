using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using TestContent.UI;

namespace TestContent.Items
{
    public class TrollMessage: ModItem
    {

        public int image = 0;
        public override void SaveData(TagCompound tag)
        {
            tag["image"] = image;
        }

        public override void LoadData(TagCompound tag)
        {
            image = tag.Get<int>("image");
        }

        public override void NetSend(BinaryWriter writer)
        {
            writer.Write(image);
        }

        public override void NetReceive(BinaryReader reader)
        {
            image = reader.ReadInt32();
        }

        public override void OnCreated(ItemCreationContext context)
        {
            //GetRandomImage();
        }

        public override void OnSpawn(IEntitySource source)
        {
            GetRandomImage();
        }

        public void GetRandomImage()
        {
            image = Main.rand.Next(0, TrollMessageUI.MaxImages + 1);
            //Main.NewText($"Spawned with Image {image}");
        }
        public override void SetDefaults() 
        {
            Item.width = 44;
            Item.height = 24;
            Item.rare = ItemRarityID.Blue;
            Item.value = 0;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 3;
            Item.useAnimation = 3;
        }

        public override bool? UseItem(Player player)
        {
            if(Main.myPlayer == player.whoAmI)
            {
                ModContent.GetInstance<TrollMessageUISystem>().ToggleUI(image);
            }
            return true;
        }
    }
}
