using NUnit.Framework;
using UnityEditor;

namespace Shiori.VRChat.Tests
{
    public class VRChatExtensionTests
    {
        [Test]
        public void PackageIdMatchesTheManifest()
        {
            Assert.That(new VRChatExtension().PackageId, Is.EqualTo("com.yaito3014.shiori.vrchat"));
        }

        [Test]
        public void ContributesOneStepAndJapaneseText()
        {
            using (var context = new FakeExtensionContext())
            {
                var extension = new VRChatExtension();
                var steps = extension.CreateSetupSteps(context);
                Assert.That(steps.Count, Is.EqualTo(1));
                Assert.That(steps[0], Is.InstanceOf<VpmIgnoreStep>());

                Assert.That(extension.GetStatusLine(context), Does.EndWith(" 向け"));
                Assert.That(extension.GetMemoPlaceholder(context), Is.EqualTo(VRChatStrings.Japanese["memo.placeholder"]));
                Assert.That(extension.GetRestoreWarning(context, null), Is.Not.Empty);
                Assert.That(VRChatExtension.RestoreWarning("ja", BuildTarget.StandaloneWindows64), Is.EqualTo(VRChatStrings.Japanese["restore.warning"]));
                Assert.That(VRChatExtension.RestoreWarning("ja", BuildTarget.Android), Is.EqualTo(VRChatStrings.Japanese["restore.warning.android"]));

                // The simple-mode vocabulary stays git-free.
                foreach (var text in VRChatStrings.Japanese.Values)
                {
                    foreach (var word in new[] { "commit", "stage", "branch", "push", "pull", "git" })
                    {
                        Assert.That(text.ToLowerInvariant(), Does.Not.Contain(word), text);
                    }
                }
            }
        }

        [Test]
        public void BuildTargetLabels()
        {
            Assert.That(BuildTargetInfo.Label(BuildTarget.StandaloneWindows64), Is.EqualTo("PC"));
            Assert.That(BuildTargetInfo.Label(BuildTarget.StandaloneWindows), Is.EqualTo("PC"));
            Assert.That(BuildTargetInfo.Label(BuildTarget.Android), Is.EqualTo("Android"));
            Assert.That(BuildTargetInfo.Label(BuildTarget.iOS), Is.EqualTo("iOS"));
            Assert.That(BuildTargetInfo.Label(BuildTarget.StandaloneOSX), Is.EqualTo("StandaloneOSX"));
        }

        [Test]
        public void Strings_FormatAndFallBack()
        {
            Assert.That(VRChatStrings.Tr("ja", "status.target", "PC"), Is.EqualTo("PC 向け"));
            Assert.That(VRChatStrings.Tr("en", "status.target", "PC"), Is.EqualTo("status.target"));
            Assert.That(VRChatStrings.Tr("ja", "no.such.key"), Is.EqualTo("no.such.key"));
        }
    }
}
