using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace TestContent.Global
{
    public class TransmogRarity : ModRarity
    {
        public override Color RarityColor => new Color(255, 254, 211);

        public override int GetPrefixedRarity(int offset, float valueMult)
        {
            return Type;
        }
    }
}
