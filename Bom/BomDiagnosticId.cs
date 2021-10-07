using LibObjectFile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppleTools.Bom
{
    enum BomDiagnosticId
    {
        BOM_ERR_InvalidMagicLength = 3000,
        BOM_ERR_MagicNotFound = 3001,
        BOM_ERR_UnknownVersion = 3002,
    }
}
