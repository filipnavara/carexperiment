using LibObjectFile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppleTools.AssetCatalog
{
    class CsiHeader
    {
        public CsiHeader(Stream stream)
        {
            // Tag
            var tag = stream.ReadU32(true); // CTSI: Core Theme Structured Image

            Version = stream.ReadU32(true);
            Flags = (RenditionFlags)stream.ReadU32(true);
            Width = stream.ReadU32(true);
            Height = stream.ReadU32(true);
            ScaleFactor = stream.ReadU32(true);
            PixelFormat = stream.ReadU32(true);
            ColorSpace = stream.ReadU32(true);
            ModificationTime = DateTimeOffset.FromUnixTimeSeconds(stream.ReadU32(true));
            Layout = (RenditionLayoutType)stream.ReadU32(true);
            Name = stream.ReadUtf8FixedWidthString(128);
        }

        public uint Version { get; set; }

        public RenditionFlags Flags { get; set; }

        public uint Width { get; set; }

        public uint Height { get; set; }

        public uint ScaleFactor { get; set; }

        public uint PixelFormat { get; set; }

        public uint ColorSpace { get; set; }

        public DateTimeOffset ModificationTime { get; set; }

        public RenditionLayoutType Layout {  get; set; }

        public string Name { get; set; }

        public override string ToString()
        {
            return
                $"CoreTheme Structured Image:\n" +
                $"  Version: {Version}\n" +
                $"  Flags: {Flags}\n" +
                $"  Width: {Width}\n" +
                $"  Height: {Height}\n" +
                $"  ScaleFactor: {ScaleFactor}\n" +
                $"  PixelFormat: {PixelFormat}\n" +
                $"  ColorSpace: {ColorSpace}\n" +
                $"  ModificationTime: {ModificationTime}\n" +
                $"  Layout: {Layout}\n" +
                $"  Name: {Name}\n";
        }
    }
}
