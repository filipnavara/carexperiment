using System.Buffers.Binary;
using System.Collections;
using System.Collections.Generic;

namespace AppleTools.Bom
{
    public class BomTreePage
    {
        Memory<byte> pageMemory;

        public BomTreePage(Memory<byte> pageMemory)
        {
            this.pageMemory = pageMemory;
        }

        public Memory<byte> BackingMemory => pageMemory;

        public bool IsLeaf
        {
            get => BinaryPrimitives.ReadUInt16BigEndian(pageMemory.Span) == 1;
            set => BinaryPrimitives.WriteUInt16BigEndian(pageMemory.Span, (ushort)(value ? 1u : 0u));
        }

        public ushort IndiciesCount
        {
            get => BinaryPrimitives.ReadUInt16BigEndian(pageMemory.Span.Slice(2));
            set => BinaryPrimitives.WriteUInt16BigEndian(pageMemory.Span.Slice(2), value);
        }

        public uint ForwardBlockIndex
        {
            get => BinaryPrimitives.ReadUInt32BigEndian(pageMemory.Span.Slice(4));
            set => BinaryPrimitives.WriteUInt32BigEndian(pageMemory.Span.Slice(4), value);
        }

        public uint BackwardBlockIndex
        {
            get => BinaryPrimitives.ReadUInt32BigEndian(pageMemory.Span.Slice(8));
            set => BinaryPrimitives.WriteUInt32BigEndian(pageMemory.Span.Slice(8), value);
        }

        public IList<uint> ValueIndices => new IndexReaderWriter(pageMemory.Slice(12));

        public IList<uint> KeyIndices => new IndexReaderWriter(pageMemory.Slice(16));

        private struct IndexReaderWriter : IList<uint>
        {
            private readonly Memory<byte> indexMemory;

            public IndexReaderWriter(Memory<byte> indexMemory)
            {
                this.indexMemory = indexMemory;
            }

            public uint this[int index]
            {
                get => BinaryPrimitives.ReadUInt32BigEndian(indexMemory.Span.Slice(8 * index));
                set => BinaryPrimitives.WriteUInt32BigEndian(indexMemory.Span.Slice(8 * index), value);
            }

            public int Count => indexMemory.Length / 8;

            public bool IsReadOnly => false;

            public void Add(uint item) => throw new NotSupportedException();

            public void Clear() => throw new NotSupportedException();

            public bool Contains(uint item) => throw new NotSupportedException();

            public void CopyTo(uint[] array, int arrayIndex) => throw new NotSupportedException();

            public IEnumerator<uint> GetEnumerator() => throw new NotSupportedException();

            public int IndexOf(uint item) => throw new NotSupportedException();

            public void Insert(int index, uint item) => throw new NotSupportedException();

            public bool Remove(uint item) => throw new NotSupportedException();

            public void RemoveAt(int index) => throw new NotSupportedException();

            IEnumerator IEnumerable.GetEnumerator() => throw new NotSupportedException();
        }
    }
}
