using System.Threading;
using NUnit.Framework;

namespace Shiori.VRChat.Tests
{
    public class VpmIgnoreStepTests
    {
        private FakeExtensionContext _context;
        private VpmIgnoreStep _step;

        [SetUp]
        public void SetUp()
        {
            _context = new FakeExtensionContext();
            _step = new VpmIgnoreStep(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        [Test]
        public void IdentityAndTitle()
        {
            Assert.That(_step.Id, Is.EqualTo(VpmIgnoreStep.StepId));
            Assert.That(_step.Title, Is.EqualTo(VRChatStrings.Japanese["step.title"]));
            _context.LanguageCode = "en";
            Assert.That(_step.Title, Is.EqualTo("step.title"), "English falls back to the key for now");
        }

        [Test]
        public void WithoutVpmManifest_IsDoneWithAnExplanation()
        {
            var view = _step.Evaluate();
            Assert.That(view.Done, Is.True);
            Assert.That(view.Message, Is.EqualTo(VRChatStrings.Japanese["step.notvcc"]));
            Assert.That(view.Actions, Is.Empty);
            Assert.That(_context.Writes, Is.Empty);
        }

        [Test]
        public void WithManifest_OffersToWriteTheBlockThenIsDone()
        {
            _context.WriteProjectFile(VpmManifest.RelativePath, VpmManifestTests.Resolved);

            var view = _step.Evaluate();
            Assert.That(view.Done, Is.False);
            Assert.That(view.Message, Does.Contain("4 件"));
            Assert.That(view.Detail, Does.Contain("com.vrchat.base"));
            Assert.That(view.Actions.Count, Is.EqualTo(1));
            Assert.That(view.Actions[0].Label, Is.EqualTo(VRChatStrings.Japanese["step.apply"]));

            view.Actions[0].Run(CancellationToken.None).GetAwaiter().GetResult();
            Assert.That(_context.Writes, Is.EqualTo(new[] { ".gitignore|shiori-vrchat" }));
            Assert.That(_context.ReadManagedBlock(".gitignore", "shiori-vrchat"), Is.EqualTo(new[]
            {
                "Packages/com.anatawa12.avatar-optimizer/",
                "Packages/com.vrchat.avatars/",
                "Packages/com.vrchat.base/",
                "Packages/com.vrchat.core.vpm-resolver/",
            }));

            var after = _step.Evaluate();
            Assert.That(after.Done, Is.True);
            Assert.That(after.Message, Is.EqualTo(string.Format(VRChatStrings.Japanese["step.ok"], 4)));
            Assert.That(after.Actions, Is.Empty);
        }

        [Test]
        public void ChangedManifest_OffersAnUpdate()
        {
            _context.WriteProjectFile(VpmManifest.RelativePath, VpmManifestTests.Resolved);
            _step.Evaluate().Actions[0].Run(CancellationToken.None).GetAwaiter().GetResult();

            _context.WriteProjectFile(VpmManifest.RelativePath, @"{ ""locked"": { ""com.vrchat.base"": {}, ""com.vrchat.worlds"": {} } }");
            var view = _step.Evaluate();
            Assert.That(view.Done, Is.False);
            Assert.That(view.Message, Is.EqualTo(string.Format(VRChatStrings.Japanese["step.changed"], 2)));
            Assert.That(view.Actions[0].Label, Is.EqualTo(VRChatStrings.Japanese["step.update"]));

            view.Actions[0].Run(CancellationToken.None).GetAwaiter().GetResult();
            Assert.That(_context.ReadManagedBlock(".gitignore", "shiori-vrchat"), Is.EqualTo(new[] { "Packages/com.vrchat.base/", "Packages/com.vrchat.worlds/" }));
            Assert.That(_step.Evaluate().Done, Is.True);
        }

        [Test]
        public void EvaluateAsync_ReturnsTheSameView()
        {
            var view = _step.EvaluateAsync(CancellationToken.None).GetAwaiter().GetResult();
            Assert.That(view.Done, Is.True);
        }
    }
}
