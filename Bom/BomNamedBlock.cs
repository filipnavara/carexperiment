using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppleTools.Bom
{
    internal class BomNamedBlock
    {
        public BomNamedBlock(string name, uint blockIndex)
        {
            Name = name;
            BlockIndex = blockIndex;
        }

        public string Name { get; set; }

        public uint BlockIndex {  get; set; }
    }
}
