using System.IO;

namespace PassivePicasso.ThunderKit.Thunderstore
{
    internal static class Constants
    {
        public const string ROS_Temp = nameof(ROS_Temp);

        public const string ThunderStorePath = Core.Constants.ThunderKitContextRoot + "Thunderstore/";

        public static readonly string TempDir = Path.Combine(Directory.GetCurrentDirectory(), ROS_Temp);
        public static readonly string Packages = Path.Combine("Packages");
        public static readonly string dependenciesPath = Path.Combine(Directory.GetCurrentDirectory(), "Packages");

    }
}