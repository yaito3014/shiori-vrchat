# Shiori for VRChat — Claude Code 向け作業ルール

`com.yaito3014.shiori` の上に載せる VRChat 向け拡張パッケージ `com.yaito3014.shiori.vrchat`。
コアパッケージ（`../shiori`、github.com/yaito3014/shiori）の `CLAUDE.md` のルールはすべてここにも適用する。
特に: Unity 2022.3 以上、C# 9 / .NET Standard 2.1、obsolete API はコンパイルエラー、UI は UI Toolkit、
表示文字列はローカライズテーブルから引く、かんたんモードの語彙は 保存 / 履歴 / 戻す / バリエーション / 同期。

## 仕組み

コアの拡張 API（`Shiori.Core` の `ShioriExtension` / `SetupStep` / `IExtensionContext`、
`shiori/docs/adr/0002-extension-api-in-core.md`）に乗る。このパッケージは UI を持たない。
`VRChatExtension` が文言とボタンを返し、描画はコアが行う。

- `Editor/VRChatExtension.cs`: `ShioriExtension` の実装。コアが `TypeCache` で見つける。
- `Editor/Setup/VpmIgnoreStep.cs`: ウィザードの手順「VRChat の設定」。
  `Packages/vpm-manifest.json` の `locked`（無ければ `dependencies`）からパッケージ名を読み、
  `.gitignore` の `shiori-vrchat` ブロックに `Packages/<id>/` を 1 行ずつ書く。
  一覧は評価のたびに作り直すので、パッケージを足すと「更新する」が出る。
  VPM マニフェストが無いプロジェクトでは「VCC で管理していない」と表示して完了扱い。
- `Editor/Vpm/`: マニフェストの読み取りとブロック内容の組み立て（Unity 非依存）。
- `Editor/BuildTargetInfo.cs`: ビルドターゲットの表示名（PC / Android / iOS）。かんたんモード上部の 1 行に使う。
- `Editor/Localization/VRChatStrings.cs`: ja のテーブル。en はキーをそのまま返す（コアと同じ方針）。
- 文字列の読み書きが必要な JSON はコアの `MiniJson`（public）を使う。別の JSON ライブラリを足さない。

## 守ること

- **VRChat SDK のアセンブリを参照しない。** `vpmDependencies` に `com.vrchat.base` はあるが、
  コードは Unity 標準 API とコアだけで動く。検証プロジェクトにも SDK は入れない。
- ユーザーのプロジェクトに書くのは `IExtensionContext` 経由の `.gitignore` ブロックと
  `ProjectSettings/Shiori.json` の自分の節だけ。直接 `File.Write` しない。
- `Shiori.Core` / `Shiori.Editor` の internal には触らない（`InternalsVisibleTo` を頼まない）。
- アセンブリは `Shiori.VRChat.Editor` と `Shiori.VRChat.Editor.Tests` のみ。

## ビルドと検証

検証用プロジェクトはコアと共用（`../shiori-dev/<stream>`）。コアの `New-DevProject.ps1` で作ったあと、
このリポジトリの `Tools~/Add-ToDevProject.ps1` で `file:` 参照と `testables` を足す。

```
pwsh Tools~/Add-ToDevProject.ps1 -Stream 2022.3          # 初回だけ（2022.3-batch / 6000.6 も同様）
pwsh ../shiori/Tools~/Test-DevProject.ps1 -UnityVersion 2022.3.22f1 -ProjectPath ../shiori-dev/2022.3-batch
pwsh ../shiori/Tools~/Test-DevProject.ps1 -UnityVersion 6000.6.0f1
```

テストはコアと一緒に走る（結果の総数にはコアの分も含まれる）。
コミット前にインストール済み全バージョンで通すのはコアと同じ。
`.meta` は Unity が生成したものをコミットする（配布物は埋め込みコピーになるため必要）。

## 依存関係と配布

- `vpmDependencies`: `com.yaito3014.shiori` と `com.vrchat.base`。UPM の `dependencies` には書かない
  （どちらもレジストリに無いので UPM が解決できない）。
- 配布は VPM リスティング（GitHub Pages の `index.json`）。コアも同じリスティングに載せる。
  公開手順は未決定。
- CI はまだ無い。コアのリポジトリを取得して一緒にプロジェクトへ埋め込む形になる見込み。

## リポジトリ構成

```
shiori-vrchat/
  package.json
  README.md / CHANGELOG.md / LICENSE（MIT）
  Editor/
    Shiori.VRChat.Editor.asmdef + csc.rsp
    VRChatExtension.cs / BuildTargetInfo.cs
    Setup/ Vpm/ Localization/
  Tests/Editor/
    Shiori.VRChat.Editor.Tests.asmdef + csc.rsp
    Fakes/FakeExtensionContext.cs       # IExtensionContext のメモリ実装。Unity なしで手順をテストする
  Tools~/Add-ToDevProject.ps1
```

## まだ決まっていないこと（勝手に決めない）

- LFS を必須にするか推奨に留めるか（今は何も出さない。コアと同じく検出と案内のみ）
- VPM リスティングの公開手順と CI の形
- Android ターゲット向けに名前以外に何を表示するか
- `com.vrchat.base` の要求バージョン範囲（今は `>=3.5.0`）
