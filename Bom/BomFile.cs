using LibObjectFile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Diagnostics.CodeAnalysis;

namespace AppleTools.Bom
{
    internal class BomFile
    {
        private readonly List<BomBlock> _blocks;
        private readonly List<BomNamedBlock> _namedBlocks;

        public static ReadOnlySpan<byte> Magic => new ReadOnlySpan<byte>(new byte[]
        {
            (byte)'B',
            (byte)'O',
            (byte)'M',
            (byte)'S',
            (byte)'t',
            (byte)'o',
            (byte)'r',
            (byte)'e',
        });

        public BomFile()
        {
            _blocks = new List<BomBlock>();
            _namedBlocks = new List<BomNamedBlock>();
        }

        public IReadOnlyList<BomBlock> Blocks => _blocks;
        public IReadOnlyList<BomNamedBlock> NamedBlocks => _namedBlocks;

        public void AddBlock(BomBlock block)
        {
            _blocks.Add(block);
        }

        public void AddNamedBlock(BomNamedBlock namedBlock)
        {
            _namedBlocks.Add(namedBlock);
        }

        public bool TryGetBlockByName(string name, [NotNullWhen(true)] out BomBlock? bomBlock)
        {
            foreach (var namedBlock in _namedBlocks)
            {
                if (namedBlock.Name == name)
                {
                    bomBlock = _blocks[(int)namedBlock.BlockIndex];
                    return true;
                }
            }

            bomBlock = null;
            return false;
        }

        public bool TryGetTreeByName(string name, [NotNullWhen(true)] out BomTreeReader? bomTreeReader)
        {
            foreach (var namedBlock in _namedBlocks)
            {
                if (namedBlock.Name == name)
                {
                    bomTreeReader = new BomTreeReader(this, _blocks[(int)namedBlock.BlockIndex].Stream);
                    return true;
                }
            }

            bomTreeReader = null;
            return false;
        }

        public static bool TryRead(Stream stream, [NotNullWhen(true)] out BomFile? bomFile, out DiagnosticBag diagnostics)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            bomFile = new BomFile();
            var reader = new BomFileReader(bomFile, stream);
            diagnostics = reader.Diagnostics;
            reader.Read();

            return !reader.Diagnostics.HasErrors;
        }
    }
}
