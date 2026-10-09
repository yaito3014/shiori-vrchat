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

        public override string GetSaveHint(IExtensionContext context)
        {
            return VRChatStrings.Tr(context.LanguageCode, "save.hint");
        }

        public override string GetRestoreWarning(IExtensionContext context, Snapshot target)
        {
            return VRChatStrings.Tr(context.LanguageCode, "restore.warning");
        }
    }
}
