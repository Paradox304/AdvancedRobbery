using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedRobbery
{
    public class DropItem
    {
        public Byte page { get; set; }
        public Byte index { get; set; }
        public ushort id { get; set; }
        public DropItem()
        {

        }
        public DropItem(Byte Page, Byte Index, ushort ID)
        {
            page = Page;
            index = Index;
            id = ID;
        }
    }
}
