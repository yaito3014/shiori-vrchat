using System.Linq;
using System.Threading;
using NUnit.Framework;

namespace Shiori.VRChat.Tests
{
    public class VpmPackageStateTests
    {
        private const string Manifest = @"{
  ""dependencies"": { ""com.vrchat.avatars"": { ""version"": ""3.7.x"" } },
  ""locked"": {
    ""com.vrchat.avatars"": { ""version"": ""3.7.4"" },
    ""com.vrchat.base"": { ""version"": ""3.7.4"" },
    ""com.anatawa12.avatar-optimizer"": { ""version"": ""1.8.0"" }
  }
}";

        private static string Package(string version) => "{ \"name\": \"x\", \"version\": \"" + version + "\" }";

        [Test]
        public void LockedVersions_AreReadFromTheLockedSectionOnly()
        {
            var manifest = VpmManifest.Parse(Manifest);
            Assert.That(manifest.LockedVersions["com.vrchat.base"], Is.EqualTo("3.7.4"));
            Assert.That(manifest.LockedVersions.Count, Is.EqualTo(3));
            Assert.That(VpmManifest.Parse(@"{ ""dependencies"": { ""a.b"": { ""version"": ""^1"" } } }").LockedVersions, Is.Empty, "ranges are not versions");
        }

        [Test]
        public void Mismatches_ListMissingAndOtherVersions()
        {
            using (var context = new FakeExtensionContext())
            {
                context.WriteProjectFile("Packages/com.vrchat.avatars/package.json", Package("3.7.4"));
                context.WriteProjectFile("Packages/com.vrchat.base/package.json", Package("3.7.3"));
                context.WriteProjectFile("Packages/my.own.package/package.json", Package("0.0.1"));

                var mismatches = VpmPackageState.FindMismatches(context.ProjectRoot, VpmManifest.Parse(Manifest));

                Assert.That(mismatches.Select(m => m.PackageId), Is.EqualTo(new[] { "com.anatawa12.avatar-optimizer", "com.vrchat.base" }));
                Assert.That(mismatches[0].InstalledVersion, Is.Null);
                Assert.That(mismatches[1].InstalledVersion, Is.EqualTo("3.7.3"));
                Assert.That(VpmPackageState.FindMismatches(context.ProjectRoot, null), Is.Empty);
            }
        }

        [Test]
        public void Diff_NamesAddedRemovedAndChangedPackages()
        {
            var now = VpmManifest.Parse(Manifest);
            var older = VpmManifest.Parse(@"{ ""locked"": { ""com.vrchat.avatars"": { ""version"": ""3.7.0"" }, ""com.vrchat.base"": { ""version"": ""3.7.4"" }, ""old.tool"": { ""version"": ""1.0.0"" } } }");

            var diff = VpmManifestDiff.Compare(now, older);

            Assert.That(diff.Added, Is.EqualTo(new[] { "old.tool" }));
            Assert.That(diff.Removed, Is.EqualTo(new[] { "com.anatawa12.avatar-optimizer" }));
            Assert.That(diff.Changed, Is.EqualTo(new[] { "com.vrchat.avatars (3.7.4 → 3.7.0)" }));
            Assert.That(VpmManifestDiff.Compare(now, VpmManifest.Parse(Manifest)).IsEmpty, Is.True);
            Assert.That(VpmManifestDiff.Compare(null, now).Added.Count, Is.EqualTo(3), "no manifest before counts as no packages");
        }

        [Test]
        public void RestoreWarning_ListsTheChangesOrIsNull()
        {
            var diff = VpmManifestDiff.Compare(VpmManifest.Parse(Manifest), VpmManifest.Parse(@"{ ""locked"": { ""com.vrchat.base"": { ""version"": ""3.7.4"" } } }"));
            var text = VpmMessages.RestoreWarning("ja", diff);
            Assert.That(text, Does.StartWith(string.Format(VRChatStrings.Japanese["packages.restore"], 0, 2, 0)));
            Assert.That(text, Does.Contain(VRChatStrings.Japanese["packages.removed"] + "\n・com.anatawa12.avatar-optimizer\n・com.vrchat.avatars"));
            Assert.That(text, Does.Not.Contain(VRChatStrings.Japanese["packages.added"]));
            Assert.That(VpmMessages.RestoreWarning("ja", VpmManifestDiff.Compare(null, null)), Is.Null);
        }

        [Test]
        public void Notice_AppearsOnlyWhileSomethingDiffers()
        {
            Assert.That(VpmMessages.OutOfDateNotice("ja", new VpmMismatch[0]), Is.Null);
            var notice = VpmMessages.OutOfDateNotice("ja", new[] { new VpmMismatch("a.b", "1.0.0", null), new VpmMismatch("c.d", "2.0.0", "1.9.0") });
            Assert.That(notice.Id, Is.EqualTo(VRChatExtension.PackagesOutOfDateNoticeId));
            Assert.That(notice.Title, Is.EqualTo(VRChatStrings.Japanese["packages.title"]));
            Assert.That(notice.Message, Is.EqualTo(string.Format(VRChatStrings.Japanese["packages.notice"], 2)));
            Assert.That(notice.Detail, Is.EqualTo("・a.b: 1.0.0 が必要です（入っていません）\n・c.d: 2.0.0 が必要です（今は 1.9.0）"));
        }

        [Test]
        public void List_StopsAtFiveItems()
        {
            var items = new[] { "1", "2", "3", "4", "5", "6", "7" };
            Assert.That(VpmMessages.Limit("ja", items), Is.EqualTo("・1\n・2\n・3\n・4\n・5\n" + string.Format(VRChatStrings.Japanese["step.more"], 2)));
        }

        [Test]
        public void Extension_ReportsTheNoticeFromProjectState()
        {
            using (var context = new FakeExtensionContext())
            {
                var extension = new VRChatExtension();
                Assert.That(extension.GetNoticesAsync(context, CancellationToken.None).GetAwaiter().GetResult(), Is.Empty, "not a VCC project");

                context.WriteProjectFile(VpmManifest.RelativePath, @"{ ""locked"": { ""com.vrchat.base"": { ""version"": ""3.7.4"" } } }");
                var notices = extension.GetNoticesAsync(context, CancellationToken.None).GetAwaiter().GetResult();
                Assert.That(notices.Single().Id, Is.EqualTo(VRChatExtension.PackagesOutOfDateNoticeId));

                context.WriteProjectFile("Packages/com.vrchat.base/package.json", Package("3.7.4"));
                Assert.That(extension.GetNoticesAsync(context, CancellationToken.None).GetAwaiter().GetResult(), Is.Empty, "resolved packages clear the notice");

                context.WriteProjectFile(VpmManifest.RelativePath, "not json");
                Assert.That(extension.GetNoticesAsync(context, CancellationToken.None).GetAwaiter().GetResult(), Is.Empty, "a broken manifest is not reported here");
            }
        }

        [Test]
        public void Extension_RestoreWarningWithoutRepository_IsTheReimportWarning()
        {
            using (var context = new FakeExtensionContext())
            {
                var snapshot = new Snapshot("abc", System.DateTimeOffset.Now, "first", "me", null);
                var preview = new RestorePreview(snapshot, new[] { VpmManifest.RelativePath });
                var text = new VRChatExtension().GetRestoreWarningAsync(context, preview, CancellationToken.None).GetAwaiter().GetResult();
                Assert.That(text, Does.StartWith("戻したあと"), "the fake context has no repository, so only the re-import warning remains");
            }
        }
    }
}
