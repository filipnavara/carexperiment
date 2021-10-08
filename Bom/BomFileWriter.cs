using LibObjectFile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppleTools.Bom
{
    class BomFileWriter : ObjectFileReaderWriter
    {
        public BomFileWriter(BomFile bomFile, Stream stream)
            : base(stream)
        {
            IsLittleEndian = false;
            BomFile = bomFile;
        }

        public BomFile BomFile { get; private set; }

        public override bool IsReadOnly => false;

        internal void Write()
        {
            uint bomStoreVarsOffset = 0x200; // Header size
            uint bomStoreVarsLength = 4; // Count (u32)

            foreach (var block in BomFile.Blocks)
            {
                // Each block is padded to 16 bytes
                bomStoreVarsOffset += ((uint)block.Stream.Length + 0xfu) & ~0xfu;
            }

            foreach (var namedBlock in BomFile.NamedBlocks)
            {
                bomStoreVarsLength += 5 + (uint)Encoding.UTF8.GetByteCount(namedBlock.Name);
            }

            uint indexEntryCount = ((uint)BomFile.Blocks.Count + 0xffu) & ~0xffu;
            uint bomStoreIndexOffset = (bomStoreVarsOffset + bomStoreVarsLength + 0xfu) & ~0xfu;
            uint bomStoreIndexLength = indexEntryCount * 8 + 4;

            Stream.Write(BomFile.Magic);
            WriteU32(1); // Version
            WriteU32((uint)BomFile.Blocks.Count - 1);
            WriteU32(bomStoreIndexOffset);
            WriteU32(bomStoreIndexLength);
            WriteU32(bomStoreVarsOffset);
            WriteU32(bomStoreVarsLength);
            // Pad the header to 512 bytes
            Stream.Position = 0x200;

            foreach (var block in BomFile.Blocks)
            {
                if (block.Stream.Length > 0)
                    block.Stream.Position = 0;
                block.Stream.CopyTo(Stream);
                // Each block is padded to 16 bytes
                Stream.Position = (Stream.Position + 0xfu) & ~0xfu;
            }

            WriteU32((uint)BomFile.NamedBlocks.Count);
            foreach (var namedBlock in BomFile.NamedBlocks)
            {
                WriteU32(namedBlock.BlockIndex);
                var nameBytes = Encoding.UTF8.GetBytes(namedBlock.Name);
                WriteU8((byte)nameBytes.Length);
                Stream.Write(nameBytes);
            }

            // Align again to 16-byte boundary
            Stream.Position = (Stream.Position + 0xfu) & ~0xfu;

            WriteU32(indexEntryCount);
            WriteU32(0);
            WriteU32(0);
            indexEntryCount--;
            uint blockOffset = 0x200;
            foreach (var block in BomFile.Blocks.Skip(1))
            {
                WriteU32(blockOffset);
                WriteU32((uint)block.Stream.Length);
                blockOffset += ((uint)block.Stream.Length + 0xfu) & ~0xfu;
                indexEntryCount--;
            }
            while (indexEntryCount-- > 0)
            {
                WriteU32(0);
                WriteU32(0);
            }
        }
    }
}
