using AssetsTools.NET;
using System;
using System.Collections.Generic;
using System.Text;

namespace AssetsExporter.Extensions
{
    public static class AssetTypeValueFieldExtensions
    {
        public static AssetTypeValueField GetLastChild(this AssetTypeValueField atvf)
        {
            return atvf[atvf.Children.Count - 1];
        }
    }
}
