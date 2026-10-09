using System.Collections.Generic;
using System.Text;

namespace Shiori.VRChat
{
    /// <summary>
    /// Turns package comparisons into the text Shiori shows. Kept free of Unity and git so it can be
    /// tested directly. The wording does not assume how the packages get fixed: both VCC / ALCOM and
    /// the SDK's own resolver bring Packages/ back in line with vpm-manifest.json.
    /// </summary>
    internal static class VpmMessages
    {
        public const int ListLimit = 5;

        /// <summary>The 戻す warning about the package set, or null when the set does not change.</summary>
        public static string RestoreWarning(string languageCode, VpmManifestDiff diff)
        {
            if (diff == null || diff.IsEmpty) return null;
            var sb = new StringBuilder();
            sb.Append(VRChatStrings.Tr(languageCode, "packages.restore", diff.Added.Count, diff.Removed.Count, diff.Changed.Count));
            AppendList(sb, languageCode, "packages.added", diff.Added);
            AppendList(sb, languageCode, "packages.removed", diff.Removed);
            AppendList(sb, languageCode, "packages.changed", diff.Changed);
            return sb.ToString();
        }

        /// <summary>The notice while Packages/ does not match vpm-manifest.json, or null when they match.</summary>
        public static ExtensionNotice OutOfDateNotice(string languageCode, IReadOnlyList<VpmMismatch> mismatches)
        {
            if (mismatches == null || mismatches.Count == 0) return null;
            var lines = new List<string>();
            foreach (var mismatch in mismatches)
            {
                lines.Add(mismatch.InstalledVersion == null
                    ? VRChatStrings.Tr(languageCode, "packages.missing", mismatch.PackageId, mismatch.LockedVersion)
                    : VRChatStrings.Tr(languageCode, "packages.version", mismatch.PackageId, mismatch.LockedVersion, mismatch.InstalledVersion));
            }
            return new ExtensionNotice(
                VRChatExtension.PackagesOutOfDateNoticeId,
                VRChatStrings.Tr(languageCode, "packages.title"),
                VRChatStrings.Tr(languageCode, "packages.notice", mismatches.Count),
                Limit(languageCode, lines));
        }

        private static void AppendList(StringBuilder sb, string languageCode, string headingKey, IReadOnlyList<string> items)
        {
            if (items.Count == 0) return;
            sb.Append('\n').Append(VRChatStrings.Tr(languageCode, headingKey)).Append('\n').Append(Limit(languageCode, items));
        }

        internal static string Limit(string languageCode, IReadOnlyList<string> items)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < items.Count && i < ListLimit; i++)
            {
                if (i > 0) sb.Append('\n');
                sb.Append("・").Append(items[i]);
            }
            if (items.Count > ListLimit) sb.Append('\n').Append(VRChatStrings.Tr(languageCode, "step.more", items.Count - ListLimit));
            return sb.ToString();
        }
    }
}
