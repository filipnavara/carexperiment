using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppleTools.Bom
{
    class BomBlock
    {
        internal BomBlock(BomFileReader reader, uint offset, uint length)
        {
            reader.Offset = offset;
            Stream = reader.ReadAsStream(length);
        }

        public Stream Stream { get; private set; }
    }
}
