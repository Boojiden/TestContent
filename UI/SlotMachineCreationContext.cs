using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;

namespace TestContent.UI
{
    public class SlotMachineCreationContext : ItemCreationContext
    {
        public int betForItem = 0;
        public int index = 0;
        public SlotMachineCreationContext(int bet, int index)
        {
            betForItem = bet;
            this.index = index;
        }
    }
}
