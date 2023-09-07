using AppleTools.AssetCatalog;
using LibObjectFile;
using System;
using System.Buffers.Binary;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppleTools.Bom
{
    class BomTreeBuilder
    {
        private BomFile bomFile;
        private BomTreeHeader header;
        private BomTreePage rootPage;
        //private BomBlock rootPageBlock;
        private Comparison<BomBlock> keyComparer;
        private SortedDictionary<BomBlock, BomBlock> blockDictionary;

        public BomTreeBuilder(BomFile bomFile, Comparison<BomBlock> keyComparer, uint pageSize = 4096)
        {
            this.bomFile = bomFile;
            this.keyComparer = keyComparer;
            this.header = BomTreeHeader.Create(pageSize: pageSize);
            this.blockDictionary = new SortedDictionary<BomBlock, BomBlock>(Comparer<BomBlock>.Create(keyComparer));
        }

        private int MaxIndicesPerPage => (int)(header.PageSize - 12) / 8;

        public void Add(BomBlock key, BomBlock value)
        {
            if (this.header.KeySize == 0)
                this.header.KeySize = (uint)key.Stream.Length;
            else if (this.header.KeySize != uint.MaxValue && this.header.KeySize != (uint)key.Stream.Length)
                this.header.KeySize = uint.MaxValue;
            this.header.InlineKeys = this.header.KeySize == 4;
            this.blockDictionary.Add(key, value);
        }

        public uint Build()
        {
            var currentPages = new List<(uint Index, BomTreePage Page)>();
            BomTreePage? currentPage = null;
            Span<byte> tmp = stackalloc byte[4];

            // Build leaf pages
            int remainingCount = blockDictionary.Count;
            int keysInPage = 0;
            foreach (var keyValuePair in blockDictionary)
            {
                if (currentPage == null || currentPage.IndiciesCount == MaxIndicesPerPage)
                {
                    // Add new leaf page
                    keysInPage = remainingCount < MaxIndicesPerPage ? remainingCount : MaxIndicesPerPage;
                    int keysSize = !this.header.InlineKeys && this.header.KeySize != uint.MaxValue ? keysInPage * (int)this.header.KeySize : 0;
                    var newPageBuffer = new byte[(int)this.header.PageSize + keysSize];
                    var newPageBlock = new BomBlock(new MemoryStream(newPageBuffer));
                    var newPage = new BomTreePage(newPageBuffer);
                    var newPageIndex = bomFile.AddBlock(newPageBlock);
                    newPage.IsLeaf = true;
                    newPage.BackwardBlockIndex = currentPages.Count > 0 ? currentPages[^1].Index : 0;
                    newPage.ForwardBlockIndex = 0;
                    newPage.IndiciesCount = 0;
                    if (currentPage != null)
                        currentPage.ForwardBlockIndex = newPageIndex;
                    currentPage = newPage;
                    currentPages.Add((newPageIndex, newPage));
                }

                ushort i = currentPage.IndiciesCount;
                uint key;
                if (header.InlineKeys)
                {
                    keyValuePair.Key.ReadInto(tmp);
                    key = BinaryPrimitives.ReadUInt32LittleEndian(tmp);
                }
                else
                {
                    key = bomFile.AddBlock(keyValuePair.Key);
                    // Fixed length keys have a second copy in the B+Tree itself
                    if (this.header.KeySize != uint.MaxValue)
                    {
                        keyValuePair.Key.ReadInto(currentPage.BackingMemory.Slice(
                            (int)(16 + (8 * keysInPage) + (i * header.KeySize)),
                            (int)header.KeySize).Span);
                    }
                }
                currentPage.KeyIndices[i] = key;
                currentPage.ValueIndices[i] = bomFile.AddBlock(keyValuePair.Value);
                currentPage.IndiciesCount = (ushort)(i + 1);
                --remainingCount;
            }

            while (currentPages.Count > 1)
            {
                // Create extra B+Tree layer
                var lastPages = currentPages;
                currentPages = new List<(uint Index, BomTreePage Page)>();
                currentPage = null;

                remainingCount = lastPages.Count;
                foreach (var page in lastPages)
                {
                    if (currentPage == null || currentPage.IndiciesCount == MaxIndicesPerPage)
                    {
                        // Add new interior page
                        keysInPage = remainingCount < MaxIndicesPerPage ? remainingCount : MaxIndicesPerPage;
                        int keysSize = !this.header.InlineKeys && this.header.KeySize != uint.MaxValue ? keysInPage * (int)this.header.KeySize : 0;
                        var newPageBuffer = new byte[(int)this.header.PageSize];
                        var newPageBlock = new BomBlock(new MemoryStream(newPageBuffer));
                        var newPage = new BomTreePage(newPageBuffer);
                        var newPageIndex = bomFile.AddBlock(newPageBlock);
                        newPage.IsLeaf = false;
                        newPage.BackwardBlockIndex = currentPages.Count > 0 ? currentPages[^1].Index : 0;
                        newPage.ForwardBlockIndex = 0;
                        newPage.IndiciesCount = 0;
                        if (currentPage != null)
                            currentPage.ForwardBlockIndex = newPageIndex;
                        currentPage = newPage;
                        currentPages.Add((newPageIndex, newPage));
                    }

                    ushort i = currentPage.IndiciesCount;
                    var key = page.Page.KeyIndices[page.Page.IndiciesCount - 1];
                    // Fixed length keys have a second copy in the B+Tree itself
                    if (!this.header.InlineKeys && this.header.KeySize != uint.MaxValue)
                    {
                        bomFile.Blocks[(int)key].ReadInto(currentPage.BackingMemory.Slice(
                            (int)(16 + (8 * keysInPage) + (i * header.KeySize)),
                            (int)header.KeySize).Span);
                    }
                    currentPage.KeyIndices[i] = key;
                    currentPage.ValueIndices[i] = page.Index;
                    currentPage.IndiciesCount = (ushort)(i + 1);
                    --remainingCount;
                }
            }

            if (currentPages.Count > 0)
            {
                header.RootBlockIndex = currentPages[0].Index;
            }
            else
            {
                var newPageBuffer = new byte[(int)this.header.PageSize];
                var newPageBlock = new BomBlock(new MemoryStream(newPageBuffer));
                var newPage = new BomTreePage(newPageBuffer);
                header.RootBlockIndex = bomFile.AddBlock(newPageBlock);
                newPage.IsLeaf = true;
            }

            if (this.header.InlineKeys)
                this.header.KeySize = 0;

            header.Count = (uint)blockDictionary.Count;
            return this.bomFile.AddBlock(new BomBlock(new MemoryStream(this.header.BackingMemory.ToArray())));
        }

        public static int DefaultKeyComparer(BomBlock left, BomBlock right)
        {
            Span<byte> leftBuffer = stackalloc byte[(int)left.Stream.Length];
            left.ReadInto(leftBuffer);
            Span<byte> rightBuffer = stackalloc byte[(int)right.Stream.Length];
            right.ReadInto(rightBuffer);
            return leftBuffer.SequenceCompareTo(rightBuffer);
        }

        /*private BomTreePage Insert(BomBlock key, BomBlock value)
        {
            if (this.header.KeySize == 0)
            {
                this.header.KeySize = (uint)key.Stream.Length;
                this.header.InlineKeys = this.header.KeySize <= 4;
            }
            else
            {
                Debug.Assert(this.header.KeySize == (uint)key.Stream.Length);
            }

            uint valueBlockIndex = this.bomFile.AddBlock(value);
            byte[] keyBuffer = new byte[this.header.KeySize];
            // set this.header.KeySize

            BomTreePage currentPage = this.rootPage;
            if (currentPage.IsLeaf)
            {
                int i = Search(currentPage, key);
                if (i < MaxIndicesPerPage)
                {
                    var backingSpan = currentPage.BackingMemory.Span;
                    // Move existing keys/values
                    backingSpan.Slice(12 + (i * 8), (currentPage.IndiciesCount - i) * 8).
                        CopyTo(backingSpan.Slice(20 + (i * 8), (currentPage.IndiciesCount - i) * 8));
                    // Add new key/value
                    BinaryPrimitives.WriteUInt32BigEndian(backingSpan.Slice(12 + (i * 8)), valueBlockIndex);
                    if (header.InlineKeys)
                    {
                        // TODO
                    }
                    else
                    {
                        uint keyBlockIndex = this.bomFile.AddBlock(key);
                        BinaryPrimitives.WriteUInt32BigEndian(backingSpan.Slice(16 + (i * 8)), keyBlockIndex);
                    }
                }
            }
        }

        private int Search(BomTreePage currentPage, BomBlock key)
        {
            BomBlock keyBlock = null!;
            MemoryStream tempStream = null!;

            if (header.InlineKeys)
            {
                tempStream = new MemoryStream(4);
                keyBlock = new BomBlock(tempStream);
            }
            
            for (int i = 0; i < currentPage.IndiciesCount; i++)
            {
                if (header.InlineKeys)
                {
                    tempStream.WriteU32(true, currentPage.KeyIndices[i]);
                    tempStream.Position = 0;
                }
                else
                {
                    keyBlock = bomFile.Blocks[(int)currentPage.KeyIndices[i]];
                }

                if (keyComparer(keyBlock, key) >= 0)
                {
                    return i;
                }
            }
        }*/
    }
}
