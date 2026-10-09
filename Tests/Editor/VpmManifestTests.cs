using System;
using System.IO;
using NUnit.Framework;

namespace Shiori.VRChat.Tests
{
    public class VpmManifestTests
    {
        public const string Resolved = @"{
  ""dependencies"": {
    ""com.vrchat.avatars"": { ""version"": ""3.7.4"" },
    ""com.anatawa12.avatar-optimizer"": { ""version"": ""1.8.0"" }
  },
  ""locked"": {
    ""com.vrchat.avatars"": { ""version"": ""3.7.4"", ""dependencies"": { ""com.vrchat.base"": ""3.7.4"" } },
    ""com.vrchat.base"": { ""version"": ""3.7.4"", ""dependencies"": {} },
    ""com.anatawa12.avatar-optimizer"": { ""version"": ""1.8.0"", ""dependencies"": {} },
    ""com.vrchat.core.vpm-resolver"": { ""version"": ""0.1.28"", ""dependencies"": {} }
  }
}";

        [Test]
        public void Parse_UsesLockedPackagesSorted()
        {
            var manifest = VpmManifest.Parse(Resolved);
            Assert.That(manifest.PackageIds, Is.EqualTo(new[]
            {
                "com.anatawa12.avatar-optimizer",
                "com.vrchat.avatars",
                "com.vrchat.base",
                "com.vrchat.core.vpm-resolver",
            }));
        }

        [Test]
        public void Parse_FallsBackToDependenciesWhenNothingIsLocked()
        {
            var manifest = VpmManifest.Parse(@"{ ""dependencies"": { ""com.vrchat.worlds"": { ""version"": ""3.7.4"" } }, ""locked"": {} }");
            Assert.That(manifest.PackageIds, Is.EqualTo(new[] { "com.vrchat.worlds" }));

            Assert.That(VpmManifest.Parse("{}").PackageIds, Is.Empty);
        }

        [Test]
        public void Parse_SkipsNamesThatAreNotPlainFolderNames()
        {
            var manifest = VpmManifest.Parse(@"{ ""locked"": { ""com.ok"": {}, ""../escape"": {}, ""with space"": {}, ""star*"": {} } }");
            Assert.That(manifest.PackageIds, Is.EqualTo(new[] { "com.ok" }));
        }

        [Test]
        public void Parse_RejectsNonObjects()
        {
            Assert.That(() => VpmManifest.Parse("[]"), Throws.TypeOf<FormatException>());
            Assert.That(() => VpmManifest.Parse("not json"), Throws.TypeOf<FormatException>());
            Assert.That(() => VpmManifest.Parse(null), Throws.ArgumentNullException);
        }

        [Test]
        public void Load_ReturnsNullWithoutManifest()
        {
            using (var context = new FakeExtensionContext())
            {
                Assert.That(VpmManifest.Load(context.ProjectRoot), Is.Null);
                context.WriteProjectFile(VpmManifest.RelativePath, Resolved);
                Assert.That(VpmManifest.PathIn(context.ProjectRoot), Is.EqualTo(Path.Combine(context.ProjectRoot, "Packages", "vpm-manifest.json")));
                Assert.That(VpmManifest.Load(context.ProjectRoot).PackageIds.Count, Is.EqualTo(4));
            }
        }

        [Test]
        public void IgnoreBlock_HasOneFolderLinePerPackage()
        {
            var lines = VpmIgnoreBlock.Build(new[] { "com.vrchat.base", "com.vrchat.avatars" });
            Assert.That(lines, Is.EqualTo(new[] { "Packages/com.vrchat.base/", "Packages/com.vrchat.avatars/" }));
            Assert.That(VpmIgnoreBlock.SameLines(lines, VpmIgnoreBlock.Build(new[] { "com.vrchat.base", "com.vrchat.avatars" })), Is.True);
            Assert.That(VpmIgnoreBlock.SameLines(lines, VpmIgnoreBlock.Build(new[] { "com.vrchat.base" })), Is.False);
            Assert.That(VpmIgnoreBlock.SameLines(null, lines), Is.False);
        }
    }
}
