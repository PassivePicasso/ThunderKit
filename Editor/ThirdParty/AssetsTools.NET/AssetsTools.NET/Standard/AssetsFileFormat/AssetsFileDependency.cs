using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetsTools.NET
{
    public class AssetsFileDependency
    {
        public string bufferedPath;
        public Guid guid;
        public int type;
        public string assetPath;
        public string originalAssetPath;
        public void Read(AssetsFileReader reader)
        {
            bufferedPath = reader.ReadNullTerminated();
            guid = new Hash128(reader).ToGuid();
            type = reader.ReadInt32();
            assetPath = reader.ReadNullTerminated();
            originalAssetPath = assetPath;

            //because lowercase "resources" is read by unity fine on linux, it either uses
            //hardcoded replaces like below or it has case insensitive pathing somehow
            //this isn't consistent with the original assetstools but it only supported
            //windows anyway, so this will only create issues if more than these three
            //pop up in the future. also, the reason I don't just replace all "library"
            //with "Resources" is so that when saving, I can change it back to the original
            //(like how unity_builtin_extra goes back to "resources", not "library")
            if (assetPath == "resources/unity_builtin_extra")
            {
                assetPath = "Resources/unity_builtin_extra";
            }
            else if (assetPath == "library/unity default resources" || assetPath == "Library/unity default resources")
            {
                assetPath = "Resources/unity default resources";
            }
            else if (assetPath == "library/unity editor resources" || assetPath == "Library/unity editor resources")
            {
                assetPath = "Resources/unity editor resources";
            }
        }

        public void Write(AssetsFileWriter writer)
        {
            writer.WriteNullTerminated(bufferedPath);
            writer.Write(guid.ToByteArray());
            writer.Write(type);
            string assetPathTemp = assetPath;
            if ((assetPath == "Resources/unity_builtin_extra" ||
                assetPath == "Resources/unity default resources" ||
                assetPath == "Resources/unity editor resources")
                && originalAssetPath != string.Empty)
            {
                assetPathTemp = originalAssetPath;
            }
            writer.WriteNullTerminated(assetPathTemp);
        }
    }
}
