using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;

namespace TestContent.Effects.Music
{
    public abstract class BaseMusicSceneEffect : ModSceneEffect
    {

        #region Overridable Properties
        public abstract int NPCType { get; }
        public abstract int ModMusic { get; }
        public virtual int MusicDistance => 5000;
        public virtual int[] AdditionalNPCs => new int[] { };
        #endregion

        #region Overridable Methods
        public virtual bool AdditionalCheck() => true;
        #endregion
        public virtual int SetMusic()
        {
            return ModMusic;
        }
        public virtual bool SetSceneEffect(Player player)
        {
            if (!AdditionalCheck())
                return false;

            Rectangle screenRect = new Rectangle((int)Main.screenPosition.X, (int)Main.screenPosition.Y, Main.screenWidth, Main.screenHeight);
            int musicDistance = MusicDistance * 2;
            foreach (NPC npc in Main.ActiveNPCs)
            {
                bool inList = false;
                if (npc.type == NPCType)
                {
                    inList = true;
                }
                else
                {
                    for (int i = 0; i < AdditionalNPCs.Length; i++)
                    {
                        if (npc.type == AdditionalNPCs[i])
                        {
                            inList = true;
                            break;
                        }
                    }
                }

                if (!inList)
                    continue;

                Rectangle npcBox = new Rectangle((int)npc.Center.X - MusicDistance, (int)npc.Center.Y - MusicDistance, musicDistance, musicDistance);
                if (screenRect.Intersects(npcBox))
                    return true;
            }
            return false;
        }

        public override int Music => SetMusic();

        public override bool IsSceneEffectActive(Player player) => SetSceneEffect(player);
    }
}
