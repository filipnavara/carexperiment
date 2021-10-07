using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibObjectFile;

namespace AppleTools.AssetCatalog
{
    internal class CarExtendedMetadata
    {
        public CarExtendedMetadata(Stream stream)
        {
            // Tag
            var tag = stream.ReadU32(true); // META

            ThinningArguments = stream.ReadStringUTF8NullTerminated(256);
            DeploymentPlatformVersion = stream.ReadStringUTF8NullTerminated(256);
            DeploymentPlatform = stream.ReadStringUTF8NullTerminated(256);
            AuthoringTool = stream.ReadStringUTF8NullTerminated(256);
        }

        public string ThinningArguments { get; set; }

        public string DeploymentPlatformVersion {  get; set; }

        public string DeploymentPlatform {  get; set; }

        public string AuthoringTool { get; set; }

        public override string ToString()
        {
            return
                $"EXTENDED_METADATA:\n" +
                $" ThinningArguments: {ThinningArguments}\n" +
                $" DeploymentPlatformVersion: {DeploymentPlatformVersion}\n" +
                $" DeploymentPlatform: {DeploymentPlatform}\n" +
                $" AuthoringTool: {AuthoringTool}\n";
        }
    }
}
