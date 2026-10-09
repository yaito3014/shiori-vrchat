using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;

namespace Shiori.VRChat
{
    /// <summary>
    /// What Shiori adds for VRChat projects. Found by the core through <c>TypeCache</c>; nothing
    /// here references the VRChat SDK, so the package also loads in a project without it.
    /// </summary>
    public sealed class VRChatExtension : ShioriExtension
    {
        public const string Id = "com.yaito3014.shiori.vrchat";

        public override string PackageId => Id;

        public override IReadOnlyList<SetupStep> CreateSetupSteps(IExtensionContext context)
        {
            return new SetupStep[] { new VpmIgnoreStep(context) };
        }

        public override string GetStatusLine(IExtensionContext context)
        {
            return VRChatStrings.Tr(context.LanguageCode, "status.target", BuildTargetInfo.Label(EditorUserBuildSettings.activeBuildTarget));
        }

        public override string GetMemoPlaceholder(IExtensionContext context)
        {
            return VRChatStrings.Tr(context.LanguageCode, "memo.placeholder");
        }

        public const string PackagesOutOfDateNoticeId = "vrchat.packages-out-of-date";

        public override string GetRestoreWarning(IExtensionContext context, Snapshot target)
        {
            return RestoreWarning(context.LanguageCode, EditorUserBuildSettings.activeBuildTarget);
        }

        /// <summary>The re-import warning, plus the package-set change when vpm-manifest.json differs in the target.</summary>
        public override async Task<string> GetRestoreWarningAsync(IExtensionContext context, RestorePreview preview, CancellationToken cancellationToken)
        {
            var reimport = RestoreWarning(context.LanguageCode, EditorUserBuildSettings.activeBuildTarget);
            if (preview == null || context.Repository == null || !preview.Changes(VpmManifest.RelativePath)) return reimport;

            var targetJson = await context.Repository.ReadFileAtAsync(preview.Target.Hash, VpmManifest.RelativePath, cancellationToken);
            var packages = VpmMessages.RestoreWarning(context.LanguageCode, VpmManifestDiff.Compare(LoadOrNull(context.ProjectRoot), ParseOrNull(targetJson)));
            return packages == null ? reimport : packages + "\n\n" + reimport;
        }

        /// <summary>While Packages/ does not match the locked versions, ask the user to resolve them.</summary>
        public override Task<IReadOnlyList<ExtensionNotice>> GetNoticesAsync(IExtensionContext context, CancellationToken cancellationToken)
        {
            var notice = VpmMessages.OutOfDateNotice(context.LanguageCode, VpmPackageState.FindMismatches(context.ProjectRoot, LoadOrNull(context.ProjectRoot)));
            IReadOnlyList<ExtensionNotice> notices = notice == null ? Array.Empty<ExtensionNotice>() : new[] { notice };
            return Task.FromResult(notices);
        }

        /// <summary>A manifest that cannot be read is treated as absent: these messages are advice, not a gate.</summary>
        private static VpmManifest LoadOrNull(string projectRoot)
        {
            try
            {
                return VpmManifest.Load(projectRoot);
            }
            catch (FormatException)
            {
                return null;
            }
        }

        private static VpmManifest ParseOrNull(string json)
        {
            if (json == null) return null;
            try
            {
                return VpmManifest.Parse(json);
            }
            catch (FormatException)
            {
                return null;
            }
        }

        /// <summary>On Android the re-import after 戻す is noticeably longer, so the warning says so.</summary>
        internal static string RestoreWarning(string languageCode, BuildTarget target)
        {
            var key = target == BuildTarget.Android ? "restore.warning.android" : "restore.warning";
            return VRChatStrings.Tr(languageCode, key);
        }
    }
}
