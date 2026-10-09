# Shiori for VRChat — Claude Code 向け作業ルール

`com.yaito3014.shiori` の上に載せる VRChat 向け拡張パッケージ `com.yaito3014.shiori.vrchat`。
コアパッケージ（`../shiori`、github.com/yaito3014/shiori）の `CLAUDE.md` のルールはすべてここにも適用する。
特に: Unity 2022.3 以上、C# 9 / .NET Standard 2.1、obsolete API はコンパイルエラー、UI は UI Toolkit、
表示文字列はローカライズテーブルから引く、かんたんモードの語彙は 保存 / 履歴 / 戻す / バリエーション / 同期。

まだ骨組みだけで、コードはない。実装は、コア側の拡張 API（下記）が入ってから始める。

## このパッケージに置くもの

- VPM 向けの無視設定: VCC が管理する `Packages/` 配下のパッケージを除外し、
  `Packages/vpm-manifest.json` と `Packages/manifest.json` は追跡する。
  除外する一覧は固定ではなく、ウィザード実行時に `vpm-manifest.json` から作る。
  `.gitignore` には独自のマーカー付きブロック（`# >>> shiori-vrchat ... >>>`）として追記する。
- ウィザードの追加ステップ「VRChat の設定」（無視ファイルの後、最初の保存の前）。
- ビルドターゲット（PC / Android）の表示と、戻す前の再インポート警告。
- 作る人向けの文言（メモの候補、アップロード前の保存の案内）。
- 大きなファイル（FBX、テクスチャ）の LFS 案内。LFS を必須にするかは未決定。

## コア側に必要な拡張 API（コアのリポジトリで実装する）

- `ManagedBlockWriter` のブロック ID 指定（複数ブロックの共存）。
- `Shiori.Editor` の拡張ポイント: 追加ウィザードステップ、追加の無視プリセット、
  保存 / 戻す の前後フック、追加のローカライズテーブル。`TypeCache` で探索し、拡張ゼロでも動く。
- `ProjectSettings/Shiori.json` にパッケージ ID をキーにした拡張用セクション。

## 依存関係と配布

- `vpmDependencies`: `com.yaito3014.shiori` と `com.vrchat.base`。
- 配布は VPM リスティング（GitHub Pages の `index.json`）。コアも同じリスティングに載せる。
  公開手順は未決定（コアの CLAUDE.md の「まだ決まっていないこと」参照）。
- アセンブリは `Shiori.VRChat.Editor` のみ。`Shiori.Core` に相当するものは作らない。

## リポジトリ構成（予定）

```
shiori-vrchat/
  package.json
  README.md
  CHANGELOG.md
  LICENSE                   # MIT
  Editor/
    Shiori.VRChat.Editor.asmdef + csc.rsp（-warnaserror+:CS0618）
    Localization/
  Tests/Editor/
    Shiori.VRChat.Editor.Tests.asmdef
  docs/adr/
```

## まだ決まっていないこと（勝手に決めない）

- 拡張 API の形（コアの M1.5 として着手するか、M2 の後にするか）
- LFS を必須にするか推奨に留めるか
- VPM リスティングの公開手順
- Android ターゲット向けに何を表示するか
