using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Shiori.VRChat
{
    /// <summary>
    /// The packages VCC / ALCOM installed into <c>Packages/</c>, read from <c>Packages/vpm-manifest.json</c>.
    /// Only the package names are needed: each one is a folder the resolver can recreate, so it
    /// does not belong in the history.
    /// </summary>
    internal sealed class VpmManifest
    {
        public const string RelativePath = "Packages/vpm-manifest.json";

        private static readonly Encoding Utf8 = new UTF8Encoding(false);

        /// <summary>Package names, sorted, without duplicates.</summary>
        public IReadOnlyList<string> PackageIds { get; }

        private VpmManifest(IReadOnlyList<string> packageIds)
        {
            PackageIds = packageIds;
        }

        public static string PathIn(string projectRoot)
        {
            return System.IO.Path.Combine(projectRoot, "Packages", "vpm-manifest.json");
        }

        /// <summary>Null when the project has no VPM manifest (it is not managed by VCC / ALCOM).</summary>
        public static VpmManifest Load(string projectRoot)
        {
            var path = PathIn(projectRoot);
            if (!File.Exists(path)) return null;
            return Parse(File.ReadAllText(path, Utf8));
        }

        /// <summary>
        /// Reads the <c>locked</c> object (every installed package, dependencies included) and falls back
        /// to <c>dependencies</c> for a manifest that has not been resolved yet.
        /// Throws <see cref="FormatException"/> when the text is not a JSON object.
        /// </summary>
        public static VpmManifest Parse(string json)
        {
            if (json == null) throw new ArgumentNullException(nameof(json));
            var root = MiniJson.Parse(json) as Dictionary<string, object>
                       ?? throw new FormatException("vpm-manifest.json: top-level value is not an object");

            var ids = new SortedSet<string>(StringComparer.Ordinal);
            AddKeys(ids, root, "locked");
            if (ids.Count == 0) AddKeys(ids, root, "dependencies");
            return new VpmManifest(new List<string>(ids));
        }

        private static void AddKeys(SortedSet<string> ids, Dictionary<string, object> root, string section)
        {
            if (!root.TryGetValue(section, out var value) || !(value is Dictionary<string, object> entries)) return;
            foreach (var key in entries.Keys)
            {
                if (!string.IsNullOrWhiteSpace(key) && IsPackageName(key)) ids.Add(key);
            }
        }

        /// <summary>A package name is a plain folder name: no separators, no wildcard characters that would change a .gitignore line.</summary>
        internal static bool IsPackageName(string name)
        {
            foreach (var c in name)
            {
                if (char.IsLetterOrDigit(c) || c == '.' || c == '-' || c == '_') continue;
                return false;
            }
            return true;
        }
    }
}
