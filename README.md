# Shiori for VRChat

[Shiori](https://github.com/yaito3014/shiori)（Unity Editor の中で動く Git クライアント）の VRChat 向け拡張です。
VCC / ALCOM で管理しているアバター・ワールドのプロジェクトで、Shiori をそのまま使いやすくします。

- 対応 Unity: 2022.3 以上（Unity 6 を含む）
- 必要なもの: Shiori 本体（`com.yaito3014.shiori`）
- VRChat SDK は参照しません。SDK が入っていないプロジェクトでも動きます。

## できること

- **はじめの設定に「VRChat の設定」が増えます。** VCC が入れたパッケージ（`Packages/` の中）を履歴に含めない設定にします。
  一覧は `Packages/vpm-manifest.json` から作るので、プロジェクトごとに合った内容になります。
  パッケージの一覧そのもの（`vpm-manifest.json` / `manifest.json`）は履歴に残るので、あとで同じ構成に戻せます。
  パッケージを足したあとは、かんたんモードの上部に「更新する」ボタン付きで知らせが出ます
  （`Project Settings > Shiori` の「VRChat の設定」からも更新できます）。
- **ウィンドウ右上に今のビルドターゲットが出ます**（「PC 向け」「Android 向け」）。
- **「戻す」の確認に、読み込み直しの注意が出ます。** アップロード中に戻してしまわないための案内です。Android 向けのときは時間がかかることも書きます。
- **空のメモ欄に例文が出ます**（「例: 衣装を追加、表情を調整、アップロード前」）。

`.gitignore` には Shiori 本体とは別の `shiori-vrchat` ブロックを追記します。手で書いた行や Shiori 本体のブロックには触りません。

## インストール

### VCC / ALCOM（おすすめ）

[yaito3014.github.io/vpm-listing](https://yaito3014.github.io/vpm-listing/) の「VCC / ALCOM に追加」を押して
リポジトリを登録し、プロジェクトの「Manage Project」から **Shiori for VRChat** を追加してください。
Shiori 本体は依存関係として一緒に入ります。

### Package Manager（VCC を使わない場合）

Package Manager は依存関係を自動では入れないので、本体を先に入れます。

1. [Shiori の最新リリース](https://github.com/yaito3014/shiori/releases/latest)から `com.yaito3014.shiori-<version>.tgz`、
   [このリポジトリの最新リリース](https://github.com/yaito3014/shiori-vrchat/releases/latest)から
   `com.yaito3014.shiori.vrchat-<version>.tgz` をダウンロードします。
2. Package Manager の「+」から "Install package from tarball..." で、本体、拡張の順に指定します。

どちらも Unity の署名付きです。

## 開発

検証用の Unity プロジェクトは Shiori 本体と共用です（`../shiori-dev/<stream>`）。

```
# 本体の検証プロジェクトにこのパッケージを足す（初回のみ）
pwsh Tools~/Add-ToDevProject.ps1 -Stream 2022.3

# テスト（本体のスクリプトを使う。本体のテストも一緒に走る）
pwsh ../shiori/Tools~/Test-DevProject.ps1 -UnityVersion 2022.3.22f1
pwsh ../shiori/Tools~/Test-DevProject.ps1 -UnityVersion 6000.6.0f1
```

作業ルールは `CLAUDE.md` にあります。

## ライセンス

MIT
