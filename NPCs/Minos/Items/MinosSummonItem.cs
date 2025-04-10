using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using TestContent.Items;
using TestContent.NPCs.KiryuPNG;

namespace TestContent.NPCs.Minos.Items
{
    public class MinosSummonItem : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 3;
            ItemID.Sets.SortingPriorityBossSpawns[Type] = 24;
        }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.maxStack = 20;
            Item.value = 100;
            Item.rare = ItemRarityID.Red;
            Item.useAnimation = 30;
            Item.useTime = 30;
            Item.useStyle = ItemUseStyleID.HoldUp;
        }

        public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
        {
            itemGroup = ContentSamples.CreativeHelper.ItemGroup.BossSpawners;
        }

        public override bool CanUseItem(Player player)
        {
            return !NPC.AnyNPCs(ModContent.NPCType<MinosPrime>()) && !NPC.AnyNPCs(ModContent.NPCType<SoulOrb>());
        }

        public override bool AltFunctionUse(Player player)
        {
            return true;
        }

        public override bool? UseItem(Player player)
        {
            //SoundEngine.PlaySound(KiryuBossSounds.summon, player.position);
            //Summon Minos Directly
            if(Main.myPlayer == player.whoAmI)
            {
                if (player.altFunctionUse == 2)
                {
                    int type = ModContent.NPCType<MinosPrime>();

                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        NPC.SpawnOnPlayer(player.whoAmI, type);
                    }
                    else
                    {
                        NetMessage.SendData(MessageID.SpawnBossUseLicenseStartEvent, number: player.whoAmI, number2: type);
                    }
                }
                else //Intro Sequence
                {
                    int type = ModContent.NPCType<SoulOrb>();

                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        NPC.NewNPC(NPC.GetBossSpawnSource(player.whoAmI), (int)player.position.X, (int)(player.position.Y - SoulOrb.UpperOffset), ModContent.NPCType<SoulOrb>(), ai2: player.whoAmI);
                    }
                    else
                    {
                        ModPacket packet = Mod.GetPacket();
                        packet.Write((byte)TestContent.NetMessageType.MinosBossIntro);
                        packet.Write((byte)(player.whoAmI));
                        packet.Send();
                    }
                }
            }
            return true;
        }

        // Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<Artifacting>(10)
                .AddIngredient(ItemID.FragmentSolar, 5)
                .AddIngredient(ItemID.FragmentNebula, 5)
                .AddIngredient(ItemID.FragmentStardust, 5)
                .AddIngredient(ItemID.FragmentVortex, 5)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
}
