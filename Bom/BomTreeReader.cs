using LibObjectFile;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppleTools.Bom
{
    class BomTreeReader : IEnumerable<KeyValuePair<BomBlock, BomBlock>>
    {
        private BomFile bomFile;
        private BomTreePage? currentPage;
        private bool inlineKeys;

        public BomTreeReader(BomFile bomFile, Stream treeStream)
        {
            treeStream.Position = 0;

            // Read header
            var magic = treeStream.ReadU32(true); // tree

            uint version = treeStream.ReadU32(false); // 1
            uint rootIndex = treeStream.ReadU32(false);
            uint blockSize = treeStream.ReadU32(false);
            uint count = treeStream.ReadU32(false);
            byte keyFlags = treeStream.ReadU8();
            uint keySize = treeStream.ReadU32(false);
            uint unknown = treeStream.ReadU32(false);

            this.inlineKeys = keyFlags != 0;

            // The structure is a B+Tree. We locate the first leaf page and then
            // iterate through the forward pointers.
            this.bomFile = bomFile;

            if (rootIndex != 0)
            {
                var rootPage = new BomTreePage(bomFile.Blocks[(int)rootIndex].Stream);
                currentPage = rootPage;
                while (!currentPage.IsLeaf)
                {
                    currentPage = new BomTreePage(bomFile.Blocks[(int)currentPage.ValueIndices[0]].Stream);
                }
            }
        }

        public IEnumerator<KeyValuePair<BomBlock, BomBlock>> GetEnumerator()
        {
            while (currentPage != null)
            {
                for (int i = 0; i < currentPage.KeyIndices.Length; i++)
                {
                    var valueBlock = bomFile.Blocks[(int)currentPage.ValueIndices[i]];
                    BomBlock keyBlock;
                    if (inlineKeys)
                    {
                        var tempStream = new MemoryStream();
                        tempStream.WriteU32(true, currentPage.KeyIndices[i]);
                        tempStream.Position = 0;
                        keyBlock = new BomBlock(tempStream);
                    }
                    else
                    {
                        keyBlock = bomFile.Blocks[(int)currentPage.KeyIndices[i]];
                    }
                    yield return new KeyValuePair<BomBlock, BomBlock>(keyBlock, valueBlock);
                }

                if (currentPage.ForwardBlockIndex != 0)
                {
                    currentPage = new BomTreePage(bomFile.Blocks[(int)currentPage.ForwardBlockIndex].Stream);
                }
                else
                {
                    currentPage = null;
                }
            }

            yield break;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public class BomTreePage
        {
            public BomTreePage(Stream stream)
            {
                stream.Position = 0;

                IsLeaf = stream.ReadU16(false) == 1;

                ushort indiciesCount = stream.ReadU16(false);
                
                ForwardBlockIndex = stream.ReadU32(false);
                BackBlockIndex = stream.ReadU32(false);

                ValueIndices = new uint[indiciesCount];
                KeyIndices = new uint[indiciesCount];

                for (int i = 0; i < indiciesCount; i++)
                {
                    ValueIndices[i] = stream.ReadU32(false);
                    KeyIndices[i] = stream.ReadU32(false);
                }
            }

            public bool IsLeaf { get; private set; }

            public uint ForwardBlockIndex { get; private set; }

            public uint BackBlockIndex { get; private set; }

            public uint[] KeyIndices { get; private set; }

            public uint[] ValueIndices { get; private set; }
        }
    }
}
