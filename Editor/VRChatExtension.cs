using System.Collections.Generic;
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

        public override string GetRestoreWarning(IExtensionContext context, Snapshot target)
        {
            return RestoreWarning(context.LanguageCode, EditorUserBuildSettings.activeBuildTarget);
        }

        /// <summary>On Android the re-import after 戻す is noticeably longer, so the warning says so.</summary>
        internal static string RestoreWarning(string languageCode, BuildTarget target)
        {
            var key = target == BuildTarget.Android ? "restore.warning.android" : "restore.warning";
            return VRChatStrings.Tr(languageCode, key);
        }
    }
}
