using System.Collections.Generic;

namespace Shiori.VRChat
{
    /// <summary>
    /// UI strings of this package. Plain, short Japanese for people who make avatars and worlds and
    /// do not know git; the core's simple-mode vocabulary (保存 / 履歴 / 戻す) is kept.
    /// English falls back to the key, like the core does for now.
    /// </summary>
    internal static class VRChatStrings
    {
        public static readonly IReadOnlyDictionary<string, string> Japanese = new Dictionary<string, string>
        {
            ["step.title"] = "VRChat の設定",
            ["step.notvcc"] = "VCC（ALCOM）で管理しているプロジェクトではないようです。この手順はそのまま進めてください。",
            ["step.explain"] = "VCC が入れたパッケージ（{0} 件）を履歴に含めない設定にします。パッケージの一覧（vpm-manifest.json）は履歴に残るので、あとで同じ構成に戻せます。",
            ["step.changed"] = "VCC のパッケージ構成が変わっています（今は {0} 件）。履歴に含めない一覧を更新します。",
            ["step.ok"] = "VCC のパッケージ（{0} 件）は履歴に含めない設定になっています。",
            ["step.apply"] = "除外する",
            ["step.update"] = "更新する",

            ["status.target"] = "ビルドターゲット: {0}",
            ["save.hint"] = "アップロードの前に保存しておくと、あとでその時点に戻せます。メモの例:「衣装を追加」「表情を調整」",
            ["restore.warning"] = "戻したあと、アセットの読み込み直しに少し時間がかかります。アップロードの途中なら、終わってから戻してください。",
        };

        public static string Tr(string languageCode, string key)
        {
            if (languageCode == "ja" && Japanese.TryGetValue(key, out var text)) return text;
            return key;
        }

        public static string Tr(string languageCode, string key, params object[] args)
        {
            var text = Tr(languageCode, key);
            return args == null || args.Length == 0 ? text : string.Format(text, args);
        }
    }
}
