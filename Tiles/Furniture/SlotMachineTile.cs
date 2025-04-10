using Terraria.ID;
using Terraria.Localization;
using Terraria.ObjectData;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using TestContent.UI;
using Terraria.Enums;
using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;
using Terraria.Utilities;
using TestContent.Dusts;
using Microsoft.Xna.Framework.Content;

namespace TestContent.Tiles.Furniture
{
    public class SlotMachineTile: ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
            TileObjectData.newTile.Width = 2;
            TileObjectData.newTile.Height = 3;
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16, 16};
            TileObjectData.newTile.LavaDeath = true;
            TileObjectData.newTile.CoordinatePadding = 2;
            TileObjectData.newTile.Direction = TileObjectDirection.PlaceLeft;
            TileObjectData.newTile.StyleWrapLimit = 2;
            TileObjectData.newTile.StyleMultiplier = 2;
            TileObjectData.newTile.StyleHorizontal = true;

            TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
            TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceRight;
            TileObjectData.addAlternate(1); // Facing right will use the second texture style
            TileObjectData.addTile(Type);
            DustType = DustID.WoodFurniture;
            AddMapEntry(Color.Red, Language.GetText("Mods.TestContent.Items.SlotMachine.DisplayName"));
        }

        public override void MouseOver(int i, int j)
        {
            Player player = Main.LocalPlayer;
            player.noThrow = 2;
            player.cursorItemIconEnabled = true;

            int style = TileObjectData.GetTileStyle(Main.tile[i, j]);
            player.cursorItemIconID = TileLoader.GetItemDropFromTypeAndStyle(Type, style);
        }

        public override bool RightClick(int i, int j)
        {
            var tile = Main.tile[i, j];
            Player player = Main.LocalPlayer;
            SoundEngine.PlaySound(SoundID.Mech, new Vector2(i * 16, j * 16));
            var slotmodplayer = player.GetModPlayer<SlotMachineSystem>();
            slotmodplayer.slotTile = tile;
            int left = i;
            var leftTile = Main.tile[i - 1, j];
            if (leftTile.HasTile && leftTile.TileType == Type)
            {
                left = i - 1;
            }
            int top = j;
            var minusOne = Main.tile[i, j+1];
            if(!minusOne.HasTile || minusOne.TileType != Type)
            {
                top--;
            }
            minusOne = Main.tile[i, top+2];
            if (!minusOne.HasTile || minusOne.TileType != Type)
            {
                top--;
            }
            slotmodplayer.dustRectangle = new Rectangle(left, top, 2, 3);
            slotmodplayer.blockInteractionPoint = new Point(i, j);
            slotmodplayer.system.ShowMyUI();
            return true;
        }

        public override void AnimateTile(ref int frame, ref int frameCounter)
        {
            if (++frameCounter >= 15)
            {
                frameCounter = 0;
                frame = ++frame % 2;
            }
        }

        public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset)
        {
            var tile = Main.tile[i, j];
            //Main.NewText($"{i} {j}");
            frameYOffset = Main.tileFrame[type] * 54;
        }

        public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
        {
            
        }
    }
}
