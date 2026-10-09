using System.Collections.Generic;

namespace Shiori.VRChat
{
    /// <summary>
    /// The <c>.gitignore</c> block this package owns: one line per VCC-managed package folder.
    /// <c>Packages/vpm-manifest.json</c>, <c>Packages/manifest.json</c> and user-made packages stay
    /// tracked because only the listed folders are excluded.
    /// </summary>
    internal static class VpmIgnoreBlock
    {
        public const string BlockId = "shiori-vrchat";
        public const string FileName = ".gitignore";

        public static IReadOnlyList<string> Build(IReadOnlyList<string> packageIds)
        {
            var lines = new List<string>(packageIds.Count);
            foreach (var id in packageIds) lines.Add("Packages/" + id + "/");
            return lines;
        }

        public static bool SameLines(IReadOnlyList<string> a, IReadOnlyList<string> b)
        {
            if (a == null || b == null) return false;
            if (a.Count != b.Count) return false;
            for (var i = 0; i < a.Count; i++)
            {
                if (!string.Equals(a[i], b[i], System.StringComparison.Ordinal)) return false;
            }
            return true;
        }
    }
}
