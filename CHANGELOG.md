# 更新履歴 / Changelog

リリースごとに、使う人向けの言葉で書いています。開発者向けの詳細は各リリースの GitHub Releases にあります。
Each release is described for the people who use it. The developer-level detail is in the GitHub release notes.

## [0.1.0] - 2026-10-09

### 日本語

- **はじめの設定に「VRChat の設定」が増えます。** VCC / ALCOM が入れたパッケージ（`Packages/` の中）を
  履歴に含めない設定にします。一覧は `vpm-manifest.json` から作るので、プロジェクトごとに合った内容になります。
  パッケージを足したあとは、かんたんモードの上部に「更新する」ボタン付きで知らせが出ます。
- **ウィンドウ右上に今のビルドターゲットが出ます**（「PC 向け」「Android 向け」）。
- **「戻す」の確認に、読み込み直しの注意が出ます。** Android 向けのときは時間がかかることも書きます。
- **空のメモ欄に例文が出ます**（「例: 衣装を追加、表情を調整、アップロード前」）。
- VRChat SDK には依存しません。SDK が入っていないプロジェクトでも動きます。

### English

- **A new setup step, 「VRChat の設定」**, keeps the packages installed by VCC / ALCOM (under
  `Packages/`) out of the history. The list comes from `vpm-manifest.json`, so it matches each
  project. After adding a package, a notice with an 更新する button appears at the top of simple mode.
- **The current build target** shows as a chip in the window header (PC / Android).
- **The 戻す confirmation warns about the re-import**, with a longer note when the target is Android.
- **The empty memo field shows example memos** for avatar and world work.
- No dependency on the VRChat SDK; the package also works in a project without it.
