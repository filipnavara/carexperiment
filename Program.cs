
using AppleTools;
using AppleTools.AssetCatalog;
using AppleTools.Bom;
using LibObjectFile;

if (BomFile.TryRead(File.OpenRead(/*@"/Applications/Pages.app/Contents/Resources/Assets.car"*/@"/Users/filipnavara/Downloads/gc/MailClient.Mobile.iOS.app/Assets.car"), out var bomFile, out var diagnosticBag))
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

    if (bomFile.TryGetTreeByName("FACETKEYS", out var facetKeysTreeReader))
    {
        Console.WriteLine("FACETKEYS:");
        foreach (var facetKeyValue in facetKeysTreeReader)
        {
            var keyBlock = facetKeyValue.Key;
            var valueBlock = facetKeyValue.Value;
            keyBlock.Stream.Position = 0;
            Console.WriteLine("  " + keyBlock.Stream.ReadUtf8FixedWidthString((uint)keyBlock.Stream.Length));
            valueBlock.Stream.Position = 0;
            var hotSpotX = valueBlock.Stream.ReadU16(true);
            var hotSpotY = valueBlock.Stream.ReadU16(true);
            var numberOfAttributes = valueBlock.Stream.ReadU16(true);
            for (int i = 0; i < numberOfAttributes; i++)
            {
                var name = (RenditionAttributeType)valueBlock.Stream.ReadU16(true);
                var value = valueBlock.Stream.ReadU16(true);
                Console.WriteLine($"    {name}: {value:X}");
            }
        }
        Console.WriteLine();
    }

    if (bomFile.TryGetTreeByName("BITMAPKEYS", out var bitmapKeysTreeReader))
    {
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
