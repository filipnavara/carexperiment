using System.Buffers.Binary;

namespace AppleTools.Bom
{
    public class BomTreeHeader
    {
        Memory<byte> headerMemory;

        public BomTreeHeader(Memory<byte> headerMemory)
        {
            this.headerMemory = headerMemory;
        }

        public static BomTreeHeader Create(bool inlineKeys = false, uint keySize = 0, uint pageSize = 0x1000)
        {
            var headerBuffer = new byte[29];
            BinaryPrimitives.WriteUInt32BigEndian(headerBuffer, 0x74726565); // Identifier: 'tree'
            BinaryPrimitives.WriteUInt32BigEndian(headerBuffer.AsSpan(4), 1); // Version: 1
            var treeHeader = new BomTreeHeader(headerBuffer);
            treeHeader.InlineKeys = inlineKeys;
            treeHeader.KeySize = keySize;
            treeHeader.PageSize = pageSize;
            return treeHeader;
        }

        public Memory<byte> BackingMemory => headerMemory;

        public uint RootBlockIndex
        {
            get => BinaryPrimitives.ReadUInt32BigEndian(headerMemory.Span.Slice(8));
            set => BinaryPrimitives.WriteUInt32BigEndian(headerMemory.Span.Slice(8), value);
        }

        public uint PageSize
        {
            get => BinaryPrimitives.ReadUInt32BigEndian(headerMemory.Span.Slice(12));
            set => BinaryPrimitives.WriteUInt32BigEndian(headerMemory.Span.Slice(12), value);
        }

        public uint Count
        {
            get => BinaryPrimitives.ReadUInt32BigEndian(headerMemory.Span.Slice(16));
            set => BinaryPrimitives.WriteUInt32BigEndian(headerMemory.Span.Slice(16), value);
        }

        public bool InlineKeys
        {
            get => headerMemory.Span[20] == 1;
            set => headerMemory.Span[20] = value ? (byte)1 : (byte)0;
        }

        public uint KeySize
        {
            get => BinaryPrimitives.ReadUInt32BigEndian(headerMemory.Span.Slice(21));
            set => BinaryPrimitives.WriteUInt32BigEndian(headerMemory.Span.Slice(21), value);
        }
    }
}
