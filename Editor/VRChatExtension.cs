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
        public override Task<string> GetRestoreWarningAsync(IExtensionContext context, RestorePreview preview, CancellationToken cancellationToken)
        {
            var reimport = RestoreWarning(context.LanguageCode, EditorUserBuildSettings.activeBuildTarget);
            return WithPackagesAsync(context, preview, reimport, VpmMessages.RestoreWarning, cancellationToken);
        }

        /// <summary>The same for switching バリエーション: the package-set change, and the upload / Android advice.</summary>
        public override Task<string> GetSwitchWarningAsync(IExtensionContext context, RestorePreview preview, CancellationToken cancellationToken)
        {
            var advice = SwitchWarning(context.LanguageCode, EditorUserBuildSettings.activeBuildTarget);
            return WithPackagesAsync(context, preview, advice, VpmMessages.SwitchWarning, cancellationToken);
        }

        private static async Task<string> WithPackagesAsync(IExtensionContext context, RestorePreview preview, string advice,
            Func<string, VpmManifestDiff, string> describePackages, CancellationToken cancellationToken)
        {
            if (preview == null || context.Repository == null || !preview.Changes(VpmManifest.RelativePath)) return advice;

            var targetJson = await context.Repository.ReadFileAtAsync(preview.Target.Hash, VpmManifest.RelativePath, cancellationToken);
            var packages = describePackages(context.LanguageCode, VpmManifestDiff.Compare(LoadOrNull(context.ProjectRoot), ParseOrNull(targetJson)));
            return packages == null ? advice : packages + "\n\n" + advice;
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

        /// <summary>The core already says files are re-imported; this adds the upload and Android advice.</summary>
        internal static string SwitchWarning(string languageCode, BuildTarget target)
        {
            var key = target == BuildTarget.Android ? "switch.warning.android" : "switch.warning";
            return VRChatStrings.Tr(languageCode, key);
        }
    }
}
