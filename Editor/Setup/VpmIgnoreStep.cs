using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Shiori.VRChat
{
    /// <summary>
    /// Wizard step 「VRChat の設定」: keep VCC-managed packages out of the history. The list is
    /// rebuilt from <c>vpm-manifest.json</c> every time, so adding a package later shows the step
    /// as pending again (also under Project Settings). A project without a VPM manifest is not a
    /// VCC project; the step then has nothing to do and counts as done.
    /// </summary>
    internal sealed class VpmIgnoreStep : SetupStep
    {
        public const string StepId = "vrchat.vpm-ignore";

        private readonly IExtensionContext _context;

        public VpmIgnoreStep(IExtensionContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public override string Id => StepId;

        public override string Title => Tr("step.title");

        public override Task<SetupStepView> EvaluateAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(Evaluate());
        }

        internal SetupStepView Evaluate()
        {
            var manifest = VpmManifest.Load(_context.ProjectRoot);
            if (manifest == null)
            {
                return new SetupStepView(true, Tr("step.notvcc"));
            }

            var wanted = VpmIgnoreBlock.Build(manifest.PackageIds);
            var current = _context.ReadManagedBlock(VpmIgnoreBlock.FileName, VpmIgnoreBlock.BlockId);
            var detail = Summarize(manifest.PackageIds);

            if (VpmIgnoreBlock.SameLines(current, wanted))
            {
                return new SetupStepView(true, Tr("step.ok", wanted.Count), detail);
            }

            var message = current == null ? Tr("step.explain", wanted.Count) : Tr("step.changed", wanted.Count);
            var label = current == null ? Tr("step.apply") : Tr("step.update");
            var actions = new[] { new SetupStepAction(label, () => Apply(wanted)) };
            return new SetupStepView(false, message, detail, actions);
        }

        /// <summary>Avatar projects lock dozens of packages; the step shows the first few and a count for the rest.</summary>
        internal const int DetailLines = 5;

        internal string Summarize(IReadOnlyList<string> packageIds)
        {
            if (packageIds.Count <= DetailLines) return string.Join("\n", packageIds);
            var shown = new List<string>(DetailLines + 1);
            for (var i = 0; i < DetailLines; i++) shown.Add(packageIds[i]);
            shown.Add(Tr("step.more", packageIds.Count - DetailLines));
            return string.Join("\n", shown);
        }

        private void Apply(IReadOnlyList<string> lines)
        {
            _context.UpsertManagedBlock(VpmIgnoreBlock.FileName, VpmIgnoreBlock.BlockId, lines);
        }

        private string Tr(string key, params object[] args)
        {
            return VRChatStrings.Tr(_context.LanguageCode, key, args);
        }
    }
}
