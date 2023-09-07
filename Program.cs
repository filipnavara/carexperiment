
using AppleTools;
using AppleTools.AssetCatalog;
using AppleTools.Bom;
using LibObjectFile;

if (BomFile.TryRead(File.OpenRead(@"/Applications/Pages.app/Contents/Resources/Assets.car"/*@"/Users/filipnavara/Downloads/gc/MailClient.Mobile.iOS.app/Assets.car"*/), out var bomFile, out var diagnosticBag))
{
    using (var outFile = File.OpenWrite("tst.bom"))
    {
        var bomCopy = new BomFile();
        if (bomFile.TryGetBlockByName("CARHEADER", out var carHeaderBlock1))
            bomCopy.AddNamedBlock("CARHEADER", carHeaderBlock1);
        if (bomFile.TryGetTreeByName("RENDITIONS", out var renditionsTreeReader1))
        {
            var builder = new BomTreeBuilder(bomCopy, BomTreeBuilder.DefaultKeyComparer);
            foreach (var kv in renditionsTreeReader1)
                builder.Add(kv.Key, kv.Value);
            bomCopy.AddNamedBlock(new BomNamedBlock("RENDITIONS", builder.Build()));
        }
        if (bomFile.TryGetTreeByName("FACETKEYS", out var facetKeysTreeReader1))
        {
            var builder = new BomTreeBuilder(bomCopy, BomTreeBuilder.DefaultKeyComparer);
            foreach (var kv in facetKeysTreeReader1)
                builder.Add(kv.Key, kv.Value);
            bomCopy.AddNamedBlock(new BomNamedBlock("FACETKEYS", builder.Build()));
        }
        if (bomFile.TryGetTreeByName("APPEARANCEKEYS", out var appearanceKeysTreeReader1))
        {
            var builder = new BomTreeBuilder(bomCopy, BomTreeBuilder.DefaultKeyComparer);
            foreach (var kv in appearanceKeysTreeReader1)
                builder.Add(kv.Key, kv.Value);
            bomCopy.AddNamedBlock(new BomNamedBlock("APPEARANCEKEYS", builder.Build()));
        }
        if (bomFile.TryGetTreeByName("LOCALIZATIONKEYS", out var localizationKeysTreeReader1))
        {
            var builder = new BomTreeBuilder(bomCopy, BomTreeBuilder.DefaultKeyComparer);
            foreach (var kv in localizationKeysTreeReader1)
                builder.Add(kv.Key, kv.Value);
            bomCopy.AddNamedBlock(new BomNamedBlock("LOCALIZATIONKEYS", builder.Build()));
        }
        if (bomFile.TryGetBlockByName("KEYFORMAT", out var keyFormat1))
            bomCopy.AddNamedBlock("KEYFORMAT", keyFormat1);
        if (bomFile.TryGetBlockByName("EXTENDED_METADATA", out var extendedMetadataBlock1))
            bomCopy.AddNamedBlock("EXTENDED_METADATA", extendedMetadataBlock1);
        if (bomFile.TryGetTreeByName("BITMAPKEYS", out var bitmapKeysTreeReader1))
        {
            var builder = new BomTreeBuilder(bomCopy, BomTreeBuilder.DefaultKeyComparer, 1024);
            foreach (var kv in bitmapKeysTreeReader1)
                builder.Add(kv.Key, kv.Value);
            bomCopy.AddNamedBlock(new BomNamedBlock("BITMAPKEYS", builder.Build()));
        }
        bomCopy.Write(outFile);

        //bomFile = bomCopy;
        //bomFile.Write(outFile);
    }

    BomFile.TryRead(File.OpenRead(@"tst.bom"), out bomFile, out diagnosticBag);

    RenditionKeyFormat renditionKeyFormat;

    if (bomFile.TryGetBlockByName("CARHEADER", out var carHeaderBlock))
    {
        carHeaderBlock.Stream.Position = 0;
        var carHeader = new CarHeader(carHeaderBlock.Stream);
        Console.WriteLine(carHeader.ToString());
    }

    if (bomFile.TryGetBlockByName("EXTENDED_METADATA", out var extendedMetadataBlock))
    {
        extendedMetadataBlock.Stream.Position = 0;
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

    if (bomFile.TryGetBlockByName("KEYFORMAT", out var keyFormatBlock))
    {
        keyFormatBlock.Stream.Position = 0;
        renditionKeyFormat = new RenditionKeyFormat(keyFormatBlock.Stream);
        Console.WriteLine(renditionKeyFormat.ToString());

        if (bomFile.TryGetTreeByName("RENDITIONS", out var renditionsTreeReader))
        {
            Console.WriteLine("RENDITIONS:");
            foreach (var renditionKeyValue in renditionsTreeReader)
            {
                var keyStream = renditionKeyValue.Key.Stream;
                keyStream.Position = 0;
                Console.WriteLine("  Key: ");
                for (int i = 0; i < renditionKeyFormat.AttributeTypes.Length; i++)
                {
                    ushort key = keyStream.ReadU16(true);
                    Console.WriteLine($"    {renditionKeyFormat.AttributeTypes[i]}: {key:X}");
                }
                Console.WriteLine("  Value: ");
                renditionKeyValue.Value.Stream.Position = 0;
                var csiHeader = new CsiHeader(renditionKeyValue.Value.Stream);
                Console.Write(String.Join("\n", csiHeader.ToString().Split('\n').Select(l => "    " + l)));
                Console.WriteLine();
            }
        }

        // Bitmap keys map a name identifier into a set of possible values for the rendition keys
        if (bomFile.TryGetTreeByName("BITMAPKEYS", out var bitmapKeysTreeReader))
        {
            Console.WriteLine("BITMAPKEYS:");
            foreach (var bitmapKeyValue in bitmapKeysTreeReader)
            {
                var keyStream = bitmapKeyValue.Key.Stream;
                keyStream.Position = 0;
                Console.WriteLine($"  Name Identifier: {keyStream.ReadU32(true):X}");
                Console.WriteLine("  Value: ");
                var valueStream = bitmapKeyValue.Value.Stream;
                valueStream.Position = 0;
                uint version = valueStream.ReadU32(true); // 1
                uint unknown = valueStream.ReadU32(true); // 0?
                uint bitmapSize = valueStream.ReadU32(true);
                uint elementCount = valueStream.ReadU32(true); // elementCount * 4 + 4 == bitmapSize
                for (int i = 0; i < renditionKeyFormat.AttributeTypes.Length; i++)
                {
                    uint valueMask = valueStream.ReadU32(true);
                    Console.Write($"    {renditionKeyFormat.AttributeTypes[i]}: ");
                    if (valueMask == 0xffffffffu)
                    {
                        Console.WriteLine("<all>");
                    }
                    else
                    {
                        Console.WriteLine(String.Join(", ", Enumerable.Range(0, 32).Where(b => (valueMask & (uint)(1 << b)) != 0)));
                    }
                }
                Console.WriteLine();
            }
        }
    }
}
