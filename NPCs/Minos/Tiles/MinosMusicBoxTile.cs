using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using Terraria.GameContent.Drawing;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.Localization;
using Terraria;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.Utilities;
using TestContent.Items.Placeables.Furniture;
using Microsoft.Xna.Framework;
using TestContent.NPCs.KiryuPNG.Items;
using ReLogic.Content;
using TestContent.NPCs.Minos.Items.BossDecorum;

namespace TestContent.NPCs.Minos.Tiles
{
    public class MinosMusicBoxTile : ModTile
    {
        public override string HighlightTexture => "TestContent/ExtraTextures/InvisibleSprite";
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileObsidianKill[Type] = true;
            TileID.Sets.HasOutlines[Type] = true;
            TileID.Sets.DisableSmartCursor[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
            TileObjectData.newTile.Origin = new Point16(0, 1);
            TileObjectData.newTile.LavaDeath = false;
            TileObjectData.newTile.DrawYOffset = 2;
            TileObjectData.newTile.StyleLineSkip = 2;
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(255, 215, 191), Language.GetText("Mods.TestContent.Items.MinosMusicBoxItem.DisplayName"));
        }

        public override void MouseOver(int i, int j)
        {
            Player player = Main.LocalPlayer;
            player.noThrow = 2;
            player.cursorItemIconEnabled = true;
            player.cursorItemIconID = ModContent.ItemType<MinosMusicBoxItem>();
        }

        public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings)
        {
            return true;
        }
        public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY)
        {
            base.SetDrawPositions(i, j, ref width, ref offsetY, ref height, ref tileFrameX, ref tileFrameY);
        }
        public override void AnimateTile(ref int frame, ref int frameCounter)
        {
            if (++frameCounter >= 15)
            {
                frameCounter = 0;
                frame = ++frame % 8;
            }
        }

        public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset)
        {
            var tile = Main.tile[i, j];
            //Main.NewText($"{i} {j}");
            frameYOffset = Main.tileFrame[type] * 36;
        }
        public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
        {
            // This code spawns the music notes when the music box is open.
            if (Lighting.UpdateEveryFrame && new FastRandom(Main.TileFrameSeed).WithModifier(i, j).Next(4) != 0)
            {
                return;
            }

            Tile tile = Main.tile[i, j];

            if (!TileDrawing.IsVisible(tile) || tile.TileFrameX != 36 || tile.TileFrameY % 36 != 0 || (int)Main.timeForVisualEffects % 7 != 0 || !Main.rand.NextBool(3))
            {
                return;
            }

            int MusicNote = Main.rand.Next(570, 573);
            Vector2 SpawnPosition = new Vector2(i * 16 + 8, j * 16 - 8);
            Vector2 NoteMovement = new Vector2(Main.WindForVisuals * 2f, -0.5f);
            NoteMovement.X *= Main.rand.NextFloat(0.5f, 1.5f);
            NoteMovement.Y *= Main.rand.NextFloat(0.5f, 1.5f);
            switch (MusicNote)
            {
                case 572:
                    SpawnPosition.X -= 8f;
                    break;
                case 571:
                    SpawnPosition.X -= 4f;
                    break;
            }

            Gore.NewGore(new EntitySource_TileUpdate(i, j), SpawnPosition, NoteMovement, MusicNote, 0.8f);
        }
    }
}
