
using AppleTools.AssetCatalog;
using AppleTools.Bom;
using LibObjectFile;

if (BomFile.TryRead(File.OpenRead(@"/Applications/Pages.app/Contents/Resources/Assets.car"), out var bomFile, out var diagnosticBag))
{
    RenditionKeyFormat renditionKeyFormat;

    if (bomFile.TryGetBlockByName("CARHEADER", out var carHeaderBlock))
    {
        var carHeader = new CarHeader(carHeaderBlock.Stream);
        Console.WriteLine(carHeader.ToString());
    }

    if (bomFile.TryGetBlockByName("EXTENDED_METADATA", out var extendedMetadataBlock))
    {
        var extendedMetadata = new CarExtendedMetadata(extendedMetadataBlock.Stream);
        Console.WriteLine(extendedMetadata.ToString());
    }

    if (bomFile.TryGetBlockByName("KEYFORMAT", out var keyFormatBlock))
    {
        renditionKeyFormat = new RenditionKeyFormat(keyFormatBlock.Stream);
        Console.WriteLine(renditionKeyFormat.ToString());

        if (bomFile.TryGetTreeByName("RENDITIONS", out var renditionsTreeReader))
        {
            Console.WriteLine("RENDITIONS:");
            foreach (var renditionKeyValue in renditionsTreeReader)
            {
                var keyStream = renditionKeyValue.Key.Stream;
                Console.WriteLine("  Key: ");
                for (int i = 0; i < renditionKeyFormat.AttributeTypes.Length; i++)
                {
                    ushort key = keyStream.ReadU16(true);
                    Console.WriteLine($"    {renditionKeyFormat.AttributeTypes[i]}: {key:X}");
                }
                Console.WriteLine("  Value: ");
                var csiHeader = new CsiHeader(renditionKeyValue.Value.Stream);
                Console.Write(String.Join("\n", csiHeader.ToString().Split('\n').Select(l => "    " + l)));
                Console.WriteLine();
            }
        }
    }
}
