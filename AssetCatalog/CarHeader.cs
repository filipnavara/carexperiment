using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibObjectFile;

namespace AppleTools.AssetCatalog
{
    internal class CarHeader
    {
        public CarHeader(Stream stream)
        {
            // Tag
            var tag = stream.ReadU32(true); // RATC

            CoreUIVersion = stream.ReadU32(true);
            StorageVersion = stream.ReadU32(true);
            StorageTimestamp = DateTimeOffset.FromUnixTimeSeconds(stream.ReadU32(true));
            RenditionCount = stream.ReadU32(true);
            MainVersion = stream.ReadStringUTF8NullTerminated(128);
            Version = stream.ReadStringUTF8NullTerminated(256);
            Span<byte> uuidBuffer = stackalloc byte[16];
            stream.Read(uuidBuffer); // FIXME: assert read size
            Uuid = new Guid(uuidBuffer);
            AssociatedChecksum = stream.ReadU32(true);
            SchemaVersion = stream.ReadU32(true);
            ColorSpace = stream.ReadU32(true);
            KeySemantics = stream.ReadU32(true);
        }

        public uint CoreUIVersion { get; set; }

        public uint StorageVersion {  get; set; }

        public DateTimeOffset StorageTimestamp { get; set; }

        public uint RenditionCount { get; set; }

        public string MainVersion { get; set; }

        public string Version { get; set; }

        public Guid Uuid { get; set; }

        public uint AssociatedChecksum { get; set; }

        public uint SchemaVersion { get; set; }

        public uint ColorSpace { get; set; }

        public uint KeySemantics { get; set; }

        public override string ToString()
        {
            return
                $"CARHEADER:\n" +
                $" CoreUIVersion: {CoreUIVersion}\n" +
                $" StorageVersion: {StorageVersion}\n" +
                $" StorageTimestamp: {StorageTimestamp}\n" +
                $" RenditionCount: {RenditionCount}\n" +
                $" MainVersion: {MainVersion}\n" +
                $" Version: {Version}\n" +
                $" Uuid: {Uuid}\n" +
                $" AssociatedChecksum: {AssociatedChecksum}\n" +
                $" SchemaVersion: {SchemaVersion}\n" +
                $" ColorSpace: {ColorSpace}\n" +
                $" KeySemantics: {KeySemantics}\n";
        }
    }
}
