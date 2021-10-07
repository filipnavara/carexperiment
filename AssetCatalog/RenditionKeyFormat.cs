using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibObjectFile;

namespace AppleTools.AssetCatalog
{
    internal class RenditionKeyFormat
    {
        public RenditionKeyFormat(Stream stream)
        {
            // Tag
            var tag = stream.ReadU32(true); // kfmt
            
            Version = stream.ReadU32(true);

            uint count = stream.ReadU32(true);
            this.AttributeTypes = new RenditionAttributeType[count];

            for (int i = 0; i < count; i++)
            {
                this.AttributeTypes[i] = (RenditionAttributeType)stream.ReadU32(true);
            }
        }

        public uint Version { get; set; }

        public RenditionAttributeType[] AttributeTypes { get; set; }

        public override string ToString()
        {
            return
                $"RenditionKeyFormat:\n" +
                $" Version: {Version}\n" +
                $" AttributeTypes: {String.Join(", ", AttributeTypes)}\n";
        }
    }
}
