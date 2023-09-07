using LibObjectFile;
using System;
using System.Buffers.Binary;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppleTools.Bom
{
    class BomTreeReader : IEnumerable<KeyValuePair<BomBlock, BomBlock>>
    {
        private readonly BomFile bomFile;
        private readonly byte[] pageBuffer;
        private readonly bool inlineKeys;
        private readonly uint keySize;
        private BomTreePage? currentPage;

        public BomTreeReader(BomFile bomFile, Stream treeStream)
        {
            treeStream.Position = 0;
            var headerBuffer = new byte[/*treeStream.Length*/ 29];
            treeStream.ReadExactly(headerBuffer);

            var header = new BomTreeHeader(headerBuffer);
            this.inlineKeys = header.InlineKeys;
            this.keySize = header.KeySize;
            this.pageBuffer = new byte[header.PageSize];


            // Read header
            /*var magic = treeStream.ReadU32(true); // tree

            uint version = treeStream.ReadU32(false); // 1
            uint rootIndex = treeStream.ReadU32(false);
            uint blockSize = treeStream.ReadU32(false);
            uint count = treeStream.ReadU32(false);
            byte keyFlags = treeStream.ReadU8();
            uint keySize = treeStream.ReadU32(false);
            uint unknown = treeStream.ReadU32(false);

            this.inlineKeys = keyFlags != 0;*/

            // The structure is a B+Tree. We locate the first leaf page and then
            // iterate through the forward pointers.
            this.bomFile = bomFile;

            if (header.RootBlockIndex != 0)
            {
                bomFile.Blocks[(int)header.RootBlockIndex].ReadInto(pageBuffer);
                currentPage = new BomTreePage(pageBuffer);
                while (!currentPage.IsLeaf)
                {
                    bomFile.Blocks[(int)currentPage.ValueIndices[0]].ReadInto(pageBuffer);
                    currentPage = new BomTreePage(pageBuffer);
                }
            }
        }

        public IEnumerator<KeyValuePair<BomBlock, BomBlock>> GetEnumerator()
        {
            while (currentPage != null)
            {
                for (int i = 0; i < currentPage.IndiciesCount; i++)
                {
                    var valueBlock = bomFile.Blocks[(int)currentPage.ValueIndices[i]];
                    BomBlock keyBlock;
                    if (inlineKeys)
                    {
                        var tmp = new byte[4];
                        BinaryPrimitives.WriteUInt32LittleEndian(tmp, currentPage.KeyIndices[i]);
                        keyBlock = new BomBlock(new MemoryStream(tmp));
                    }
                    else
                    {
                        keyBlock = bomFile.Blocks[(int)currentPage.KeyIndices[i]];
                    }
                    yield return new KeyValuePair<BomBlock, BomBlock>(keyBlock, valueBlock);
                }

                if (currentPage.ForwardBlockIndex != 0)
                {
                    bomFile.Blocks[(int)currentPage.ForwardBlockIndex].ReadInto(pageBuffer);
                    currentPage = new BomTreePage(pageBuffer);
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
    }
}
