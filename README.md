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
  パッケージを足したあとは `Project Settings > Shiori` の「VRChat の設定」で一覧を更新できます。
- **かんたんモードに今のビルドターゲット（PC / Android）が出ます。**
- **「戻す」の確認に、読み込み直しの注意が出ます。** アップロード中に戻してしまわないための案内です。
- **「保存」の下に、作る人向けの短い案内が出ます**（アップロード前の保存、メモの例）。

`.gitignore` には Shiori 本体とは別の `shiori-vrchat` ブロックを追記します。手で書いた行や Shiori 本体のブロックには触りません。

## インストール

VPM リスティングは準備中です。それまでは Package Manager の "Add package from git URL" で、
Shiori 本体のあとにこのリポジトリを追加してください。

```
https://github.com/yaito3014/shiori.git
https://github.com/yaito3014/shiori-vrchat.git
```

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
