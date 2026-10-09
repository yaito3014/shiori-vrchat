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
            ["step.more"] = "ほか {0} 件",

            ["status.target"] = "{0} 向け",
            ["memo.placeholder"] = "例: 衣装を追加、表情を調整、アップロード前",
            ["restore.warning"] = "戻したあと、アセットの読み込み直しに少し時間がかかります。アップロードの途中なら、終わってから戻してください。",
            ["packages.restore"] = "VCC のパッケージ構成も戻ります（追加 {0} / 削除 {1} / 版の変更 {2}）。戻したあと、パッケージをそろえるまでエラーが出ることがあります。",
            ["packages.added"] = "増えるもの:",
            ["packages.removed"] = "なくなるもの:",
            ["packages.changed"] = "版が変わるもの:",
            ["packages.title"] = "パッケージをそろえてください",
            ["packages.notice"] = "パッケージ {0} 件が、保存されている構成（vpm-manifest.json）と違います。VCC / ALCOM でこのプロジェクトのパッケージを確認し、そろえてください。Unity にパッケージを解決するボタンが出たときは、それを押してもかまいません。",
            ["packages.missing"] = "{0}: {1} が必要です（入っていません）",
            ["packages.version"] = "{0}: {1} が必要です（今は {2}）",

            ["restore.warning.android"] ="戻したあと、アセットの読み込み直しが始まります。Android 向けのときは数分かかることがあります。アップロードの途中なら、終わってから戻してください。",
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
