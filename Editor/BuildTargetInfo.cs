using UnityEditor;

namespace Shiori.VRChat
{
    /// <summary>Names build targets the way VRChat creators talk about them: PC, Android, iOS.</summary>
    internal static class BuildTargetInfo
    {
        public static string Label(BuildTarget target)
        {
            switch (target)
            {
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    return "PC";
                case BuildTarget.Android:
                    return "Android";
                case BuildTarget.iOS:
                    return "iOS";
                default:
                    return target.ToString();
            }
        }
    }
}
