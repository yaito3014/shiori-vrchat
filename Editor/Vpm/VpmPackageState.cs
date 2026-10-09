using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Shiori.VRChat
{
    /// <summary>One package whose installed folder does not match the version locked in vpm-manifest.json.</summary>
    internal sealed class VpmMismatch
    {
        public string PackageId { get; }
        public string LockedVersion { get; }

        /// <summary>The version found in <c>Packages/&lt;id&gt;/package.json</c>, or null when the package is not installed.</summary>
        public string InstalledVersion { get; }

        public VpmMismatch(string packageId, string lockedVersion, string installedVersion)
        {
            PackageId = packageId;
            LockedVersion = lockedVersion;
            InstalledVersion = installedVersion;
        }
    }

    /// <summary>
    /// Compares what vpm-manifest.json asks for with what is in <c>Packages/</c>. Because Packages/ is
    /// kept out of the history, a 戻す (or a fresh clone) can leave the two apart until VCC / ALCOM
    /// resolves the project. Only packages listed in <c>locked</c> are checked; folders that are not in
    /// the manifest may be the user's own packages and are left alone.
    /// </summary>
    internal static class VpmPackageState
    {
        private static readonly Encoding Utf8 = new UTF8Encoding(false);

        public static IReadOnlyList<VpmMismatch> FindMismatches(string projectRoot, VpmManifest manifest)
        {
            var result = new List<VpmMismatch>();
            if (manifest == null) return result;
            foreach (var pair in manifest.LockedVersions)
            {
                var installed = InstalledVersion(projectRoot, pair.Key);
                if (!string.Equals(installed, pair.Value, StringComparison.Ordinal)) result.Add(new VpmMismatch(pair.Key, pair.Value, installed));
            }
            return result;
        }

        /// <summary>The <c>version</c> of <c>Packages/&lt;id&gt;/package.json</c>, or null when missing or unreadable.</summary>
        internal static string InstalledVersion(string projectRoot, string packageId)
        {
            var path = Path.Combine(projectRoot, "Packages", packageId, "package.json");
            if (!File.Exists(path)) return null;
            try
            {
                var json = MiniJson.Parse(File.ReadAllText(path, Utf8)) as Dictionary<string, object>;
                return json != null && json.TryGetValue("version", out var version) ? version as string : null;
            }
            catch (FormatException)
            {
                return null;
            }
            catch (IOException)
            {
                return null;
            }
        }
    }

    /// <summary>How the locked package set differs between two manifests (now, and the snapshot 戻す goes to).</summary>
    internal sealed class VpmManifestDiff
    {
        public IReadOnlyList<string> Added { get; }
        public IReadOnlyList<string> Removed { get; }

        /// <summary>Packages in both, with a different locked version: "id (old → new)".</summary>
        public IReadOnlyList<string> Changed { get; }

        public bool IsEmpty => Added.Count == 0 && Removed.Count == 0 && Changed.Count == 0;

        private VpmManifestDiff(IReadOnlyList<string> added, IReadOnlyList<string> removed, IReadOnlyList<string> changed)
        {
            Added = added;
            Removed = removed;
            Changed = changed;
        }

        /// <summary>A null manifest counts as having no packages.</summary>
        public static VpmManifestDiff Compare(VpmManifest from, VpmManifest to)
        {
            var before = from?.LockedVersions ?? new Dictionary<string, string>();
            var after = to?.LockedVersions ?? new Dictionary<string, string>();
            var added = new List<string>();
            var removed = new List<string>();
            var changed = new List<string>();
            foreach (var pair in after)
            {
                if (!before.TryGetValue(pair.Key, out var old)) added.Add(pair.Key);
                else if (!string.Equals(old, pair.Value, StringComparison.Ordinal)) changed.Add(pair.Key + " (" + old + " → " + pair.Value + ")");
            }
            foreach (var pair in before)
            {
                if (!after.ContainsKey(pair.Key)) removed.Add(pair.Key);
            }
            added.Sort(StringComparer.Ordinal);
            removed.Sort(StringComparer.Ordinal);
            changed.Sort(StringComparer.Ordinal);
            return new VpmManifestDiff(added, removed, changed);
        }
    }
}
