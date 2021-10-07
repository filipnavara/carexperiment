using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppleTools.AssetCatalog
{
    enum RenditionLayoutType : uint
    {
        TextEffect = 0x007,
        Vector = 0x009,
        Data = 0x3E8,
        ExternalLink = 0x3E9,
        LayerStack = 0x3EA,
        InternalReference = 0x3EB,
        PackedImage = 0x3EC,
        NameList = 0x3ED,
        UnknownAddObject = 0x3EE,
        Texture = 0x3EF,
        TextureImage = 0x3F0,
        Color = 0x3F1,
        MultisizeImage = 0x3F2,
        LayerReference = 0x3F4,
        ContentRendition = 0x3F5,
        RecognitionObject = 0x3F6,
    }
}
