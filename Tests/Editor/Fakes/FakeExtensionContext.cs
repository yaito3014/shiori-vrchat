using System;
using System.Collections.Generic;
using System.IO;

namespace Shiori.VRChat.Tests
{
    /// <summary>An in-memory <see cref="IExtensionContext"/> over a throw-away project folder; blocks never touch the disk.</summary>
    internal sealed class FakeExtensionContext : IExtensionContext, IDisposable
    {
        private readonly Dictionary<string, IReadOnlyList<string>> _blocks = new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal);

        public string ProjectRoot { get; }
        public string LanguageCode { get; set; } = "ja";
        public IGitRepository Repository => null;
        public IDictionary<string, object> Settings { get; } = new Dictionary<string, object>(StringComparer.Ordinal);
        public int Saves { get; private set; }
        public List<string> Writes { get; } = new List<string>();

        public FakeExtensionContext()
        {
            ProjectRoot = Path.Combine(Path.GetTempPath(), "shiori-vrchat-test-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(ProjectRoot);
        }

        public void WriteProjectFile(string relativePath, string content)
        {
            var full = Path.Combine(ProjectRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(full));
            File.WriteAllText(full, content);
        }

        public IReadOnlyList<string> ReadManagedBlock(string fileName, string blockId)
        {
            return _blocks.TryGetValue(fileName + "|" + blockId, out var lines) ? lines : null;
        }

        public bool UpsertManagedBlock(string fileName, string blockId, IReadOnlyList<string> lines)
        {
            Writes.Add(fileName + "|" + blockId);
            var key = fileName + "|" + blockId;
            var changed = !_blocks.TryGetValue(key, out var current) || !VpmIgnoreBlock.SameLines(current, lines);
            _blocks[key] = new List<string>(lines);
            return changed;
        }

        public void SaveSettings()
        {
            Saves++;
        }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(ProjectRoot)) Directory.Delete(ProjectRoot, true);
            }
            catch (IOException)
            {
            }
        }
    }
}
