using UnityEngine;
using System.IO;
namespace Krod
{
    public static class Assets
    {
        public static AssetBundle bundle;

        public static void Init()
        {
            bundle = AssetBundle.LoadFromFile(
                 Path.Combine(Path.GetDirectoryName(MainPlugin.PInfo.Location), "krod.assetbundle")
            );
        }
    }
}
