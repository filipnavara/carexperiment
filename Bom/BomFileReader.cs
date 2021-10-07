using LibObjectFile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppleTools.Bom
{
    class BomFileReader : ObjectFileReaderWriter
    {
        public BomFileReader(BomFile bomFile, Stream stream)
            : base(stream)
        {
            IsLittleEndian = false;
            BomFile = bomFile;
        }

        public BomFile BomFile { get; private set; }

        public override bool IsReadOnly => true;

        internal static bool IsBom(Stream stream, DiagnosticBag diagnostics)
        {
            Span<byte> magic = stackalloc byte[BomFile.Magic.Length];
            int magicLength = stream.Read(magic);
            if (magicLength != magic.Length)
            {
                if (diagnostics != null)
                {
                    diagnostics.Error((DiagnosticId)BomDiagnosticId.BOM_ERR_InvalidMagicLength, $"Invalid length {magicLength} while trying to read !<arch> from stream while expecting at least {magic.Length} bytes");
                }
                return false;
            }

            if (!magic.SequenceEqual(BomFile.Magic))
            {
                if (diagnostics != null)
                {
                    diagnostics.Error((DiagnosticId)BomDiagnosticId.BOM_ERR_MagicNotFound, $"Magic !<arch>\\n not found");
                }
                return false;
            }

            return true;
        }

        internal void Read()
        {
            if (!IsBom(Stream, Diagnostics))
            {
                return;
            }

            var bomStoreVersion = ReadU32();
            if (bomStoreVersion != 1)
            {
                Diagnostics.Error((DiagnosticId)BomDiagnosticId.BOM_ERR_UnknownVersion, $"Unknown BOM file version: {bomStoreVersion}");
                return;
            }

            var bomStoreNumberOfBlocks = ReadU32();
            var bomStoreIndexOffset = ReadU32();
            var bomStoreIndexLength = ReadU32();
            var bomStoreVarsOffset = ReadU32();
            var bomStoreVarsLength = ReadU32();

            // Read block index
            this.Offset = bomStoreIndexOffset;
            var bomIndexCount = ReadU32();
            var bomIndex = new (uint Offset, uint Length)[bomStoreNumberOfBlocks + 1];
            for (int i = 0; i < bomIndexCount && i <= bomStoreNumberOfBlocks; i++)
            {
                bomIndex[i] = (ReadU32(), ReadU32());
            }

            foreach (var bomIndexEntry in bomIndex)
            {
                BomFile.AddBlock(new BomBlock(this, bomIndexEntry.Offset, bomIndexEntry.Length));
            }

            // Read named blocks
            this.Offset = bomStoreVarsOffset;
            var bomVarsCounts = ReadU32();
            for (int i = 0; i < bomVarsCounts; i++)
            {
                uint bomVarIndex = ReadU32();
                byte bomVarNameLength = ReadU8();
                string bomVarName = ReadStringUTF8NullTerminated(bomVarNameLength);
                BomFile.AddNamedBlock(new BomNamedBlock(bomVarName, bomVarIndex));
            }
        }
    }
}
